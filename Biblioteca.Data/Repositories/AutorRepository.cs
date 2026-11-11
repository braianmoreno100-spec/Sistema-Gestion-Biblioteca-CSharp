
using Biblioteca.Data.Database;
using Biblioteca.Domain.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Biblioteca.Data.Repositories
{
    public class AutorRepository
    {
        public List<Autor> ObtenerTodos()
        {
            var autores = new List<Autor>();

            using var connection = ConnectionFactory.CreateConnection();
            connection.Open();

            const string sql = @"
                SELECT AutorID, Nombre, Apellido, Pais
                FROM Autores
                ORDER BY AutorID";

            using var command = new SqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                autores.Add(new Autor
                {
                    AutorID = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Apellido = reader.GetString(2),
                    Pais = reader.IsDBNull(3)
                        ? null : reader.GetString(3)
                });
            }

            return autores;
        }

        public void Agregar(Autor autor)
        {
            using var connection = ConnectionFactory.CreateConnection();
            connection.Open();

            const string sql = @"
                INSERT INTO Autores (Nombre, Apellido, Pais)
                VALUES (@Nombre, @Apellido, @Pais)";

            using var command = new SqlCommand(sql, connection);
            AgregarParametros(command, autor);
            command.ExecuteNonQuery();
        }

        public void Actualizar(Autor autor)
        {
            using var connection = ConnectionFactory.CreateConnection();
            connection.Open();

            const string sql = @"
                UPDATE Autores
                SET Nombre = @Nombre,
                    Apellido = @Apellido,
                    Pais = @Pais
                WHERE AutorID = @ID";

            using var command = new SqlCommand(sql, connection);
            AgregarParametros(command, autor);

            command.Parameters.Add("@ID", SqlDbType.Int).Value =
                autor.AutorID;

            if (command.ExecuteNonQuery() == 0)
                throw new Exception("No se encontró el autor.");
        }

        public void Eliminar(int autorID)
        {
            using var connection = ConnectionFactory.CreateConnection();
            connection.Open();

            const string sql =
                "DELETE FROM Autores WHERE AutorID = @ID";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@ID", SqlDbType.Int).Value =
                autorID;

            if (command.ExecuteNonQuery() == 0)
                throw new Exception("No se encontró el autor.");
        }

        private static void AgregarParametros(
            SqlCommand command, Autor autor)
        {
            command.Parameters.Add(
                "@Nombre", SqlDbType.NVarChar, 100).Value =
                autor.Nombre;

            command.Parameters.Add(
                "@Apellido", SqlDbType.NVarChar, 100).Value =
                autor.Apellido;

            command.Parameters.Add(
                "@Pais", SqlDbType.NVarChar, 80).Value =
                string.IsNullOrWhiteSpace(autor.Pais)
                    ? DBNull.Value
                    : autor.Pais;
        }
    }
}

