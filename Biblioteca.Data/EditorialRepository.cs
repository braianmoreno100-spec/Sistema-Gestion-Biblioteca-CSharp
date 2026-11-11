
using Biblioteca.Data.Database;
using Biblioteca.Domain.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Biblioteca.Data.Repositories
{
    public class EditorialRepository
    {
        public List<Editorial> ObtenerTodos()
        {
            var lista = new List<Editorial>();

            using var conexion = ConnectionFactory.CreateConnection();
            conexion.Open();

            const string sql =
                "SELECT EditorialID, Nombre, Pais FROM Editoriales ORDER BY EditorialID";

            using var comando = new SqlCommand(sql, conexion);
            using var lector = comando.ExecuteReader();

            while (lector.Read())
            {
                lista.Add(new Editorial
                {
                    EditorialID = lector.GetInt32(0),
                    Nombre = lector.GetString(1),
                    Pais = lector.IsDBNull(2) ? null : lector.GetString(2)
                });
            }

            return lista;
        }

        public void Agregar(Editorial editorial)
        {
            using var conexion = ConnectionFactory.CreateConnection();
            conexion.Open();

            const string sql =
                "INSERT INTO Editoriales (Nombre, Pais) VALUES (@Nombre, @Pais)";

            using var comando = new SqlCommand(sql, conexion);
            AgregarParametros(comando, editorial);
            comando.ExecuteNonQuery();
        }

        public void Actualizar(Editorial editorial)
        {
            using var conexion = ConnectionFactory.CreateConnection();
            conexion.Open();

            const string sql = @"UPDATE Editoriales
                                 SET Nombre = @Nombre, Pais = @Pais
                                 WHERE EditorialID = @ID";

            using var comando = new SqlCommand(sql, conexion);
            AgregarParametros(comando, editorial);
            comando.Parameters.Add("@ID", SqlDbType.Int).Value =
                editorial.EditorialID;

            if (comando.ExecuteNonQuery() == 0)
                throw new Exception("No se encontró la editorial.");
        }

        public void Eliminar(int id)
        {
            using var conexion = ConnectionFactory.CreateConnection();
            conexion.Open();

            const string sql =
                "DELETE FROM Editoriales WHERE EditorialID = @ID";

            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            if (comando.ExecuteNonQuery() == 0)
                throw new Exception("No se encontró la editorial.");
        }

        private static void AgregarParametros(
            SqlCommand comando, Editorial editorial)
        {
            comando.Parameters.Add(
                "@Nombre", SqlDbType.NVarChar, 150).Value =
                editorial.Nombre;

            comando.Parameters.Add(
                "@Pais", SqlDbType.NVarChar, 80).Value =
                string.IsNullOrWhiteSpace(editorial.Pais)
                    ? DBNull.Value
                    : editorial.Pais;
        }
    }
}
