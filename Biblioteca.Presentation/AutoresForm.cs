
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Biblioteca.Business.Services;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Presentation
{
    public class AutoresForm : Form
    {
        private readonly AutorService servicio = new();
        private readonly LibroService libroService = new();

        private readonly TextBox txtNombre = new();
        private readonly TextBox txtApellido = new();
        private readonly TextBox txtPais = new();
        private readonly TextBox txtBuscar = new();

        private readonly DataGridView tabla = new();
        private readonly DataGridView tablaLibros = new();
        private readonly Label lblCantidadLibros = new();

        private int autorID = 0;

        private readonly Color azul =
            Color.FromArgb(38, 78, 133);

        private readonly Color fondo =
            Color.FromArgb(245, 248, 252);

        public AutoresForm()
        {
            ConfigurarFormulario();
            ConstruirInterfaz();
            ConfigurarTablas();

            Load += (s, e) => Ejecutar(Recargar);
        }

        private void ConfigurarFormulario()
        {
            Text = "Biblioteca | Gestión de Autores";
            Size = new Size(1100, 850);
            MinimumSize = new Size(900, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = fondo;
            Font = new Font("Segoe UI", 10F);
        }

        private void ConstruirInterfaz()
        {
            var encabezado = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = azul
            };

            encabezado.Controls.Add(new Label
            {
                Text = "GESTIÓN DE AUTORES",
                Font = new Font("Segoe UI", 19F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 12)
            });

            encabezado.Controls.Add(new Label
            {
                Text = "Administración de autores y sus libros",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(220, 232, 248),
                AutoSize = true,
                Location = new Point(32, 53)
            });

            var contenido = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(25, 15, 25, 15),
                ColumnCount = 1,
                RowCount = 5
            };

            contenido.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 150));
            contenido.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 60));
            contenido.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 55));
            contenido.RowStyles.Add(
                new RowStyle(SizeType.Percent, 48));
            contenido.RowStyles.Add(
                new RowStyle(SizeType.Percent, 52));

            var campos = new Panel
            {
                Dock = DockStyle.Fill
            };

            AgregarCampo(campos, "NOMBRE", txtNombre,
                10, 10, 390);

            AgregarCampo(campos, "APELLIDO", txtApellido,
                430, 10, 390);

            AgregarCampo(campos, "PAÍS", txtPais,
                10, 85, 390);

            contenido.Controls.Add(campos, 0, 0);

            var botones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true
            };

            var guardar = CrearBoton(
                "GUARDAR", Color.FromArgb(35, 105, 75));

            var editar = CrearBoton("EDITAR", azul);

            var eliminar = CrearBoton(
                "ELIMINAR", Color.FromArgb(170, 55, 65));

            var limpiar = CrearBoton(
                "LIMPIAR", Color.FromArgb(95, 105, 120));

            botones.Controls.AddRange(
                new Control[]
                {
                    guardar, editar, eliminar, limpiar
                });

            contenido.Controls.Add(botones, 0, 1);

            var buscador = new Panel
            {
                Dock = DockStyle.Fill
            };

            buscador.Controls.Add(new Label
            {
                Text = "BUSCAR AUTOR",
                Font = new Font(
                    "Segoe UI", 10F, FontStyle.Bold),
                ForeColor = azul,
                AutoSize = true,
                Location = new Point(10, 12)
            });

            txtBuscar.PlaceholderText =
                "Buscar por nombre, apellido o país...";

            txtBuscar.Location = new Point(160, 8);
            txtBuscar.Size = new Size(450, 30);
            txtBuscar.BorderStyle = BorderStyle.FixedSingle;

            txtBuscar.TextChanged +=
                (s, e) => Ejecutar(Recargar);

            buscador.Controls.Add(txtBuscar);
            contenido.Controls.Add(buscador, 0, 2);

            var panelAutores = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 0, 0, 8)
            };

            panelAutores.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 35));

            panelAutores.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            panelAutores.Controls.Add(new Label
            {
                Text = "AUTORES REGISTRADOS",
                Font = new Font(
                    "Segoe UI", 11F, FontStyle.Bold),
                ForeColor = azul,
                Dock = DockStyle.Fill
            }, 0, 0);

            tabla.Dock = DockStyle.Fill;
            panelAutores.Controls.Add(tabla, 0, 1);

            contenido.Controls.Add(panelAutores, 0, 3);

            var panelLibros = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            panelLibros.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 40));

            panelLibros.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            var cabeceraLibros = new Panel
            {
                Dock = DockStyle.Fill
            };

            cabeceraLibros.Controls.Add(new Label
            {
                Text = "LIBROS DEL AUTOR SELECCIONADO",
                Font = new Font(
                    "Segoe UI", 11F, FontStyle.Bold),
                ForeColor = azul,
                AutoSize = true,
                Location = new Point(0, 8)
            });

            lblCantidadLibros.Text =
                "Selecciona un autor para ver sus libros";

            lblCantidadLibros.ForeColor =
                Color.FromArgb(80, 90, 105);

            lblCantidadLibros.AutoSize = true;
            lblCantidadLibros.Anchor =
                AnchorStyles.Top | AnchorStyles.Right;

            lblCantidadLibros.Location =
                new Point(630, 10);

            cabeceraLibros.Controls.Add(lblCantidadLibros);

            panelLibros.Controls.Add(cabeceraLibros, 0, 0);

            tablaLibros.Dock = DockStyle.Fill;
            panelLibros.Controls.Add(tablaLibros, 0, 1);

            contenido.Controls.Add(panelLibros, 0, 4);

            Controls.Add(contenido);
            Controls.Add(encabezado);

            guardar.Click += (s, e) => Ejecutar(() =>
            {
                ValidarCampos();
                servicio.RegistrarAutor(LeerAutor());

                MessageBox.Show(
                    "Autor registrado correctamente.",
                    "Registro exitoso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Recargar();
            });

            editar.Click += (s, e) => Ejecutar(() =>
            {
                if (autorID <= 0)
                    throw new Exception(
                        "Primero selecciona un autor.");

                ValidarCampos();
                servicio.ActualizarAutor(LeerAutor());

                MessageBox.Show(
                    "Autor actualizado correctamente.",
                    "Actualización exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Recargar();
            });

            eliminar.Click += (s, e) => Ejecutar(() =>
            {
                if (autorID <= 0)
                    throw new Exception(
                        "Primero selecciona un autor.");

                if (MessageBox.Show(
                    "¿Deseas eliminar este autor?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question)
                    != DialogResult.Yes)
                    return;

                servicio.EliminarAutor(autorID);

                MessageBox.Show(
                    "Autor eliminado correctamente.");

                Recargar();
            });

            limpiar.Click += (s, e) => Limpiar();
        }

        private void AgregarCampo(
            Panel contenedor,
            string nombre,
            TextBox campo,
            int x,
            int y,
            int ancho)
        {
            contenedor.Controls.Add(new Label
            {
                Text = nombre,
                Font = new Font(
                    "Segoe UI", 9F, FontStyle.Bold),
                ForeColor = azul,
                AutoSize = true,
                Location = new Point(x, y)
            });

            campo.Location = new Point(x, y + 25);
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
                Width = 205,
                Height = 45,
                Margin = new Padding(0, 0, 15, 0),
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

        private void ConfigurarTablas()
        {
            EstilizarTabla(tabla);
            EstilizarTabla(tablaLibros);

            tabla.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0)
                    return;

                if (tabla.Rows[e.RowIndex].DataBoundItem
                    is not Autor autor)
                    return;

                autorID = autor.AutorID;
                txtNombre.Text = autor.Nombre;
                txtApellido.Text = autor.Apellido;
                txtPais.Text = autor.Pais ?? "";

                Ejecutar(() => CargarLibros(autorID));
            };

            tablaLibros.SelectionChanged += (s, e) =>
                tablaLibros.ClearSelection();
        }

        private void EstilizarTabla(DataGridView grid)
        {
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AutoGenerateColumns = true;
            grid.MultiSelect = false;

            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.RowHeadersVisible = false;

            grid.EnableHeadersVisualStyles = false;

            grid.ColumnHeadersDefaultCellStyle.BackColor =
                azul;

            grid.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold);

            grid.ColumnHeadersHeight = 42;

            grid.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode
                    .DisableResizing;

            grid.DefaultCellStyle.Font =
                new Font("Segoe UI", 10F);

            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(210, 225, 245);

            grid.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(25, 40, 65);

            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(244, 248, 253);

            grid.RowTemplate.Height = 34;
        }

        private void CargarLibros(int id)
        {
            var libros = libroService.ObtenerLibrosPorAutor(id);

            tablaLibros.DataSource = null;

            tablaLibros.DataSource = libros
                .Select(l => new
                {
                    l.Titulo,
                    l.ISBN,
                    l.AnioPublicacion,
                    l.TotalEjemplares,
                    l.EjemplaresDisponibles
                })
                .ToList();

            if (tablaLibros.Columns.Contains("Titulo"))
                tablaLibros.Columns["Titulo"].HeaderText =
                    "TÍTULO";

            if (tablaLibros.Columns.Contains("ISBN"))
                tablaLibros.Columns["ISBN"].HeaderText =
                    "ISBN";

            if (tablaLibros.Columns.Contains("AnioPublicacion"))
                tablaLibros.Columns["AnioPublicacion"].HeaderText =
                    "AÑO";

            if (tablaLibros.Columns.Contains("TotalEjemplares"))
                tablaLibros.Columns["TotalEjemplares"].HeaderText =
                    "TOTAL";

            if (tablaLibros.Columns.Contains("EjemplaresDisponibles"))
                tablaLibros.Columns["EjemplaresDisponibles"].HeaderText =
                    "DISPONIBLES";

            lblCantidadLibros.Text =
                libros.Count == 1
                    ? "1 libro registrado"
                    : $"{libros.Count} libros registrados";
        }

        private void ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
                throw new Exception(
                    "Debes ingresar el nombre del autor.");

            if (string.IsNullOrWhiteSpace(txtApellido.Text))
                throw new Exception(
                    "Debes ingresar el apellido del autor.");

            if (txtNombre.Text.Trim().Length > 100)
                throw new Exception(
                    "El nombre no puede superar 100 caracteres.");

            if (txtApellido.Text.Trim().Length > 100)
                throw new Exception(
                    "El apellido no puede superar 100 caracteres.");

            if (txtPais.Text.Trim().Length > 80)
                throw new Exception(
                    "El país no puede superar 80 caracteres.");
        }

        private Autor LeerAutor()
        {
            return new Autor
            {
                AutorID = autorID,
                Nombre = txtNombre.Text.Trim(),
                Apellido = txtApellido.Text.Trim(),
                Pais = txtPais.Text.Trim()
            };
        }

        private void Recargar()
        {
            var autores = servicio.ObtenerAutores();

            string busqueda = txtBuscar.Text.Trim();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                autores = autores
                    .Where(a =>
                        (a.Nombre ?? "").Contains(
                            busqueda,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        (a.Apellido ?? "").Contains(
                            busqueda,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        (a.Pais ?? "").Contains(
                            busqueda,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            tabla.DataSource = null;
            tabla.DataSource = autores;

            if (tabla.Columns.Contains("AutorID"))
                tabla.Columns["AutorID"].HeaderText = "ID";

            if (tabla.Columns.Contains("Nombre"))
                tabla.Columns["Nombre"].HeaderText = "NOMBRE";

            if (tabla.Columns.Contains("Apellido"))
                tabla.Columns["Apellido"].HeaderText = "APELLIDO";

            if (tabla.Columns.Contains("Pais"))
                tabla.Columns["Pais"].HeaderText = "PAÍS";

            Limpiar();
        }

        private void Limpiar()
        {
            autorID = 0;

            txtNombre.Clear();
            txtApellido.Clear();
            txtPais.Clear();

            tabla.ClearSelection();

            tablaLibros.DataSource = null;

            lblCantidadLibros.Text =
                "Selecciona un autor para ver sus libros";
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
