
using Biblioteca.Data.Repositories;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Business.Services
{
    public class EditorialService
    {
        private readonly EditorialRepository _repository = new();

        public List<Editorial> ObtenerEditoriales()
        {
            return _repository.ObtenerTodos();
        }

        public void RegistrarEditorial(Editorial editorial)
        {
            ValidarEditorial(editorial);
            _repository.Agregar(editorial);
        }

        public void ActualizarEditorial(Editorial editorial)
        {
            if (editorial.EditorialID <= 0)
                throw new Exception("Selecciona una editorial válida.");

            ValidarEditorial(editorial);
            _repository.Actualizar(editorial);
        }

        public void EliminarEditorial(int id)
        {
            if (id <= 0)
                throw new Exception("Selecciona una editorial válida.");

            _repository.Eliminar(id);
        }

        private static void ValidarEditorial(Editorial editorial)
        {
            if (string.IsNullOrWhiteSpace(editorial.Nombre))
                throw new Exception("El nombre es obligatorio.");

            if (editorial.Nombre.Length > 150)
                throw new Exception(
                    "El nombre no puede superar 150 caracteres.");

            if (editorial.Pais?.Length > 80)
                throw new Exception(
                    "El país no puede superar 80 caracteres.");
        }
    }
}
