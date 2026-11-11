
using Biblioteca.Data.Repositories;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Business.Services
{
    public class AutorService
    {
        private readonly AutorRepository _repository = new();

        public List<Autor> ObtenerAutores()
        {
            return _repository.ObtenerTodos();
        }

        public void RegistrarAutor(Autor autor)
        {
            ValidarAutor(autor);
            _repository.Agregar(autor);
        }

        public void ActualizarAutor(Autor autor)
        {
            if (autor.AutorID <= 0)
                throw new Exception("Selecciona un autor válido.");

            ValidarAutor(autor);
            _repository.Actualizar(autor);
        }

        public void EliminarAutor(int id)
        {
            if (id <= 0)
                throw new Exception("Selecciona un autor válido.");

            _repository.Eliminar(id);
        }

        private static void ValidarAutor(Autor autor)
        {
            if (string.IsNullOrWhiteSpace(autor.Nombre))
                throw new Exception("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(autor.Apellido))
                throw new Exception("El apellido es obligatorio.");

            if (autor.Nombre.Length > 100 ||
                autor.Apellido.Length > 100)
                throw new Exception(
                    "El nombre o apellido supera 100 caracteres.");

            if (autor.Pais?.Length > 80)
                throw new Exception(
                    "El país no puede superar 80 caracteres.");
        }
    }
}
