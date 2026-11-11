
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Biblioteca.Business.Services;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Presentation
{
    public class PrestamosForm : Form
    {
        private readonly PrestamoService servicio = new();
        private readonly UsuarioService usuarios = new();
        private readonly LibroService libros = new();

        private readonly ComboBox cmbUsuario = new();
        private readonly ComboBox cmbLibro = new();
        private readonly ComboBox cmbEstado = new();

        private readonly DateTimePicker dtpVencimiento = new();
        private readonly TextBox txtBuscar = new();

        private readonly DataGridView tabla = new();
        private readonly Label lblResultados = new();
        private readonly Label lblDisponibilidad = new();

        private readonly Color azul =
            Color.FromArgb(38, 78, 133);

        private readonly Color fondo =
            Color.FromArgb(245, 248, 252);

        private int detalleSeleccionadoID = 0;
        private bool cargando = false;

        private DataTable? datosPrestamos;

        public PrestamosForm()
        {
            Text = "Biblioteca | Gestión de Préstamos";
            Size = new Size(1250, 850);
            MinimumSize = new Size(950, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = fondo;
            Font = new Font("Segoe UI", 10F);

            ConstruirInterfaz();

            cmbUsuario.FormattingEnabled = true;
            cmbUsuario.Format += FormatearUsuario;

            cmbLibro.SelectedIndexChanged +=
                (s, e) => ActualizarDisponibilidad();

            txtBuscar.TextChanged +=
                (s, e) => FiltrarPrestamos();

            cmbEstado.SelectedIndexChanged +=
                (s, e) => FiltrarPrestamos();

            tabla.SelectionChanged +=
                Tabla_SelectionChanged;

            Load += (s, e) => Ejecutar(Recargar);
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
                Text = "GESTIÓN DE PRÉSTAMOS",
                Font = new Font(
                    "Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 12)
            });

            encabezado.Controls.Add(new Label
            {
                Text = "Registro de préstamos, devoluciones y control de disponibilidad",
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
                new RowStyle(SizeType.Absolute, 245));

            estructura.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 120));

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
                panelDatos,
                "USUARIO",
                cmbUsuario,
                20, 18, 520);

            CrearCampo(
                panelDatos,
                "LIBRO",
                cmbLibro,
                570, 18, 520);

            CrearCampo(
                panelDatos,
                "FECHA DE VENCIMIENTO",
                dtpVencimiento,
                20, 90, 300);

            cmbUsuario.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cmbLibro.DropDownStyle =
                ComboBoxStyle.DropDownList;

            dtpVencimiento.Format =
                DateTimePickerFormat.Short;

            dtpVencimiento.Value =
                DateTime.Today.AddDays(7);

            lblDisponibilidad.Location =
                new Point(570, 120);

            lblDisponibilidad.AutoSize = true;
            lblDisponibilidad.ForeColor = azul;
            lblDisponibilidad.Font = new Font(
                "Segoe UI", 10F, FontStyle.Bold);

            panelDatos.Controls.Add(lblDisponibilidad);

            // BOTONES
            var btnPrestar = CrearBoton(
                "REGISTRAR PRÉSTAMO",
                Color.FromArgb(35, 105, 75),
                220);

            var btnDevolver = CrearBoton(
                "REGISTRAR DEVOLUCIÓN",
                azul,
                240);

            var btnActualizar = CrearBoton(
                "ACTUALIZAR",
                Color.FromArgb(95, 105, 120),
                165);

            btnPrestar.Location = new Point(20, 175);
            btnDevolver.Location = new Point(255, 175);
            btnActualizar.Location = new Point(510, 175);

            panelDatos.Controls.AddRange(new Control[]
            {
                btnPrestar, btnDevolver, btnActualizar
            });

            btnPrestar.Click += (s, e) => Ejecutar(() =>
            {
                if (cmbUsuario.SelectedValue is not int usuarioID)
                    throw new Exception(
                        "Selecciona un usuario.");

                if (cmbLibro.SelectedValue is not int libroID)
                    throw new Exception(
                        "Selecciona un libro.");

                if (dtpVencimiento.Value.Date <= DateTime.Today)
                    throw new Exception(
                        "La fecha de vencimiento debe ser posterior a hoy.");

                if (cmbLibro.SelectedItem is Libro libro &&
                    libro.EjemplaresDisponibles <= 0)
                {
                    throw new Exception(
                        "Este libro no tiene ejemplares disponibles.");
                }

                servicio.RegistrarPrestamo(
                    usuarioID,
                    libroID,
                    dtpVencimiento.Value.Date);

                MessageBox.Show(
                    "Préstamo registrado correctamente.",
                    "Registro exitoso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Recargar();
            });

            btnDevolver.Click += (s, e) => Ejecutar(() =>
            {
                if (detalleSeleccionadoID <= 0)
                    throw new Exception(
                        "Selecciona un préstamo de la tabla.");

                if (tabla.CurrentRow?.Cells["Estado"].Value
                    ?.ToString() != "Activo")
                {
                    throw new Exception(
                        "Solo puedes devolver préstamos activos.");
                }

                if (MessageBox.Show(
                    "¿Confirmas la devolución del libro?",
                    "Confirmar devolución",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question)
                    != DialogResult.Yes)
                    return;

                servicio.RegistrarDevolucion(
                    detalleSeleccionadoID);

                MessageBox.Show(
                    "Devolución registrada correctamente.",
                    "Devolución exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Recargar();
            });

            btnActualizar.Click += (s, e) =>
                Ejecutar(Recargar);

            // PANEL DE BÚSQUEDA
            var panelBusqueda = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = fondo,
                Margin = Padding.Empty
            };

            estructura.Controls.Add(panelBusqueda, 0, 1);

            panelBusqueda.Controls.Add(new Label
            {
                Text = "BUSCAR PRÉSTAMO",
                Font = new Font(
                    "Segoe UI", 10F, FontStyle.Bold),
                ForeColor = azul,
                AutoSize = true,
                Location = new Point(0, 15)
            });

            txtBuscar.PlaceholderText =
                "Buscar por usuario, libro, estado o identificador...";

            txtBuscar.Location = new Point(0, 45);
            txtBuscar.Size = new Size(500, 32);
            txtBuscar.BorderStyle = BorderStyle.FixedSingle;

            panelBusqueda.Controls.Add(txtBuscar);

            panelBusqueda.Controls.Add(new Label
            {
                Text = "ESTADO",
                Font = new Font(
                    "Segoe UI", 10F, FontStyle.Bold),
                ForeColor = azul,
                AutoSize = true,
                Location = new Point(530, 15)
            });

            cmbEstado.Location = new Point(530, 45);
            cmbEstado.Size = new Size(180, 32);
            cmbEstado.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cmbEstado.Items.AddRange(new object[]
            {
                "Todos",
                "Activo",
                "Devuelto"
            });

            cmbEstado.SelectedIndex = 0;
            panelBusqueda.Controls.Add(cmbEstado);

            lblResultados.Location = new Point(0, 87);
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

            tabla.ColumnHeadersDefaultCellStyle.BackColor =
                azul;

            tabla.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            tabla.ColumnHeadersDefaultCellStyle.Font =
                new Font(
                    "Segoe UI", 10F, FontStyle.Bold);

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
            Control campo,
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
            campo.Size = new Size(ancho, 32);

            contenedor.Controls.Add(campo);
        }

        private static Button CrearBoton(
            string texto,
            Color color,
            int ancho)
        {
            var boton = new Button
            {
                Text = texto,
                Size = new Size(ancho, 44),
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

        private void Recargar()
        {
            cargando = true;

            try
            {
                var listaUsuarios = usuarios.ObtenerUsuarios();

                cmbUsuario.DataSource = null;
                cmbUsuario.DisplayMember = "Nombre";
                cmbUsuario.ValueMember = "UsuarioID";
                cmbUsuario.DataSource = listaUsuarios;
                cmbUsuario.SelectedIndex = -1;

                var listaLibros = libros.ObtenerLibros();

                cmbLibro.DataSource = null;
                cmbLibro.DisplayMember = "Titulo";
                cmbLibro.ValueMember = "LibroID";
                cmbLibro.DataSource = listaLibros;
                cmbLibro.SelectedIndex = -1;

                lblDisponibilidad.Text =
                    "Selecciona un libro para consultar disponibilidad.";

                // PrestamoService devuelve DataTable.
                datosPrestamos = servicio.ObtenerPrestamos();

                FiltrarPrestamos();

                detalleSeleccionadoID = 0;
                tabla.ClearSelection();
            }
            finally
            {
                cargando = false;
            }
        }

        private void FiltrarPrestamos()
        {
            if (datosPrestamos == null)
                return;

            cargando = true;

            try
            {
                string busqueda = txtBuscar.Text.Trim();
                string estado = cmbEstado.SelectedItem
                    ?.ToString() ?? "Todos";

                var vista = new DataView(datosPrestamos);

                var condiciones = new System.Collections.Generic
                    .List<string>();

                if (!string.IsNullOrWhiteSpace(busqueda))
                {
                    string texto = busqueda
                        .Replace("'", "''")
                        .Replace("[", "[[]")
                        .Replace("%", "[%]")
                        .Replace("*", "[*]");

                    var columnas = datosPrestamos.Columns
                        .Cast<DataColumn>()
                        .Where(c => c.DataType == typeof(string))
                        .Select(c =>
                            $"CONVERT([{c.ColumnName}], 'System.String') LIKE '%{texto}%'")
                        .ToList();

                    if (columnas.Count > 0)
                        condiciones.Add(
                            "(" + string.Join(" OR ", columnas) + ")");
                }

                if (estado != "Todos" &&
                    datosPrestamos.Columns.Contains("Estado"))
                {
                    condiciones.Add(
                        $"[Estado] = '{estado}'");
                }

                vista.RowFilter = string.Join(
                    " AND ", condiciones);

                tabla.DataSource = null;
                tabla.DataSource = vista;

                CambiarTitulo("DetalleID", "DETALLE ID");
                CambiarTitulo("PrestamoID", "PRÉSTAMO ID");
                CambiarTitulo("Estado", "ESTADO");

                tabla.ClearSelection();

                lblResultados.Text = vista.Count == 1
                    ? "1 préstamo encontrado"
                    : $"{vista.Count} préstamos encontrados";

                detalleSeleccionadoID = 0;
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

            detalleSeleccionadoID = 0;

            if (tabla.SelectedRows.Count == 0)
                return;

            var fila = tabla.SelectedRows[0];

            if (!tabla.Columns.Contains("DetalleID"))
                return;

            if (fila.Cells["DetalleID"].Value == null)
                return;

            detalleSeleccionadoID = Convert.ToInt32(
                fila.Cells["DetalleID"].Value);
        }

        private void ActualizarDisponibilidad()
        {
            if (cmbLibro.SelectedItem is Libro libro)
            {
                lblDisponibilidad.Text =
                    $"Ejemplares disponibles: {libro.EjemplaresDisponibles}";

                lblDisponibilidad.ForeColor =
                    libro.EjemplaresDisponibles > 0
                        ? Color.FromArgb(35, 105, 75)
                        : Color.FromArgb(170, 55, 65);
            }
            else
            {
                lblDisponibilidad.Text =
                    "Selecciona un libro para consultar disponibilidad.";
            }
        }

        private void FormatearUsuario(
            object? sender,
            ListControlConvertEventArgs e)
        {
            if (e.ListItem is Usuario usuario)
            {
                e.Value =
                    $"{usuario.Nombre} {usuario.Apellido}";
            }
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
