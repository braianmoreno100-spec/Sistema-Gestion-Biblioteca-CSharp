
using System;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
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
        private readonly TextBox txtBuscar = new();

        private readonly DataGridView tabla = new();
        private readonly Label lblResultados = new();

        private readonly Color azul =
            Color.FromArgb(38, 78, 133);

        private readonly Color fondo =
            Color.FromArgb(245, 248, 252);

        private int usuarioID = 0;
        private bool cargando = false;

        public UsuariosForm()
        {
            Text = "Biblioteca | Gestión de Usuarios";
            Size = new Size(1150, 790);
            MinimumSize = new Size(900, 680);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = fondo;
            Font = new Font("Segoe UI", 10F);

            ConstruirInterfaz();

            txtBuscar.TextChanged += (s, e) =>
                Ejecutar(CargarUsuarios);

            tabla.SelectionChanged += Tabla_SelectionChanged;

            Load += (s, e) => Ejecutar(CargarUsuarios);
        }

        private void ConstruirInterfaz()
        {
            SuspendLayout();

            // ENCABEZADO
            var encabezado = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = azul
            };

            encabezado.Controls.Add(new Label
            {
                Text = "GESTIÓN DE USUARIOS",
                Font = new Font(
                    "Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 12)
            });

            encabezado.Controls.Add(new Label
            {
                Text = "Administración de usuarios de la biblioteca",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(220, 232, 248),
                AutoSize = true,
                Location = new Point(32, 56)
            });

            // CONTENEDOR
            var contenedor = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(25, 20, 25, 25),
                BackColor = fondo
            };

            var estructura = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            estructura.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            estructura.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 235));

            estructura.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 105));

            estructura.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            contenedor.Controls.Add(estructura);

            // PANEL DE DATOS
            var panelDatos = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 12)
            };

            estructura.Controls.Add(panelDatos, 0, 0);

            CrearCampo(
                panelDatos, "NOMBRE",
                txtNombre, 20, 18, 430);

            CrearCampo(
                panelDatos, "APELLIDO",
                txtApellido, 480, 18, 430);

            CrearCampo(
                panelDatos, "CORREO ELECTRÓNICO",
                txtCorreo, 20, 90, 430);

            CrearCampo(
                panelDatos, "TELÉFONO",
                txtTelefono, 480, 90, 430);

            txtCorreo.PlaceholderText =
                "ejemplo@correo.com";

            txtTelefono.PlaceholderText =
                "Número de contacto";

            // BOTONES
            var guardar = CrearBoton(
                "GUARDAR",
                Color.FromArgb(35, 105, 75));

            var editar = CrearBoton("EDITAR", azul);

            var eliminar = CrearBoton(
                "ELIMINAR",
                Color.FromArgb(170, 55, 65));

            var limpiar = CrearBoton(
                "LIMPIAR",
                Color.FromArgb(95, 105, 120));

            guardar.Location = new Point(20, 175);
            editar.Location = new Point(190, 175);
            eliminar.Location = new Point(360, 175);
            limpiar.Location = new Point(530, 175);

            panelDatos.Controls.AddRange(new Control[]
            {
                guardar, editar, eliminar, limpiar
            });

            guardar.Click += (s, e) => Ejecutar(() =>
            {
                ValidarDatos();

                servicio.RegistrarUsuario(LeerUsuario());

                MessageBox.Show(
                    "Usuario registrado correctamente.",
                    "Registro exitoso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarUsuarios();
                Limpiar();
            });

            editar.Click += (s, e) => Ejecutar(() =>
            {
                if (usuarioID <= 0)
                    throw new Exception(
                        "Selecciona un usuario para editar.");

                ValidarDatos();

                servicio.ActualizarUsuario(LeerUsuario());

                MessageBox.Show(
                    "Usuario actualizado correctamente.",
                    "Actualización exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarUsuarios();
                Limpiar();
            });

            eliminar.Click += (s, e) => Ejecutar(() =>
            {
                if (usuarioID <= 0)
                    throw new Exception(
                        "Selecciona un usuario para eliminar.");

                if (MessageBox.Show(
                    "¿Seguro que deseas eliminar este usuario?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question)
                    != DialogResult.Yes)
                    return;

                servicio.EliminarUsuario(usuarioID);

                MessageBox.Show(
                    "Usuario eliminado correctamente.",
                    "Eliminación exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarUsuarios();
                Limpiar();
            });

            limpiar.Click += (s, e) => Limpiar();

            // BÚSQUEDA
            var panelBusqueda = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                BackColor = fondo
            };

            estructura.Controls.Add(panelBusqueda, 0, 1);

            panelBusqueda.Controls.Add(new Label
            {
                Text = "BUSCAR USUARIO",
                Font = new Font(
                    "Segoe UI", 10F, FontStyle.Bold),
                ForeColor = azul,
                AutoSize = true,
                Location = new Point(0, 15)
            });

            txtBuscar.PlaceholderText =
                "Buscar por nombre, apellido, correo o teléfono...";

            txtBuscar.Location = new Point(0, 45);
            txtBuscar.Size = new Size(500, 32);
            txtBuscar.BorderStyle = BorderStyle.FixedSingle;

            panelBusqueda.Controls.Add(txtBuscar);

            lblResultados.Location = new Point(530, 50);
            lblResultados.AutoSize = true;
            lblResultados.ForeColor =
                Color.FromArgb(80, 90, 105);

            panelBusqueda.Controls.Add(lblResultados);

            // TABLA
            var panelTabla = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = Padding.Empty
            };

            estructura.Controls.Add(panelTabla, 0, 2);

            tabla.Dock = DockStyle.Fill;
            tabla.ReadOnly = true;
            tabla.AllowUserToAddRows = false;
            tabla.AllowUserToDeleteRows = false;
            tabla.MultiSelect = false;

            tabla.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            tabla.AutoGenerateColumns = true;
            tabla.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            tabla.BackgroundColor = Color.White;
            tabla.BorderStyle = BorderStyle.None;
            tabla.RowHeadersVisible = false;
            tabla.RowTemplate.Height = 38;

            tabla.EnableHeadersVisualStyles = false;
            tabla.ColumnHeadersHeight = 45;

            tabla.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode
                    .DisableResizing;

            tabla.ColumnHeadersDefaultCellStyle.BackColor = azul;
            tabla.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            tabla.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold);

            tabla.DefaultCellStyle.Font =
                new Font("Segoe UI", 10F);

            tabla.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(210, 225, 245);

            tabla.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(25, 40, 65);

            tabla.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(244, 248, 253);

            panelTabla.Controls.Add(tabla);

            // ORDEN CORRECTO DE DOCK
            Controls.Add(encabezado);
            Controls.Add(contenedor);

            Controls.SetChildIndex(contenedor, 0);
            Controls.SetChildIndex(encabezado, 1);

            ResumeLayout(true);
        }

        private static void CrearCampo(
            Control contenedor,
            string etiqueta,
            TextBox campo,
            int x,
            int y,
            int ancho)
        {
            contenedor.Controls.Add(new Label
            {
                Text = etiqueta,
                Font = new Font(
                    "Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(38, 78, 133),
                AutoSize = true,
                Location = new Point(x, y)
            });

            campo.Location = new Point(x, y + 27);
            campo.Size = new Size(ancho, 30);
            campo.BorderStyle = BorderStyle.FixedSingle;

            contenedor.Controls.Add(campo);
        }

        private static Button CrearBoton(
            string texto,
            Color color)
        {
            var boton = new Button
            {
                Text = texto,
                Size = new Size(155, 42),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(
                    "Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            boton.FlatAppearance.BorderSize = 0;

            return boton;
        }

        private void CargarUsuarios()
        {
            cargando = true;

            try
            {
                var usuarios = servicio.ObtenerUsuarios();
                string busqueda = txtBuscar.Text.Trim();

                if (!string.IsNullOrWhiteSpace(busqueda))
                {
                    usuarios = usuarios
                        .Where(u =>
                            (u.Nombre ?? "").Contains(
                                busqueda,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            (u.Apellido ?? "").Contains(
                                busqueda,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            (u.Correo ?? "").Contains(
                                busqueda,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            (u.Telefono ?? "").Contains(
                                busqueda,
                                StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                tabla.DataSource = null;
                tabla.DataSource = usuarios;

                CambiarTitulo("UsuarioID", "ID");
                CambiarTitulo("Nombre", "NOMBRE");
                CambiarTitulo("Apellido", "APELLIDO");
                CambiarTitulo("Correo", "CORREO ELECTRÓNICO");
                CambiarTitulo("Telefono", "TELÉFONO");

                tabla.ClearSelection();

                lblResultados.Text = usuarios.Count == 1
                    ? "1 usuario encontrado"
                    : $"{usuarios.Count} usuarios encontrados";

                usuarioID = 0;
            }
            finally
            {
                cargando = false;
            }
        }

        private void CambiarTitulo(
            string columna,
            string titulo)
        {
            if (tabla.Columns.Contains(columna))
                tabla.Columns[columna].HeaderText = titulo;
        }

        private void Tabla_SelectionChanged(
            object? sender,
            EventArgs e)
        {
            if (cargando)
                return;

            if (tabla.SelectedRows.Count == 0)
                return;

            if (tabla.SelectedRows[0].DataBoundItem
                is not Usuario usuario)
                return;

            usuarioID = usuario.UsuarioID;
            txtNombre.Text = usuario.Nombre;
            txtApellido.Text = usuario.Apellido;
            txtCorreo.Text = usuario.Correo;
            txtTelefono.Text = usuario.Telefono ?? "";
        }

        private void ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
                throw new Exception("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(txtApellido.Text))
                throw new Exception("El apellido es obligatorio.");

            if (string.IsNullOrWhiteSpace(txtCorreo.Text))
                throw new Exception("El correo es obligatorio.");

            try
            {
                var correo = new MailAddress(txtCorreo.Text.Trim());

                if (!string.Equals(
                    correo.Address,
                    txtCorreo.Text.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new FormatException();
                }
            }
            catch (FormatException)
            {
                throw new Exception(
                    "Ingresa un correo electrónico válido.");
            }

            if (!string.IsNullOrWhiteSpace(txtTelefono.Text) &&
                txtTelefono.Text.Trim().Length < 7)
            {
                throw new Exception(
                    "El teléfono debe tener al menos 7 caracteres.");
            }
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

        private void Limpiar()
        {
            usuarioID = 0;

            txtNombre.Clear();
            txtApellido.Clear();
            txtCorreo.Clear();
            txtTelefono.Clear();

            tabla.ClearSelection();
            txtNombre.Focus();
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
