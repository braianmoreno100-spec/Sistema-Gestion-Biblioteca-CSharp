
using System;
using System.Collections.Generic;
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
        private readonly AutorService _autorService = new();

        private readonly Color azul =
            Color.FromArgb(38, 78, 133);

        private readonly Color fondo =
            Color.FromArgb(245, 248, 252);

        private readonly TextBox txtBuscarLibro = new();
        private readonly ComboBox cboEditorial = new();

        // NUEVO: BUSCADOR Y SELECCIÓN DE AUTORES
        private readonly TextBox txtBuscarAutor = new();
        private readonly CheckedListBox listaAutores = new();
        private readonly Label lblAutoresSeleccionados = new();
        private readonly Label lblResultadosAutores = new();

        private readonly List<AutorOpcion> _todosAutores = new();
        private readonly HashSet<int> _autoresSeleccionados = new();

        private int _libroSeleccionadoID = 0;
        private bool _cargandoLibros = false;
        private bool _actualizandoListaAutores = false;

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
                (s, e) => CargarLibros();

            txtBuscarAutor.TextChanged +=
                (s, e) => FiltrarAutores();

            listaAutores.ItemCheck +=
                ListaAutores_ItemCheck;

            Load += Formulario_Load;
        }

        private void ConfigurarDiseno()
        {
            Text = "Biblioteca | Gestión de Libros";
            BackColor = fondo;
            Font = new Font("Segoe UI", 10F);
            Size = new Size(1300, 900);
            MinimumSize = new Size(1050, 800);
            StartPosition = FormStartPosition.CenterScreen;

            // OCULTAR ETIQUETAS ANTIGUAS
            foreach (Control control in Controls)
            {
                if (control is Label)
                    control.Visible = false;
            }

            txtEditorialID.Visible = false;

            // ENCABEZADO
            var encabezado = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = azul
            };

            Controls.Add(encabezado);
            encabezado.BringToFront();

            encabezado.Controls.Add(new Label
            {
                Text = "GESTIÓN DE LIBROS",
                Font = new Font(
                    "Segoe UI", 19F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 12)
            });

            encabezado.Controls.Add(new Label
            {
                Text = "Administración del catálogo bibliográfico",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(220, 232, 248),
                AutoSize = true,
                Location = new Point(32, 53)
            });

            // CAMPOS DEL LIBRO
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

            // BOTONES PRINCIPALES
            EstilizarBoton(
                btnGuardar,
                "GUARDAR",
                Color.FromArgb(35, 105, 75));

            EstilizarBoton(
                btnEditar,
                "EDITAR",
                azul);

            EstilizarBoton(
                btnEliminar,
                "ELIMINAR",
                Color.FromArgb(170, 55, 65));

            btnGuardar.Location = new Point(930, 127);
            btnEditar.Location = new Point(930, 192);
            btnEliminar.Location = new Point(930, 257);

            // SECCIÓN AUTORES
            var lblAutores = new Label
            {
                Text = "AUTORES DEL LIBRO",
                Font = new Font(
                    "Segoe UI", 10F, FontStyle.Bold),
                ForeColor = azul,
                AutoSize = true,
                Location = new Point(30, 320)
            };

            Controls.Add(lblAutores);

            // BUSCADOR DE AUTORES
            txtBuscarAutor.PlaceholderText =
                "Buscar autor por nombre, apellido o país...";

            txtBuscarAutor.Location =
                new Point(30, 350);

            txtBuscarAutor.Size =
                new Size(580, 30);

            txtBuscarAutor.BorderStyle =
                BorderStyle.FixedSingle;

            Controls.Add(txtBuscarAutor);

            var btnLimpiarFiltro = new Button
            {
                Text = "LIMPIAR FILTRO",
                Location = new Point(625, 348),
                Size = new Size(140, 34),
                BackColor = Color.FromArgb(95, 105, 120),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            btnLimpiarFiltro.FlatAppearance.BorderSize = 0;

            btnLimpiarFiltro.Click += (s, e) =>
            {
                txtBuscarAutor.Clear();
            };

            Controls.Add(btnLimpiarFiltro);

            // LISTA DE AUTORES
            listaAutores.Location = new Point(30, 390);
            listaAutores.Size = new Size(855, 110);
            listaAutores.CheckOnClick = true;
            listaAutores.BorderStyle =
                BorderStyle.FixedSingle;

            listaAutores.Font =
                new Font("Segoe UI", 10F);

            Controls.Add(listaAutores);

            // CONTADORES
            lblAutoresSeleccionados.Location =
                new Point(30, 510);

            lblAutoresSeleccionados.AutoSize = true;
            lblAutoresSeleccionados.ForeColor = azul;

            Controls.Add(lblAutoresSeleccionados);

            lblResultadosAutores.Location =
                new Point(300, 510);

            lblResultadosAutores.AutoSize = true;
            lblResultadosAutores.ForeColor =
                Color.FromArgb(80, 90, 105);

            Controls.Add(lblResultadosAutores);

            // DESMARCAR TODOS
            var btnDesmarcar = new Button
            {
                Text = "DESMARCAR TODOS",
                Location = new Point(690, 505),
                Size = new Size(195, 35),
                BackColor = Color.FromArgb(170, 55, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            btnDesmarcar.FlatAppearance.BorderSize = 0;

            btnDesmarcar.Click += (s, e) =>
            {
                if (_autoresSeleccionados.Count == 0)
                    return;

                if (MessageBox.Show(
                    "¿Deseas desmarcar todos los autores?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question)
                    != DialogResult.Yes)
                    return;

                _autoresSeleccionados.Clear();
                FiltrarAutores();
            };

            Controls.Add(btnDesmarcar);

            // BUSCADOR DE LIBROS
            var lblBuscarLibro = new Label
            {
                Text = "BUSCAR LIBRO",
                Font = new Font(
                    "Segoe UI", 10F, FontStyle.Bold),
                ForeColor = azul,
                AutoSize = true,
                Location = new Point(30, 560)
            };

            Controls.Add(lblBuscarLibro);

            txtBuscarLibro.PlaceholderText =
                "Buscar por título o ISBN...";

            txtBuscarLibro.Location =
                new Point(175, 555);

            txtBuscarLibro.Size =
                new Size(370, 30);

            Controls.Add(txtBuscarLibro);

            // TABLA DE LIBROS
            dgvLibros.Location = new Point(30, 610);

            dgvLibros.Size = new Size(
                ClientSize.Width - 60,
                ClientSize.Height - 640);

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
                DataGridViewColumnHeadersHeightSizeMode
                    .DisableResizing;

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
            string texto,
            Control campo,
            int x,
            int y)
        {
            var etiqueta = new Label
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
            Button boton,
            string texto,
            Color color)
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
            object? sender,
            EventArgs e)
        {
            CargarEditoriales();
            CargarAutores();
            CargarLibros();
        }

        // EVENTO DEL DISEÑADOR
        private void Form1_Load(
            object sender,
            EventArgs e)
        {
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
                MostrarError(ex.Message);
            }
        }

        // CARGAR TODOS LOS AUTORES
        private void CargarAutores()
        {
            try
            {
                _todosAutores.Clear();

                var autores =
                    _autorService.ObtenerAutores();

                foreach (var autor in autores)
                {
                    _todosAutores.Add(new AutorOpcion
                    {
                        AutorID = autor.AutorID,
                        Nombre = autor.Nombre ?? "",
                        Apellido = autor.Apellido ?? "",
                        Pais = autor.Pais ?? ""
                    });
                }

                FiltrarAutores();
            }
            catch (Exception ex)
            {
                MostrarError(
                    "Error al cargar autores: " +
                    ex.Message);
            }
        }

        // FILTRAR SIN PERDER SELECCIONES
        private void FiltrarAutores()
        {
            string busqueda =
                txtBuscarAutor.Text.Trim();

            var filtrados = _todosAutores
                .Where(a =>
                    string.IsNullOrWhiteSpace(busqueda)
                    ||
                    a.Nombre.Contains(
                        busqueda,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    a.Apellido.Contains(
                        busqueda,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    a.Pais.Contains(
                        busqueda,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    a.NombreCompleto.Contains(
                        busqueda,
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(a => a.Apellido)
                .ThenBy(a => a.Nombre)
                .ToList();

            _actualizandoListaAutores = true;

            try
            {
                listaAutores.BeginUpdate();
                listaAutores.Items.Clear();

                foreach (var autor in filtrados)
                {
                    bool marcado =
                        _autoresSeleccionados.Contains(
                            autor.AutorID);

                    listaAutores.Items.Add(
                        autor, marcado);
                }
            }
            finally
            {
                listaAutores.EndUpdate();
                _actualizandoListaAutores = false;
            }

            ActualizarContadores(filtrados.Count);
        }

        // REGISTRAR CAMBIO DE CASILLA
        private void ListaAutores_ItemCheck(
            object? sender,
            ItemCheckEventArgs e)
        {
            if (_actualizandoListaAutores)
                return;

            if (listaAutores.Items[e.Index]
                is not AutorOpcion autor)
                return;

            if (e.NewValue == CheckState.Checked)
            {
                _autoresSeleccionados.Add(
                    autor.AutorID);
            }
            else
            {
                _autoresSeleccionados.Remove(
                    autor.AutorID);
            }

            ActualizarContadores(
                listaAutores.Items.Count);
        }

        private void ActualizarContadores(
            int resultados)
        {
            int cantidad =
                _autoresSeleccionados.Count;

            lblAutoresSeleccionados.Text =
                cantidad == 1
                    ? "1 autor seleccionado"
                    : $"{cantidad} autores seleccionados";

            lblResultadosAutores.Text =
                resultados == 1
                    ? "1 resultado encontrado"
                    : $"{resultados} resultados encontrados";
        }

        private List<int> ObtenerAutoresSeleccionados()
        {
            return _autoresSeleccionados.ToList();
        }

        // MOSTRAR AUTORES DE UN LIBRO EXISTENTE
        private void SeleccionarAutoresDelLibro(
            int libroID)
        {
            var ids =
                _libroService.ObtenerAutoresPorLibro(
                    libroID);

            _autoresSeleccionados.Clear();

            foreach (int id in ids)
            {
                _autoresSeleccionados.Add(id);
            }

            FiltrarAutores();
        }

        private void LimpiarAutores()
        {
            _autoresSeleccionados.Clear();
            txtBuscarAutor.Clear();
            FiltrarAutores();
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
                        .Where(l =>
                            (l.Titulo ?? "").Contains(
                                busqueda,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            (l.ISBN ?? "").Contains(
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
                MostrarError(
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
            CambiarTituloColumna("LibroID", "ID");
            CambiarTituloColumna("Titulo", "TÍTULO");
            CambiarTituloColumna("ISBN", "ISBN");
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
            string nombre,
            string titulo)
        {
            if (dgvLibros.Columns.Contains(nombre))
                dgvLibros.Columns[nombre].HeaderText =
                    titulo;
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
                txtTotal.Text,
                out int total) ||
                total < 1)
            {
                throw new Exception(
                    "El total de ejemplares debe ser mayor que cero.");
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

            if (!string.IsNullOrWhiteSpace(
                txtAnio.Text))
            {
                if (!int.TryParse(
                    txtAnio.Text,
                    out int valorAnio) ||
                    valorAnio < 1 ||
                    valorAnio > DateTime.Now.Year)
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
            object? sender,
            EventArgs e)
        {
            try
            {
                var libro = ObtenerDatosFormulario();
                var autores = ObtenerAutoresSeleccionados();

                if (autores.Count == 0)
                    throw new Exception(
                        "Selecciona al menos un autor.");

                int nuevoID =
                    _libroService.RegistrarLibroConAutores(
                        libro, autores);

                MessageBox.Show(
                    $"Libro registrado correctamente. ID: {nuevoID}",
                    "Registro exitoso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LimpiarCampos();
                CargarLibros();
            }
            catch (Exception ex)
            {
                MostrarError(
                    "Error al guardar: " +
                    ex.Message);
            }
        }

        private void btnEditar_Click(
            object? sender,
            EventArgs e)
        {
            try
            {
                if (_libroSeleccionadoID <= 0)
                    throw new Exception(
                        "Primero selecciona un libro.");

                var libro = ObtenerDatosFormulario();
                var autores = ObtenerAutoresSeleccionados();

                if (autores.Count == 0)
                    throw new Exception(
                        "Selecciona al menos un autor.");

                _libroService.ActualizarLibroConAutores(
                    libro, autores);

                MessageBox.Show(
                    "Libro y autores actualizados correctamente.",
                    "Actualización exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LimpiarCampos();
                CargarLibros();
            }
            catch (Exception ex)
            {
                MostrarError(
                    "Error al editar: " +
                    ex.Message);
            }
        }

        private void btnEliminar_Click(
            object? sender,
            EventArgs e)
        {
            try
            {
                if (_libroSeleccionadoID <= 0)
                    throw new Exception(
                        "Primero selecciona un libro.");

                if (MessageBox.Show(
                    "¿Deseas eliminar este libro?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question)
                    != DialogResult.Yes)
                {
                    return;
                }

                _libroService.EliminarLibro(
                    _libroSeleccionadoID);

                MessageBox.Show(
                    "Libro eliminado correctamente.");

                LimpiarCampos();
                CargarLibros();
            }
            catch (Exception ex)
            {
                MostrarError(
                    "Error al eliminar: " +
                    ex.Message);
            }
        }

        private void dgvLibros_SelectionChanged(
            object? sender,
            EventArgs e)
        {
            if (_cargandoLibros)
                return;

            if (dgvLibros.SelectedRows.Count == 0)
            {
                _libroSeleccionadoID = 0;
                return;
            }

            var fila = dgvLibros.SelectedRows[0];

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

            try
            {
                SeleccionarAutoresDelLibro(
                    libro.LibroID);
            }
            catch (Exception ex)
            {
                MostrarError(
                    "Error al consultar autores: " +
                    ex.Message);
            }
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

            LimpiarAutores();
        }

        private static void MostrarError(
            string mensaje)
        {
            MessageBox.Show(
                mensaje,
                "Aviso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        // EVENTOS ANTIGUOS DEL DISEÑADOR
        private void label1_Click(
            object? sender,
            EventArgs e)
        {
        }

        private void dataGridView1_CellContentClick_1(
            object? sender,
            DataGridViewCellEventArgs e)
        {
        }

        // MODELO PARA MOSTRAR AUTORES
        private sealed class AutorOpcion
        {
            public int AutorID { get; set; }

            public string Nombre { get; set; } = "";
            public string Apellido { get; set; } = "";
            public string Pais { get; set; } = "";

            public string NombreCompleto =>
                $"{Nombre} {Apellido}".Trim();

            public override string ToString()
            {
                return string.IsNullOrWhiteSpace(Pais)
                    ? NombreCompleto
                    : $"{NombreCompleto} — {Pais}";
            }
        }
    }
}
