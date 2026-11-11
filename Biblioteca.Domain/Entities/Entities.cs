
namespace Biblioteca.Domain.Entities
{
    public class Autor
    {
        public int AutorID { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string? Pais { get; set; }
    }

    public class Editorial
    {
        public int EditorialID { get; set; }
        public string Nombre { get; set; } = "";
        public string? Pais { get; set; }
    }

    public class Usuario
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Correo { get; set; } = "";
        public string? Telefono { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class Libro
    {
        public int LibroID { get; set; }
        public string Titulo { get; set; } = "";
        public string ISBN { get; set; } = "";
        public int EditorialID { get; set; }
        public int? AnioPublicacion { get; set; }
        public int TotalEjemplares { get; set; }
        public int EjemplaresDisponibles { get; set; }
    }

    public class Prestamo
    {
        public int PrestamoID { get; set; }
        public int UsuarioID { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaVencimiento { get; set; }
    }

    public class DetallePrestamo
    {
        public int DetalleID { get; set; }
        public int PrestamoID { get; set; }
        public int LibroID { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public string Estado { get; set; } = "Activo";
    }
}
