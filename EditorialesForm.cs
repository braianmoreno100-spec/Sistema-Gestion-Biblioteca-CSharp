
using System;
using System.Drawing;
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
        private readonly DataGridView tabla = new();

        private int editorialID = 0;

        public EditorialesForm()
        {
            Text = "Gestión de Editoriales";
            Size = new Size(850, 520);
            MinimumSize = new Size(650, 420);
            StartPosition = FormStartPosition.CenterScreen;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 120,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(15)
            };

            panel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 110));
            panel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            AgregarCampo(panel, "Nombre:", txtNombre, 0);
            AgregarCampo(panel, "País:", txtPais, 1);

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
                servicio.RegistrarEditorial(LeerEditorial());
                MessageBox.Show("Editorial registrada.");
                Recargar();
            });

            editar.Click += (s, e) => Ejecutar(() =>
            {
                if (editorialID <= 0)
                    throw new Exception("Selecciona una editorial.");

                servicio.ActualizarEditorial(LeerEditorial());
                MessageBox.Show("Editorial actualizada.");
                Recargar();
            });

            eliminar.Click += (s, e) => Ejecutar(() =>
            {
                if (editorialID <= 0)
                    throw new Exception("Selecciona una editorial.");

                if (MessageBox.Show(
                    "¿Eliminar esta editorial?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                servicio.EliminarEditorial(editorialID);
                MessageBox.Show("Editorial eliminada.");
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
                    is not Editorial editorial)
                    return;

                editorialID = editorial.EditorialID;
                txtNombre.Text = editorial.Nombre;
                txtPais.Text = editorial.Pais ?? "";
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

        private Editorial LeerEditorial()
        {
            return new Editorial
            {
                EditorialID = editorialID,
                Nombre = txtNombre.Text.Trim(),
                Pais = txtPais.Text.Trim()
            };
        }

        private void Recargar()
        {
            tabla.DataSource = null;
            tabla.DataSource = servicio.ObtenerEditoriales();
            Limpiar();
        }

        private void Limpiar()
        {
            editorialID = 0;
            txtNombre.Clear();
            txtPais.Clear();
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
