
using Biblioteca.Data.Database;
using Biblioteca.Domain.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Biblioteca.Data.Repositories
{
    public class UsuarioRepository
    {
        public List<Usuario> ObtenerTodos()
        {
            var usuarios = new List<Usuario>();

            using var conexion = ConnectionFactory.CreateConnection();
            conexion.Open();

            const string sql = @"
                SELECT UsuarioID, Nombre, Apellido, Correo,
                       Telefono, FechaRegistro
                FROM Usuarios
                ORDER BY UsuarioID";

            using var comando = new SqlCommand(sql, conexion);
            using var lector = comando.ExecuteReader();

            while (lector.Read())
            {
                usuarios.Add(new Usuario
                {
                    UsuarioID = lector.GetInt32(0),
                    Nombre = lector.GetString(1),
                    Apellido = lector.GetString(2),
                    Correo = lector.GetString(3),
                    Telefono = lector.IsDBNull(4)
                        ? null : lector.GetString(4),
                    FechaRegistro = lector.GetDateTime(5)
                });
            }

            return usuarios;
        }

        public void Agregar(Usuario usuario)
        {
            using var conexion = ConnectionFactory.CreateConnection();
            conexion.Open();

            const string sql = @"
                INSERT INTO Usuarios
                    (Nombre, Apellido, Correo, Telefono)
                VALUES
                    (@Nombre, @Apellido, @Correo, @Telefono)";

            using var comando = new SqlCommand(sql, conexion);
            AgregarParametros(comando, usuario);
            comando.ExecuteNonQuery();
        }

        public void Actualizar(Usuario usuario)
        {
            using var conexion = ConnectionFactory.CreateConnection();
            conexion.Open();

            const string sql = @"
                UPDATE Usuarios
                SET Nombre = @Nombre,
                    Apellido = @Apellido,
                    Correo = @Correo,
                    Telefono = @Telefono
                WHERE UsuarioID = @ID";

            using var comando = new SqlCommand(sql, conexion);
            AgregarParametros(comando, usuario);

            comando.Parameters.Add("@ID", SqlDbType.Int).Value =
                usuario.UsuarioID;

            if (comando.ExecuteNonQuery() == 0)
                throw new Exception("No se encontró el usuario.");
        }

        public void Eliminar(int id)
        {
            using var conexion = ConnectionFactory.CreateConnection();
            conexion.Open();

            const string sql =
                "DELETE FROM Usuarios WHERE UsuarioID = @ID";

            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            if (comando.ExecuteNonQuery() == 0)
                throw new Exception("No se encontró el usuario.");
        }

        private static void AgregarParametros(
            SqlCommand comando, Usuario usuario)
        {
            comando.Parameters.Add(
                "@Nombre", SqlDbType.NVarChar, 100).Value =
                usuario.Nombre;

            comando.Parameters.Add(
                "@Apellido", SqlDbType.NVarChar, 100).Value =
                usuario.Apellido;

            comando.Parameters.Add(
                "@Correo", SqlDbType.NVarChar, 150).Value =
                usuario.Correo;

            comando.Parameters.Add(
                "@Telefono", SqlDbType.NVarChar, 20).Value =
                string.IsNullOrWhiteSpace(usuario.Telefono)
                    ? DBNull.Value
                    : usuario.Telefono;
        }
    }
}
