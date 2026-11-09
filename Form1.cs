
using System;
using System.Windows.Forms;
using Biblioteca.Business.Services;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Presentation
{
    public partial class Form1 : Form
    {
        private readonly LibroService _libroService = new();

        private int _libroSeleccionadoID = 0;
        private bool _cargandoLibros = false;

        public Form1()
        {
            InitializeComponent();

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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CargarLibros();
        }

        private void CargarLibros()
        {
            try
            {
                _cargandoLibros = true;

                dgvLibros.DataSource = null;
                dgvLibros.DataSource =
                    _libroService.ObtenerLibros();

                dgvLibros.ClearSelection();
                _libroSeleccionadoID = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar libros: " + ex.Message);
            }
            finally
            {
                _cargandoLibros = false;
            }
        }

        private Libro ObtenerDatosFormulario()
        {
            if (!int.TryParse(
                txtEditorialID.Text, out int editorialID))
            {
                throw new Exception(
                    "El ID de la editorial debe ser un número.");
            }

            if (!int.TryParse(
                txtTotal.Text, out int total))
            {
                throw new Exception(
                    "El total de ejemplares debe ser un número.");
            }

            if (!int.TryParse(
                txtDisponibles.Text, out int disponibles))
            {
                throw new Exception(
                    "Los ejemplares disponibles deben ser un número.");
            }

            int? anio = null;

            if (!string.IsNullOrWhiteSpace(txtAnio.Text))
            {
                if (!int.TryParse(
                    txtAnio.Text, out int valorAnio))
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
                        "Primero selecciona un libro de la tabla.");
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
            txtEditorialID.Text =
                libro.EditorialID.ToString();
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
            txtEditorialID.Clear();
            txtAnio.Clear();
            txtTotal.Clear();
            txtDisponibles.Clear();

            _libroSeleccionadoID = 0;
            dgvLibros.ClearSelection();
        }

        // Eventos antiguos del diseñador
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
