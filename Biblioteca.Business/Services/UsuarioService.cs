
using Biblioteca.Data.Repositories;
using Biblioteca.Domain.Entities;
using System.Net.Mail;

namespace Biblioteca.Business.Services
{
    public class UsuarioService
    {
        private readonly UsuarioRepository _repository = new();

        public List<Usuario> ObtenerUsuarios()
        {
            return _repository.ObtenerTodos();
        }

        public void RegistrarUsuario(Usuario usuario)
        {
            ValidarUsuario(usuario);
            _repository.Agregar(usuario);
        }

        public void ActualizarUsuario(Usuario usuario)
        {
            if (usuario.UsuarioID <= 0)
                throw new Exception("Selecciona un usuario válido.");

            ValidarUsuario(usuario);
            _repository.Actualizar(usuario);
        }

        public void EliminarUsuario(int id)
        {
            if (id <= 0)
                throw new Exception("Selecciona un usuario válido.");

            _repository.Eliminar(id);
        }

        private static void ValidarUsuario(Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Nombre) ||
                string.IsNullOrWhiteSpace(usuario.Apellido))
                throw new Exception(
                    "El nombre y apellido son obligatorios.");

            if (usuario.Nombre.Length > 100 ||
                usuario.Apellido.Length > 100)
                throw new Exception(
                    "El nombre o apellido es demasiado largo.");

            if (string.IsNullOrWhiteSpace(usuario.Correo))
                throw new Exception("El correo es obligatorio.");

            if (usuario.Correo.Length > 150 ||
                !MailAddress.TryCreate(usuario.Correo, out var correo) ||
                correo.Address != usuario.Correo ||
                !correo.Host.Contains('.'))
                throw new Exception("El correo no es válido.");

            if (usuario.Telefono?.Length > 20)
                throw new Exception(
                    "El teléfono no puede superar 20 caracteres.");
        }
    }
}
