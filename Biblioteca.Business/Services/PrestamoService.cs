
using System;
using System.Data;
using Biblioteca.Data.Repositories;

namespace Biblioteca.Business.Services
{
    public class PrestamoService
    {
        private readonly PrestamoRepository _repository = new();

        public void RegistrarPrestamo(
            int usuarioID,
            int libroID,
            DateTime fechaVencimiento)
        {
            if (usuarioID <= 0)
                throw new Exception(
                    "Debes seleccionar un usuario válido.");

            if (libroID <= 0)
                throw new Exception(
                    "Debes seleccionar un libro válido.");

            if (fechaVencimiento.Date <= DateTime.Today)
                throw new Exception(
                    "La fecha de vencimiento debe ser posterior a hoy.");

            _repository.RegistrarPrestamo(
                usuarioID,
                libroID,
                fechaVencimiento.Date);
        }

        public void RegistrarDevolucion(int detalleID)
        {
            if (detalleID <= 0)
                throw new Exception(
                    "Selecciona un préstamo válido.");

            _repository.RegistrarDevolucion(detalleID);
        }

        public DataTable ObtenerPrestamos()
        {
            return _repository.ObtenerPrestamos();
        }
    }
}
