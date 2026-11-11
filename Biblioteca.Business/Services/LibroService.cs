
using System;
using System.Collections.Generic;
using System.Linq;
using Biblioteca.Data.Repositories;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Business.Services
{
    public class LibroService
    {
        private readonly LibroRepository _repository = new();
        private readonly LibroAutorRepository _libroAutorRepository = new();

        public List<Libro> ObtenerLibros()
        {
            return _repository.ObtenerTodos();
        }

        public List<Libro> ObtenerLibrosPorAutor(int autorID)
        {
            if (autorID <= 0)
                throw new Exception("Selecciona un autor válido.");

            return _repository.ObtenerPorAutor(autorID);
        }

        public List<int> ObtenerAutoresPorLibro(int libroID)
        {
            if (libroID <= 0)
                throw new Exception("Selecciona un libro válido.");

            return _libroAutorRepository.ObtenerAutoresPorLibro(libroID);
        }

        public void GuardarAutoresDeLibro(
            int libroID,
            IEnumerable<int> autoresIDs)
        {
            if (libroID <= 0)
                throw new Exception("Selecciona un libro válido.");

            var ids = ValidarAutores(autoresIDs);

            _libroAutorRepository.GuardarAutores(libroID, ids);
        }

        // Compatibilidad con el resto del sistema.
        public void RegistrarLibro(Libro libro)
        {
            ValidarLibro(libro);
            _repository.Agregar(libro);
        }

        // REGISTRO ATÓMICO
        public int RegistrarLibroConAutores(
            Libro libro,
            IEnumerable<int> autoresIDs)
        {
            ValidarLibro(libro);
            var ids = ValidarAutores(autoresIDs);

            return _repository.AgregarConAutores(libro, ids);
        }

        public void ActualizarLibro(Libro libro)
        {
            if (libro == null || libro.LibroID <= 0)
                throw new Exception("Selecciona un libro válido.");

            ValidarLibro(libro);
            _repository.Actualizar(libro);
        }

        // ACTUALIZACIÓN ATÓMICA
        public void ActualizarLibroConAutores(
            Libro libro,
            IEnumerable<int> autoresIDs)
        {
            if (libro == null || libro.LibroID <= 0)
                throw new Exception("Selecciona un libro válido.");

            ValidarLibro(libro);
            var ids = ValidarAutores(autoresIDs);

            _repository.ActualizarConAutores(libro, ids);
        }

        public void EliminarLibro(int id)
        {
            if (id <= 0)
                throw new Exception("Selecciona un libro válido.");

            _repository.Eliminar(id);
        }

        private static List<int> ValidarAutores(
            IEnumerable<int> autoresIDs)
        {
            if (autoresIDs == null)
                throw new Exception(
                    "Debes seleccionar al menos un autor.");

            var ids = autoresIDs.Distinct().ToList();

            if (ids.Count == 0 || ids.Any(id => id <= 0))
                throw new Exception(
                    "Selecciona al menos un autor válido.");

            return ids;
        }

        private static void ValidarLibro(Libro libro)
        {
            if (libro == null)
                throw new Exception(
                    "Los datos del libro son obligatorios.");

            if (string.IsNullOrWhiteSpace(libro.Titulo))
                throw new Exception("El título es obligatorio.");

            if (string.IsNullOrWhiteSpace(libro.ISBN))
                throw new Exception("El ISBN es obligatorio.");

            if (libro.ISBN.Length > 20)
                throw new Exception(
                    "El ISBN no puede superar 20 caracteres.");

            if (libro.EditorialID <= 0)
                throw new Exception(
                    "Selecciona una editorial válida.");

            if (libro.TotalEjemplares < 1)
                throw new Exception(
                    "Debe existir al menos un ejemplar.");

            if (libro.EjemplaresDisponibles < 0 ||
                libro.EjemplaresDisponibles > libro.TotalEjemplares)
                throw new Exception(
                    "La cantidad de ejemplares no es válida.");

            if (libro.AnioPublicacion.HasValue &&
                (libro.AnioPublicacion.Value < 1 ||
                 libro.AnioPublicacion.Value > DateTime.Now.Year))
                throw new Exception(
                    "El año de publicación no es válido.");
        }
    }
}
