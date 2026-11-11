
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Biblioteca.Business.Services;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Presentation
{
    public class EditorialesForm : Form
    {
        private readonly EditorialService servicio = new();

        private readonly TextBox txtNombre = new();
        private readonly TextBox txtPais = new();
        private readonly TextBox txtBuscar = new();

        private readonly DataGridView tabla = new();
        private readonly Label lblResultados = new();

        private readonly Color azul =
            Color.FromArgb(38, 78, 133);

        private readonly Color fondo =
            Color.FromArgb(245, 248, 252);

        private int editorialID = 0;
        private bool cargando = false;

        public EditorialesForm()
        {
            Text = "Biblioteca | Gestión de Editoriales";
            Size = new Size(1100, 750);
            MinimumSize = new Size(850, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = fondo;
            Font = new Font("Segoe UI", 10F);

            ConstruirInterfaz();

            txtBuscar.TextChanged += (s, e) =>
                Ejecutar(CargarEditoriales);

            tabla.SelectionChanged += Tabla_SelectionChanged;

            Load += (s, e) =>
                Ejecutar(CargarEditoriales);
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
                Text = "GESTIÓN DE EDITORIALES",
                Font = new Font(
                    "Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(25, 12)
            });

            encabezado.Controls.Add(new Label
            {
                Text = "Administración de editoriales y países de origen",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(220, 232, 248),
                AutoSize = true,
                Location = new Point(27, 56)
            });

            // CONTENEDOR PRINCIPAL
            var contenedor = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(25, 20, 25, 25),
                BackColor = fondo
            };

            // USAMOS TABLELAYOUTPANEL PARA EVITAR SUPERPOSICIONES
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
                new RowStyle(SizeType.Absolute, 170));

            estructura.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 110));

            estructura.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            contenedor.Controls.Add(estructura);

            // PANEL DE FORMULARIO
            var panelDatos = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 12)
            };

            estructura.Controls.Add(panelDatos, 0, 0);

            CrearCampo(
                panelDatos,
                "NOMBRE DE LA EDITORIAL",
                txtNombre,
                20, 20, 430);

            CrearCampo(
                panelDatos,
                "PAÍS DE ORIGEN",
                txtPais,
                480, 20, 350);

            // BOTONES
            var guardar = CrearBoton(
                "GUARDAR",
                Color.FromArgb(35, 105, 75));

            var editar = CrearBoton(
                "EDITAR", azul);

            var eliminar = CrearBoton(
                "ELIMINAR",
                Color.FromArgb(170, 55, 65));

            var limpiar = CrearBoton(
                "LIMPIAR",
                Color.FromArgb(95, 105, 120));

            guardar.Location = new Point(20, 100);
            editar.Location = new Point(190, 100);
            eliminar.Location = new Point(360, 100);
            limpiar.Location = new Point(530, 100);

            panelDatos.Controls.AddRange(new Control[]
            {
                guardar, editar, eliminar, limpiar
            });

            // GUARDAR
            guardar.Click += (s, e) => Ejecutar(() =>
            {
                ValidarDatos();

                servicio.RegistrarEditorial(LeerEditorial());

                MessageBox.Show(
                    "Editorial registrada correctamente.",
                    "Registro exitoso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarEditoriales();
                Limpiar();
            });

            // EDITAR
            editar.Click += (s, e) => Ejecutar(() =>
            {
                if (editorialID <= 0)
                    throw new Exception(
                        "Selecciona una editorial para editar.");

                ValidarDatos();

                servicio.ActualizarEditorial(LeerEditorial());

                MessageBox.Show(
                    "Editorial actualizada correctamente.",
                    "Actualización exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarEditoriales();
                Limpiar();
            });

            // ELIMINAR
            eliminar.Click += (s, e) => Ejecutar(() =>
            {
                if (editorialID <= 0)
                    throw new Exception(
                        "Selecciona una editorial para eliminar.");

                if (MessageBox.Show(
                    "¿Seguro que deseas eliminar esta editorial?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question)
                    != DialogResult.Yes)
                {
                    return;
                }

                servicio.EliminarEditorial(editorialID);

                MessageBox.Show(
                    "Editorial eliminada correctamente.",
                    "Eliminación exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarEditoriales();
                Limpiar();
            });

            // LIMPIAR
            limpiar.Click += (s, e) => Limpiar();

            // PANEL DE BÚSQUEDA
            var panelBusqueda = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                BackColor = fondo
            };

            estructura.Controls.Add(panelBusqueda, 0, 1);

            panelBusqueda.Controls.Add(new Label
            {
                Text = "BUSCAR EDITORIAL",
                Font = new Font(
                    "Segoe UI", 10F, FontStyle.Bold),
                ForeColor = azul,
                AutoSize = true,
                Location = new Point(0, 17)
            });

            txtBuscar.PlaceholderText =
                "Buscar por nombre o país...";

            txtBuscar.Location = new Point(0, 47);
            txtBuscar.Size = new Size(500, 32);
            txtBuscar.BorderStyle = BorderStyle.FixedSingle;

            panelBusqueda.Controls.Add(txtBuscar);

            lblResultados.Location = new Point(530, 51);
            lblResultados.AutoSize = true;
            lblResultados.ForeColor =
                Color.FromArgb(80, 90, 105);

            panelBusqueda.Controls.Add(lblResultados);

            // PANEL DE TABLA
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
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

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

            // ORDEN CORRECTO: ENCABEZADO ARRIBA Y CONTENIDO DEBAJO
            // El encabezado ocupa los primeros 90 píxeles.
            // El contenido comienza justo debajo.
            encabezado.Dock = DockStyle.Top;

            contenedor.Dock = DockStyle.Fill;

            // En WinForms, el orden de acoplamiento importa.
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

            campo.Location = new Point(x, y + 28);
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

        private void CargarEditoriales()
        {
            cargando = true;

            try
            {
                var editoriales = servicio.ObtenerEditoriales();

                string busqueda = txtBuscar.Text.Trim();

                if (!string.IsNullOrWhiteSpace(busqueda))
                {
                    editoriales = editoriales
                        .Where(e =>
                            (e.Nombre ?? "").Contains(
                                busqueda,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            (e.Pais ?? "").Contains(
                                busqueda,
                                StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                tabla.DataSource = null;
                tabla.DataSource = editoriales;

                if (tabla.Columns.Contains("EditorialID"))
                    tabla.Columns["EditorialID"].HeaderText = "ID";

                if (tabla.Columns.Contains("Nombre"))
                    tabla.Columns["Nombre"].HeaderText =
                        "NOMBRE DE LA EDITORIAL";

                if (tabla.Columns.Contains("Pais"))
                    tabla.Columns["Pais"].HeaderText =
                        "PAÍS DE ORIGEN";

                tabla.ClearSelection();

                lblResultados.Text = editoriales.Count == 1
                    ? "1 editorial encontrada"
                    : $"{editoriales.Count} editoriales encontradas";

                editorialID = 0;
            }
            finally
            {
                cargando = false;
            }
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
                is not Editorial editorial)
                return;

            editorialID = editorial.EditorialID;
            txtNombre.Text = editorial.Nombre;
            txtPais.Text = editorial.Pais ?? "";
        }

        private void ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
                throw new Exception(
                    "El nombre de la editorial es obligatorio.");

            if (txtNombre.Text.Trim().Length < 2)
                throw new Exception(
                    "El nombre debe tener al menos 2 caracteres.");

            if (string.IsNullOrWhiteSpace(txtPais.Text))
                throw new Exception(
                    "El país de origen es obligatorio.");
        }

        private Editorial LeerEditorial()
        {
            return new Editorial
            {
                EditorialID = editorialID,
                Nombre = txtNombre.Text.Trim(),
                Pais = txtPais.Text.Trim()
            };
        }

        private void Limpiar()
        {
            editorialID = 0;
            txtNombre.Clear();
            txtPais.Clear();
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
