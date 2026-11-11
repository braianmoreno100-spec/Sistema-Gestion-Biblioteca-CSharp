
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Biblioteca.Data.Database;
using Biblioteca.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Data.Repositories
{
    public class LibroRepository
    {
        public List<Libro> ObtenerTodos()
        {
            var libros = new List<Libro>();

            using var connection = ConnectionFactory.CreateConnection();
            connection.Open();

            const string sql = @"
                SELECT LibroID, Titulo, ISBN, EditorialID,
                       AnioPublicacion, TotalEjemplares,
                       EjemplaresDisponibles
                FROM Libros
                ORDER BY LibroID";

            using var command = new SqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
                libros.Add(LeerLibro(reader));

            return libros;
        }

        public List<Libro> ObtenerPorAutor(int autorID)
        {
            var libros = new List<Libro>();

            using var connection = ConnectionFactory.CreateConnection();
            connection.Open();

            const string sql = @"
                SELECT l.LibroID, l.Titulo, l.ISBN,
                       l.EditorialID, l.AnioPublicacion,
                       l.TotalEjemplares, l.EjemplaresDisponibles
                FROM Libros l
                INNER JOIN LibroAutor la
                    ON l.LibroID = la.LibroID
                WHERE la.AutorID = @AutorID
                ORDER BY l.Titulo";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@AutorID", SqlDbType.Int).Value =
                autorID;

            using var reader = command.ExecuteReader();

            while (reader.Read())
                libros.Add(LeerLibro(reader));

            return libros;
        }

        // Conserva compatibilidad con otras llamadas existentes.
        public int Agregar(Libro libro)
        {
            using var connection = ConnectionFactory.CreateConnection();
            connection.Open();

            using var command = new SqlCommand(
                SqlInsertarLibro, connection);

            AgregarParametros(command, libro);

            int id = Convert.ToInt32(command.ExecuteScalar());
            libro.LibroID = id;

            return id;
        }

        // NUEVO: INSERTAR LIBRO Y AUTORES ATÓMICAMENTE
        public int AgregarConAutores(
            Libro libro,
            IEnumerable<int> autoresIDs)
        {
            var ids = ValidarIDs(autoresIDs);

            using var connection = ConnectionFactory.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = new SqlCommand(
                    SqlInsertarLibro, connection, transaction);

                AgregarParametros(command, libro);

                int nuevoID =
                    Convert.ToInt32(command.ExecuteScalar());

                InsertarAutores(
                    connection, transaction, nuevoID, ids);

                transaction.Commit();

                libro.LibroID = nuevoID;
                return nuevoID;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void Actualizar(Libro libro)
        {
            using var connection = ConnectionFactory.CreateConnection();
            connection.Open();

            using var command = new SqlCommand(
                SqlActualizarLibro, connection);

            AgregarParametros(command, libro);

            command.Parameters.Add("@ID", SqlDbType.Int).Value =
                libro.LibroID;

            if (command.ExecuteNonQuery() == 0)
                throw new Exception(
                    "No se encontró el libro que deseas actualizar.");
        }

        // NUEVO: ACTUALIZAR LIBRO Y AUTORES ATÓMICAMENTE
        public void ActualizarConAutores(
            Libro libro,
            IEnumerable<int> autoresIDs)
        {
            var ids = ValidarIDs(autoresIDs);

            using var connection = ConnectionFactory.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                using (var command = new SqlCommand(
                    SqlActualizarLibro, connection, transaction))
                {
                    AgregarParametros(command, libro);

                    command.Parameters.Add(
                        "@ID", SqlDbType.Int).Value = libro.LibroID;

                    if (command.ExecuteNonQuery() == 0)
                        throw new Exception(
                            "No se encontró el libro que deseas actualizar.");
                }

                using (var eliminar = new SqlCommand(
                    "DELETE FROM LibroAutor WHERE LibroID = @ID",
                    connection, transaction))
                {
                    eliminar.Parameters.Add(
                        "@ID", SqlDbType.Int).Value = libro.LibroID;

                    eliminar.ExecuteNonQuery();
                }

                InsertarAutores(
                    connection, transaction, libro.LibroID, ids);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void Eliminar(int libroID)
        {
            using var connection = ConnectionFactory.CreateConnection();
            connection.Open();

            const string sql =
                "DELETE FROM Libros WHERE LibroID = @ID";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@ID", SqlDbType.Int).Value =
                libroID;

            if (command.ExecuteNonQuery() == 0)
                throw new Exception("No se encontró el libro.");
        }

        private static void InsertarAutores(
            SqlConnection connection,
            SqlTransaction transaction,
            int libroID,
            List<int> ids)
        {
            const string sql = @"
                INSERT INTO LibroAutor (LibroID, AutorID)
                VALUES (@LibroID, @AutorID)";

            foreach (int autorID in ids)
            {
                using var command = new SqlCommand(
                    sql, connection, transaction);

                command.Parameters.Add(
                    "@LibroID", SqlDbType.Int).Value = libroID;

                command.Parameters.Add(
                    "@AutorID", SqlDbType.Int).Value = autorID;

                command.ExecuteNonQuery();
            }
        }

        private static List<int> ValidarIDs(
            IEnumerable<int> autoresIDs)
        {
            if (autoresIDs == null)
                throw new Exception(
                    "Debes seleccionar al menos un autor.");

            var ids = autoresIDs.Distinct().ToList();

            if (ids.Count == 0 || ids.Any(id => id <= 0))
                throw new Exception(
                    "Selecciona al menos un autor válido.");

            return ids;
        }

        private const string SqlInsertarLibro = @"
            INSERT INTO Libros
                (Titulo, ISBN, EditorialID,
                 AnioPublicacion, TotalEjemplares,
                 EjemplaresDisponibles)
            OUTPUT INSERTED.LibroID
            VALUES
                (@Titulo, @ISBN, @EditorialID,
                 @Anio, @Total, @Disponibles)";

        private const string SqlActualizarLibro = @"
            UPDATE Libros
            SET Titulo = @Titulo,
                ISBN = @ISBN,
                EditorialID = @EditorialID,
                AnioPublicacion = @Anio,
                TotalEjemplares = @Total,
                EjemplaresDisponibles = @Disponibles
            WHERE LibroID = @ID";

        private static Libro LeerLibro(SqlDataReader reader)
        {
            return new Libro
            {
                LibroID = reader.GetInt32(0),
                Titulo = reader.GetString(1),
                ISBN = reader.GetString(2),
                EditorialID = reader.GetInt32(3),
                AnioPublicacion = reader.IsDBNull(4)
                    ? null : reader.GetInt32(4),
                TotalEjemplares = reader.GetInt32(5),
                EjemplaresDisponibles = reader.GetInt32(6)
            };
        }

        private static void AgregarParametros(
            SqlCommand command,
            Libro libro)
        {
            command.Parameters.Add(
                "@Titulo", SqlDbType.NVarChar, 200).Value =
                libro.Titulo;

            command.Parameters.Add(
                "@ISBN", SqlDbType.NVarChar, 20).Value =
                libro.ISBN;

            command.Parameters.Add(
                "@EditorialID", SqlDbType.Int).Value =
                libro.EditorialID;

            command.Parameters.Add(
                "@Anio", SqlDbType.Int).Value =
                (object?)libro.AnioPublicacion ?? DBNull.Value;

            command.Parameters.Add(
                "@Total", SqlDbType.Int).Value =
                libro.TotalEjemplares;

            command.Parameters.Add(
                "@Disponibles", SqlDbType.Int).Value =
                libro.EjemplaresDisponibles;
        }
    }
}
