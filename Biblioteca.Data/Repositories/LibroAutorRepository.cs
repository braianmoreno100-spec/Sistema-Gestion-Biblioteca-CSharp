
using System;
using System.Collections.Generic;
using System.Data;
using Biblioteca.Data.Database;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Data.Repositories
{
    public class LibroAutorRepository
    {
        // CONSULTAR LOS AUTORES DE UN LIBRO
        public List<int> ObtenerAutoresPorLibro(int libroID)
        {
            var autores = new List<int>();

            using var connection =
                ConnectionFactory.CreateConnection();

            connection.Open();

            const string sql = @"
                SELECT AutorID
                FROM LibroAutor
                WHERE LibroID = @LibroID
                ORDER BY AutorID";

            using var command =
                new SqlCommand(sql, connection);

            command.Parameters.Add(
                "@LibroID", SqlDbType.Int).Value = libroID;

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                autores.Add(reader.GetInt32(0));
            }

            return autores;
        }

        // GUARDAR O ACTUALIZAR LOS AUTORES DE UN LIBRO
        public void GuardarAutores(
            int libroID,
            IEnumerable<int> autoresIDs)
        {
            if (libroID <= 0)
                throw new Exception(
                    "Selecciona un libro válido.");

            if (autoresIDs == null)
                throw new Exception(
                    "Debes indicar los autores del libro.");

            var ids = new HashSet<int>(autoresIDs);

            if (ids.Count == 0 || ids.Contains(0) ||
                ids.Any(id => id < 0))
            {
                throw new Exception(
                    "Selecciona al menos un autor válido.");
            }

            using var connection =
                ConnectionFactory.CreateConnection();

            connection.Open();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                // Eliminar las asociaciones anteriores.
                const string eliminarSql = @"
                    DELETE FROM LibroAutor
                    WHERE LibroID = @LibroID";

                using (var eliminar = new SqlCommand(
                    eliminarSql, connection, transaction))
                {
                    eliminar.Parameters.Add(
                        "@LibroID", SqlDbType.Int).Value =
                        libroID;

                    eliminar.ExecuteNonQuery();
                }

                // Registrar los autores seleccionados.
                const string insertarSql = @"
                    INSERT INTO LibroAutor
                        (LibroID, AutorID)
                    VALUES
                        (@LibroID, @AutorID)";

                foreach (int autorID in ids)
                {
                    using var insertar = new SqlCommand(
                        insertarSql, connection, transaction);

                    insertar.Parameters.Add(
                        "@LibroID", SqlDbType.Int).Value =
                        libroID;

                    insertar.Parameters.Add(
                        "@AutorID", SqlDbType.Int).Value =
                        autorID;

                    insertar.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
