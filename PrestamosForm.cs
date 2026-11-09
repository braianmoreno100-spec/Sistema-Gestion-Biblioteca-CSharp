
using System;
using System.Data;
using System.Drawing;
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
        private readonly DateTimePicker dtpVencimiento = new();
        private readonly DataGridView tabla = new();

        private int detalleSeleccionadoID = 0;

        public PrestamosForm()
        {
            Text = "Gestión de Préstamos y Devoluciones";
            Size = new Size(1050, 650);
            MinimumSize = new Size(800, 500);
            StartPosition = FormStartPosition.CenterScreen;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 165,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(15)
            };

            panel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 170));
            panel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            cmbUsuario.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cmbLibro.DropDownStyle =
                ComboBoxStyle.DropDownList;

            dtpVencimiento.Format =
                DateTimePickerFormat.Short;

            dtpVencimiento.Value =
                DateTime.Today.AddDays(7);

            AgregarCampo(panel, "Usuario:", cmbUsuario, 0);
            AgregarCampo(panel, "Libro:", cmbLibro, 1);
            AgregarCampo(
                panel, "Fecha vencimiento:", dtpVencimiento, 2);

            var botones = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(15, 5, 0, 0)
            };

            var btnPrestar = new Button
            {
                Text = "REGISTRAR PRÉSTAMO",
                Width = 180,
                Height = 35
            };

            var btnDevolver = new Button
            {
                Text = "REGISTRAR DEVOLUCIÓN",
                Width = 190,
                Height = 35
            };

            var btnActualizar = new Button
            {
                Text = "ACTUALIZAR",
                Width = 130,
                Height = 35
            };

            botones.Controls.AddRange(new Control[]
            {
                btnPrestar, btnDevolver, btnActualizar
            });

            tabla.Dock = DockStyle.Fill;
            tabla.ReadOnly = true;
            tabla.AllowUserToAddRows = false;
            tabla.AllowUserToDeleteRows = false;
            tabla.MultiSelect = false;
            tabla.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            tabla.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            tabla.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0)
                    return;

                DataGridViewRow fila =
                    tabla.Rows[e.RowIndex];

                if (fila.Cells["DetalleID"].Value == null)
                    return;

                detalleSeleccionadoID = Convert.ToInt32(
                    fila.Cells["DetalleID"].Value);
            };

            btnPrestar.Click += (s, e) => Ejecutar(() =>
            {
                if (cmbUsuario.SelectedValue is not int usuarioID ||
                    cmbLibro.SelectedValue is not int libroID)
                {
                    throw new Exception(
                        "Selecciona un usuario y un libro.");
                }

                servicio.RegistrarPrestamo(
                    usuarioID,
                    libroID,
                    dtpVencimiento.Value.Date);

                MessageBox.Show(
                    "Préstamo registrado correctamente.");

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
                        "El préstamo ya fue devuelto.");
                }

                if (MessageBox.Show(
                    "¿Confirmas la devolución del libro?",
                    "Confirmar devolución",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                servicio.RegistrarDevolucion(
                    detalleSeleccionadoID);

                MessageBox.Show(
                    "Devolución registrada correctamente.");

                Recargar();
            });

            btnActualizar.Click += (s, e) =>
                Ejecutar(Recargar);

            Controls.Add(tabla);
            Controls.Add(botones);
            Controls.Add(panel);

            Load += (s, e) => Ejecutar(Recargar);
        }

        private static void AgregarCampo(
            TableLayoutPanel panel,
            string etiqueta,
            Control control,
            int fila)
        {
            panel.Controls.Add(new Label
            {
                Text = etiqueta,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            }, 0, fila);

            control.Dock = DockStyle.Fill;
            panel.Controls.Add(control, 1, fila);
        }

        private void Recargar()
        {
            var listaUsuarios = usuarios.ObtenerUsuarios();

            cmbUsuario.DataSource = null;
            cmbUsuario.DataSource = listaUsuarios;
            cmbUsuario.DisplayMember = "NombreCompleto";
            cmbUsuario.ValueMember = "UsuarioID";

            // Mostrar nombre y apellido en la lista.
            cmbUsuario.Format += FormatearUsuario;
            cmbUsuario.FormattingEnabled = true;
            cmbUsuario.DisplayMember = "Nombre";

            var listaLibros = libros.ObtenerLibros();

            cmbLibro.DataSource = null;
            cmbLibro.DataSource = listaLibros;
            cmbLibro.DisplayMember = "Titulo";
            cmbLibro.ValueMember = "LibroID";

            tabla.DataSource = null;
            tabla.DataSource = servicio.ObtenerPrestamos();

            detalleSeleccionadoID = 0;
            tabla.ClearSelection();
        }

        private void FormatearUsuario(
            object? sender, ListControlConvertEventArgs e)
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
