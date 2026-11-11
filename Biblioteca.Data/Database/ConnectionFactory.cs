
using Microsoft.Data.SqlClient;

namespace Biblioteca.Data.Database
{
    public static class ConnectionFactory
    {
        private static readonly string connectionString =
            "Server=localhost;Database=BibliotecaDB;Integrated Security=True;TrustServerCertificate=True;";

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
