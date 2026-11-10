
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Biblioteca.Business.Services;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Presentation
{
    public partial class Form1 : Form
    {
        private readonly LibroService _libroService = new();
        private readonly EditorialService _editorialService = new();

        private int _libroSeleccionadoID = 0;
        private bool _cargandoLibros = false;

        private readonly TextBox txtBuscarLibro = new();
        private readonly ComboBox cboEditorial = new();

        private readonly Color azul = Color.FromArgb(38, 78, 133);
        private readonly Color fondo = Color.FromArgb(245, 248, 252);

        public Form1()
        {
            InitializeComponent();

            ConfigurarDiseno();

            dgvLibros.AutoGenerateColumns = true;
            dgvLibros.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvLibros.MultiSelect = false;
            dgvLibros.ReadOnly = true;
            dgvLibros.AllowUserToAddRows = false;

            btnGuardar.Click += btnGuardar_Click;
            btnEditar.Click += btnEditar_Click;
            btnEliminar.Click += btnEliminar_Click;

            dgvLibros.SelectionChanged +=
                dgvLibros_SelectionChanged;

            txtBuscarLibro.TextChanged +=
                (sender, e) => CargarLibros();

            Load += Formulario_Load;
        }

        private void ConfigurarDiseno()
        {
            Text = "Biblioteca | Gestión de Libros";
            BackColor = fondo;
            Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(1050, 720);
            Size = new Size(1250, 780);
            StartPosition = FormStartPosition.CenterScreen;

            // Ocultar etiquetas originales.
            foreach (Control control in Controls)
            {
                if (control is Label)
                    control.Visible = false;
            }

            // Ocultar campo de ID de editorial.
            txtEditorialID.Visible = false;

            // Encabezado.
            Panel encabezado = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = azul
            };

            Controls.Add(encabezado);
            encabezado.BringToFront();

            Label titulo = new Label
            {
                Text = "GESTIÓN DE LIBROS",
                Font = new Font(
                    "Segoe UI", 19F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 12)
            };

            Label subtitulo = new Label
            {
                Text = "Administración del catálogo bibliográfico",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(220, 232, 248),
                AutoSize = true,
                Location = new Point(32, 53)
            };

            encabezado.Controls.Add(titulo);
            encabezado.Controls.Add(subtitulo);

            // Distribución de campos.
            ConfigurarCampo(
                "TÍTULO DEL LIBRO", txtTitulo, 30, 105);

            ConfigurarCampo(
                "ISBN", txtISBN, 30, 170);

            ConfigurarCampo(
                "EDITORIAL", cboEditorial, 30, 235);

            ConfigurarCampo(
                "AÑO DE PUBLICACIÓN", txtAnio, 480, 105);

            ConfigurarCampo(
                "TOTAL DE EJEMPLARES", txtTotal, 480, 170);

            ConfigurarCampo(
                "EJEMPLARES DISPONIBLES",
                txtDisponibles, 480, 235);

            cboEditorial.DropDownStyle =
                ComboBoxStyle.DropDownList;

            Controls.Add(cboEditorial);

            // Botones.
            EstilizarBoton(
                btnGuardar, "GUARDAR",
                Color.FromArgb(35, 105, 75));

            EstilizarBoton(
                btnEditar, "EDITAR", azul);

            EstilizarBoton(
                btnEliminar, "ELIMINAR",
                Color.FromArgb(170, 55, 65));

            btnGuardar.Location = new Point(930, 127);
            btnEditar.Location = new Point(930, 192);
            btnEliminar.Location = new Point(930, 257);

            // Buscador.
            Label lblBuscar = new Label
            {
                Text = "BUSCAR LIBRO",
                Font = new Font(
                    "Segoe UI", 10F, FontStyle.Bold),
                ForeColor = azul,
                AutoSize = true,
                Location = new Point(30, 325)
            };

            txtBuscarLibro.Name = "txtBuscarLibro";
            txtBuscarLibro.PlaceholderText =
                "Buscar por título o ISBN...";
            txtBuscarLibro.Font =
                new Font("Segoe UI", 10F);
            txtBuscarLibro.Location =
                new Point(175, 320);
            txtBuscarLibro.Size =
                new Size(370, 30);

            Controls.Add(lblBuscar);
            Controls.Add(txtBuscarLibro);

            // Tabla.
            dgvLibros.Location = new Point(30, 375);
            dgvLibros.Size = new Size(
                ClientSize.Width - 60,
                ClientSize.Height - 405);

            dgvLibros.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            dgvLibros.BackgroundColor = Color.White;
            dgvLibros.BorderStyle = BorderStyle.None;
            dgvLibros.EnableHeadersVisualStyles = false;

            dgvLibros.ColumnHeadersDefaultCellStyle.BackColor =
                azul;

            dgvLibros.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            dgvLibros.ColumnHeadersDefaultCellStyle.Font =
                new Font(
                    "Segoe UI", 10F, FontStyle.Bold);

            dgvLibros.ColumnHeadersHeight = 42;
            dgvLibros.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgvLibros.DefaultCellStyle.Font =
                new Font("Segoe UI", 10F);

            dgvLibros.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(210, 225, 245);

            dgvLibros.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(25, 40, 65);

            dgvLibros.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(244, 248, 253);

            dgvLibros.RowTemplate.Height = 34;
            dgvLibros.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgvLibros.RowHeadersVisible = false;
        }

        private void ConfigurarCampo(
            string texto, Control campo, int x, int y)
        {
            Label etiqueta = new Label
            {
                Text = texto,
                Font = new Font(
                    "Segoe UI", 9F, FontStyle.Bold),
                ForeColor = azul,
                AutoSize = true,
                Location = new Point(x, y)
            };

            Controls.Add(etiqueta);

            campo.Location = new Point(x, y + 23);
            campo.Size = new Size(405, 29);
            campo.Font = new Font("Segoe UI", 10F);

            if (campo is TextBox caja)
                caja.BorderStyle = BorderStyle.FixedSingle;
        }

        private void EstilizarBoton(
            Button boton, string texto, Color color)
        {
            boton.Text = texto;
            boton.BackColor = color;
            boton.ForeColor = Color.White;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Font = new Font(
                "Segoe UI", 10F, FontStyle.Bold);
            boton.Cursor = Cursors.Hand;
            boton.Size = new Size(220, 45);
        }

        private void Formulario_Load(
            object? sender, EventArgs e)
        {
            CargarEditoriales();
            CargarLibros();
        }

        // Conserva el evento del diseñador.
        private void Form1_Load(
            object sender, EventArgs e)
        {
            // La carga se realiza en Formulario_Load.
        }

        private void CargarEditoriales()
        {
            try
            {
                var editoriales =
                    _editorialService.ObtenerEditoriales();

                cboEditorial.DataSource = null;
                cboEditorial.DataSource = editoriales;
                cboEditorial.DisplayMember = "Nombre";
                cboEditorial.ValueMember = "EditorialID";
                cboEditorial.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar editoriales: " +
                    ex.Message);
            }
        }

        private void CargarLibros()
        {
            try
            {
                _cargandoLibros = true;

                var libros =
                    _libroService.ObtenerLibros();

                string busqueda =
                    txtBuscarLibro.Text.Trim();

                if (!string.IsNullOrWhiteSpace(busqueda))
                {
                    libros = libros
                        .Where(libro =>
                            (libro.Titulo ?? "").Contains(
                                busqueda,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            (libro.ISBN ?? "").Contains(
                                busqueda,
                                StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                dgvLibros.DataSource = null;
                dgvLibros.DataSource = libros;

                dgvLibros.ClearSelection();
                _libroSeleccionadoID = 0;

                ConfigurarColumnas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar libros: " +
                    ex.Message);
            }
            finally
            {
                _cargandoLibros = false;
            }
        }

        private void ConfigurarColumnas()
        {
            CambiarTituloColumna(
                "LibroID", "ID");

            CambiarTituloColumna(
                "Titulo", "TÍTULO");

            CambiarTituloColumna(
                "ISBN", "ISBN");

            CambiarTituloColumna(
                "EditorialID", "EDITORIAL ID");

            CambiarTituloColumna(
                "AnioPublicacion", "AÑO");

            CambiarTituloColumna(
                "TotalEjemplares", "TOTAL");

            CambiarTituloColumna(
                "EjemplaresDisponibles", "DISPONIBLES");
        }

        private void CambiarTituloColumna(
            string nombre, string titulo)
        {
            if (dgvLibros.Columns.Contains(nombre))
                dgvLibros.Columns[nombre].HeaderText = titulo;
        }

        private Libro ObtenerDatosFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtTitulo.Text))
                throw new Exception(
                    "Debes ingresar el título del libro.");

            if (string.IsNullOrWhiteSpace(txtISBN.Text))
                throw new Exception(
                    "Debes ingresar el ISBN del libro.");

            if (cboEditorial.SelectedValue == null ||
                !int.TryParse(
                    cboEditorial.SelectedValue.ToString(),
                    out int editorialID))
            {
                throw new Exception(
                    "Debes seleccionar una editorial.");
            }

            if (!int.TryParse(
                txtTotal.Text, out int total) ||
                total < 0)
            {
                throw new Exception(
                    "El total de ejemplares no es válido.");
            }

            if (!int.TryParse(
                txtDisponibles.Text,
                out int disponibles) ||
                disponibles < 0 ||
                disponibles > total)
            {
                throw new Exception(
                    "Los ejemplares disponibles deben estar entre 0 y el total.");
            }

            int? anio = null;

            if (!string.IsNullOrWhiteSpace(txtAnio.Text))
            {
                if (!int.TryParse(
                    txtAnio.Text,
                    out int valorAnio) ||
                    valorAnio < 1450 ||
                    valorAnio > DateTime.Now.Year + 1)
                {
                    throw new Exception(
                        "El año de publicación no es válido.");
                }

                anio = valorAnio;
            }

            return new Libro
            {
                LibroID = _libroSeleccionadoID,
                Titulo = txtTitulo.Text.Trim(),
                ISBN = txtISBN.Text.Trim(),
                EditorialID = editorialID,
                AnioPublicacion = anio,
                TotalEjemplares = total,
                EjemplaresDisponibles = disponibles
            };
        }

        private void btnGuardar_Click(
            object? sender, EventArgs e)
        {
            try
            {
                Libro libro = ObtenerDatosFormulario();

                _libroService.RegistrarLibro(libro);

                MessageBox.Show(
                    "Libro registrado correctamente.",
                    "Registro exitoso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LimpiarCampos();
                CargarLibros();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al guardar: " + ex.Message);
            }
        }

        private void btnEditar_Click(
            object? sender, EventArgs e)
        {
            try
            {
                if (_libroSeleccionadoID <= 0)
                {
                    MessageBox.Show(
                        "Primero selecciona un libro.");
                    return;
                }

                Libro libro = ObtenerDatosFormulario();

                libro.LibroID = _libroSeleccionadoID;

                _libroService.ActualizarLibro(libro);

                MessageBox.Show(
                    "Libro actualizado correctamente.",
                    "Actualización exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LimpiarCampos();
                CargarLibros();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al editar: " + ex.Message);
            }
        }

        private void btnEliminar_Click(
            object? sender, EventArgs e)
        {
            try
            {
                if (_libroSeleccionadoID <= 0)
                {
                    MessageBox.Show(
                        "Primero selecciona un libro.");
                    return;
                }

                DialogResult respuesta = MessageBox.Show(
                    "¿Deseas eliminar este libro?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (respuesta != DialogResult.Yes)
                    return;

                _libroService.EliminarLibro(
                    _libroSeleccionadoID);

                MessageBox.Show(
                    "Libro eliminado correctamente.");

                LimpiarCampos();
                CargarLibros();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al eliminar: " + ex.Message);
            }
        }

        private void dgvLibros_SelectionChanged(
            object? sender, EventArgs e)
        {
            if (_cargandoLibros)
                return;

            if (dgvLibros.SelectedRows.Count == 0)
            {
                _libroSeleccionadoID = 0;
                return;
            }

            DataGridViewRow fila =
                dgvLibros.SelectedRows[0];

            if (fila.DataBoundItem is not Libro libro)
                return;

            _libroSeleccionadoID = libro.LibroID;

            txtTitulo.Text = libro.Titulo;
            txtISBN.Text = libro.ISBN;

            cboEditorial.SelectedValue =
                libro.EditorialID;

            txtAnio.Text =
                libro.AnioPublicacion?.ToString() ?? "";

            txtTotal.Text =
                libro.TotalEjemplares.ToString();

            txtDisponibles.Text =
                libro.EjemplaresDisponibles.ToString();
        }

        private void LimpiarCampos()
        {
            txtTitulo.Clear();
            txtISBN.Clear();
            txtAnio.Clear();
            txtTotal.Clear();
            txtDisponibles.Clear();

            cboEditorial.SelectedIndex = -1;

            _libroSeleccionadoID = 0;
            dgvLibros.ClearSelection();
        }

        // Eventos antiguos del diseñador.
        private void label1_Click(
            object? sender, EventArgs e)
        {
        }

        private void dataGridView1_CellContentClick_1(
            object? sender,
            DataGridViewCellEventArgs e)
        {
        }
    }
}
