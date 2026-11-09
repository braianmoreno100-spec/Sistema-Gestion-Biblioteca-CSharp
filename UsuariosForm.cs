
using System;
using System.Drawing;
using System.Windows.Forms;
using Biblioteca.Business.Services;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Presentation
{
    public class UsuariosForm : Form
    {
        private readonly UsuarioService servicio = new();

        private readonly TextBox txtNombre = new();
        private readonly TextBox txtApellido = new();
        private readonly TextBox txtCorreo = new();
        private readonly TextBox txtTelefono = new();

        private readonly DataGridView tabla = new();
        private int usuarioID = 0;

        public UsuariosForm()
        {
            Text = "Gestión de Usuarios";
            Size = new Size(900, 600);
            MinimumSize = new Size(750, 480);
            StartPosition = FormStartPosition.CenterScreen;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 210,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(15)
            };

            panel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 110));
            panel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            AgregarCampo(panel, "Nombre:", txtNombre, 0);
            AgregarCampo(panel, "Apellido:", txtApellido, 1);
            AgregarCampo(panel, "Correo:", txtCorreo, 2);
            AgregarCampo(panel, "Teléfono:", txtTelefono, 3);

            var botones = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 55,
                Padding = new Padding(15, 5, 0, 0)
            };

            var guardar = new Button
            {
                Text = "GUARDAR",
                Width = 110
            };

            var editar = new Button
            {
                Text = "EDITAR",
                Width = 110
            };

            var eliminar = new Button
            {
                Text = "ELIMINAR",
                Width = 110
            };

            var limpiar = new Button
            {
                Text = "LIMPIAR",
                Width = 110
            };

            guardar.Click += (s, e) => Ejecutar(() =>
            {
                servicio.RegistrarUsuario(LeerUsuario());
                MessageBox.Show("Usuario registrado.");
                Recargar();
            });

            editar.Click += (s, e) => Ejecutar(() =>
            {
                if (usuarioID <= 0)
                    throw new Exception("Selecciona un usuario.");

                servicio.ActualizarUsuario(LeerUsuario());
                MessageBox.Show("Usuario actualizado.");
                Recargar();
            });

            eliminar.Click += (s, e) => Ejecutar(() =>
            {
                if (usuarioID <= 0)
                    throw new Exception("Selecciona un usuario.");

                if (MessageBox.Show(
                    "¿Eliminar este usuario?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                servicio.EliminarUsuario(usuarioID);
                MessageBox.Show("Usuario eliminado.");
                Recargar();
            });

            limpiar.Click += (s, e) => Limpiar();

            botones.Controls.AddRange(new Control[]
            {
                guardar, editar, eliminar, limpiar
            });

            tabla.Dock = DockStyle.Fill;
            tabla.ReadOnly = true;
            tabla.AllowUserToAddRows = false;
            tabla.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            tabla.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            tabla.MultiSelect = false;

            tabla.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0)
                    return;

                if (tabla.Rows[e.RowIndex].DataBoundItem
                    is not Usuario usuario)
                    return;

                usuarioID = usuario.UsuarioID;
                txtNombre.Text = usuario.Nombre;
                txtApellido.Text = usuario.Apellido;
                txtCorreo.Text = usuario.Correo;
                txtTelefono.Text = usuario.Telefono ?? "";
            };

            Controls.Add(tabla);
            Controls.Add(botones);
            Controls.Add(panel);

            Load += (s, e) => Ejecutar(Recargar);
        }

        private static void AgregarCampo(
            TableLayoutPanel panel,
            string etiqueta,
            TextBox campo,
            int fila)
        {
            panel.Controls.Add(new Label
            {
                Text = etiqueta,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            }, 0, fila);

            campo.Dock = DockStyle.Fill;
            panel.Controls.Add(campo, 1, fila);
        }

        private Usuario LeerUsuario()
        {
            return new Usuario
            {
                UsuarioID = usuarioID,
                Nombre = txtNombre.Text.Trim(),
                Apellido = txtApellido.Text.Trim(),
                Correo = txtCorreo.Text.Trim(),
                Telefono = txtTelefono.Text.Trim()
            };
        }

        private void Recargar()
        {
            tabla.DataSource = null;
            tabla.DataSource = servicio.ObtenerUsuarios();
            Limpiar();
        }

        private void Limpiar()
        {
            usuarioID = 0;
            txtNombre.Clear();
            txtApellido.Clear();
            txtCorreo.Clear();
            txtTelefono.Clear();
            tabla.ClearSelection();
        }

        private static void Ejecutar(Action accion)
        {
            try
            {
                accion();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}
