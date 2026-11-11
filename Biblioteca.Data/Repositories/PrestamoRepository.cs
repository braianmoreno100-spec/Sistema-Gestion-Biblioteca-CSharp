
using Biblioteca.Data.Database;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Biblioteca.Data.Repositories
{
    public class PrestamoRepository
    {
        public void RegistrarPrestamo(
            int usuarioID, int libroID, DateTime vencimiento)
        {
            using var conexion = ConnectionFactory.CreateConnection();
            conexion.Open();

            using var transaccion = conexion.BeginTransaction();

            try
            {
                // Bloquear el libro mientras se comprueba disponibilidad.
                const string sqlDisponibilidad = @"
                    SELECT EjemplaresDisponibles
                    FROM Libros WITH (UPDLOCK, HOLDLOCK)
                    WHERE LibroID = @LibroID";

                using var comprobar = new SqlCommand(
                    sqlDisponibilidad, conexion, transaccion);

                comprobar.Parameters.Add(
                    "@LibroID", SqlDbType.Int).Value = libroID;

                object? resultado = comprobar.ExecuteScalar();

                if (resultado == null)
                    throw new Exception("El libro no existe.");

                if (Convert.ToInt32(resultado) <= 0)
                    throw new Exception("No hay ejemplares disponibles.");

                const string sqlPrestamo = @"
                    INSERT INTO Prestamos
                        (UsuarioID, FechaPrestamo, FechaVencimiento)
                    OUTPUT INSERTED.PrestamoID
                    VALUES (@UsuarioID, @Fecha, @Vencimiento)";

                using var prestamo = new SqlCommand(
                    sqlPrestamo, conexion, transaccion);

                prestamo.Parameters.Add(
                    "@UsuarioID", SqlDbType.Int).Value = usuarioID;

                prestamo.Parameters.Add(
                    "@Fecha", SqlDbType.Date).Value =
                    DateTime.Today;

                prestamo.Parameters.Add(
                    "@Vencimiento", SqlDbType.Date).Value =
                    vencimiento.Date;

                int prestamoID = Convert.ToInt32(
                    prestamo.ExecuteScalar());

                const string sqlDetalle = @"
                    INSERT INTO DetallePrestamo
                        (PrestamoID, LibroID, Estado)
                    VALUES (@PrestamoID, @LibroID, 'Activo')";

                using var detalle = new SqlCommand(
                    sqlDetalle, conexion, transaccion);

                detalle.Parameters.Add(
                    "@PrestamoID", SqlDbType.Int).Value =
                    prestamoID;

                detalle.Parameters.Add(
                    "@LibroID", SqlDbType.Int).Value =
                    libroID;

                detalle.ExecuteNonQuery();

                const string sqlActualizar = @"
                    UPDATE Libros
                    SET EjemplaresDisponibles =
                        EjemplaresDisponibles - 1
                    WHERE LibroID = @LibroID";

                using var actualizar = new SqlCommand(
                    sqlActualizar, conexion, transaccion);

                actualizar.Parameters.Add(
                    "@LibroID", SqlDbType.Int).Value =
                    libroID;

                actualizar.ExecuteNonQuery();

                transaccion.Commit();
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
        }

        public void RegistrarDevolucion(int detalleID)
        {
            using var conexion = ConnectionFactory.CreateConnection();
            conexion.Open();

            using var transaccion = conexion.BeginTransaction();

            try
            {
                const string sqlBuscar = @"
                    SELECT LibroID
                    FROM DetallePrestamo WITH (UPDLOCK, HOLDLOCK)
                    WHERE DetalleID = @DetalleID
                      AND Estado = 'Activo'";

                using var buscar = new SqlCommand(
                    sqlBuscar, conexion, transaccion);

                buscar.Parameters.Add(
                    "@DetalleID", SqlDbType.Int).Value =
                    detalleID;

                object? resultado = buscar.ExecuteScalar();

                if (resultado == null)
                    throw new Exception(
                        "No existe un préstamo activo con ese detalle.");

                int libroID = Convert.ToInt32(resultado);

                const string sqlDevolucion = @"
                    UPDATE DetallePrestamo
                    SET FechaDevolucion = @Fecha,
                        Estado = 'Devuelto'
                    WHERE DetalleID = @DetalleID
                      AND Estado = 'Activo'";

                using var devolver = new SqlCommand(
                    sqlDevolucion, conexion, transaccion);

                devolver.Parameters.Add(
                    "@Fecha", SqlDbType.Date).Value =
                    DateTime.Today;

                devolver.Parameters.Add(
                    "@DetalleID", SqlDbType.Int).Value =
                    detalleID;

                if (devolver.ExecuteNonQuery() != 1)
                    throw new Exception(
                        "No se pudo registrar la devolución.");

                const string sqlActualizar = @"
                    UPDATE Libros
                    SET EjemplaresDisponibles =
                        EjemplaresDisponibles + 1
                    WHERE LibroID = @LibroID
                      AND EjemplaresDisponibles < TotalEjemplares";

                using var actualizar = new SqlCommand(
                    sqlActualizar, conexion, transaccion);

                actualizar.Parameters.Add(
                    "@LibroID", SqlDbType.Int).Value =
                    libroID;

                if (actualizar.ExecuteNonQuery() != 1)
                    throw new Exception(
                        "No se pudo actualizar el inventario.");

                transaccion.Commit();
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
        }

        public DataTable ObtenerPrestamos()
        {
            using var conexion = ConnectionFactory.CreateConnection();
            conexion.Open();

            const string sql = @"
                SELECT
                    d.DetalleID,
                    p.PrestamoID,
                    u.Nombre + ' ' + u.Apellido AS Usuario,
                    l.Titulo AS Libro,
                    p.FechaPrestamo,
                    p.FechaVencimiento,
                    d.FechaDevolucion,
                    d.Estado
                FROM DetallePrestamo d
                INNER JOIN Prestamos p
                    ON d.PrestamoID = p.PrestamoID
                INNER JOIN Usuarios u
                    ON p.UsuarioID = u.UsuarioID
                INNER JOIN Libros l
                    ON d.LibroID = l.LibroID
                ORDER BY p.PrestamoID DESC";

            using var adaptador = new SqlDataAdapter(sql, conexion);

            var tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }
    }
}
