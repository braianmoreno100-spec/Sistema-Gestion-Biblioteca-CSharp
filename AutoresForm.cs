
using System;
using System.Drawing;
using System.Windows.Forms;
using Biblioteca.Business.Services;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Presentation
{
    public class AutoresForm : Form
    {
        private readonly AutorService servicio = new();

        private readonly TextBox txtNombre = new();
        private readonly TextBox txtApellido = new();
        private readonly TextBox txtPais = new();

        private readonly DataGridView tabla = new();
        private int autorID = 0;

        public AutoresForm()
        {
            Text = "Gestión de Autores";
            Size = new Size(850, 560);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(700, 450);

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 170,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(15)
            };

            panel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 110));
            panel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            AgregarCampo(panel, "Nombre:", txtNombre, 0);
            AgregarCampo(panel, "Apellido:", txtApellido, 1);
            AgregarCampo(panel, "País:", txtPais, 2);

            var botones = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
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
                servicio.RegistrarAutor(LeerAutor());
                MessageBox.Show("Autor registrado.");
                Recargar();
            });

            editar.Click += (s, e) => Ejecutar(() =>
            {
                if (autorID <= 0)
                    throw new Exception("Selecciona un autor.");

                servicio.ActualizarAutor(LeerAutor());
                MessageBox.Show("Autor actualizado.");
                Recargar();
            });

            eliminar.Click += (s, e) => Ejecutar(() =>
            {
                if (autorID <= 0)
                    throw new Exception("Selecciona un autor.");

                if (MessageBox.Show(
                    "¿Eliminar este autor?",
                    "Confirmar",
                    MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;

                servicio.EliminarAutor(autorID);
                MessageBox.Show("Autor eliminado.");
                Recargar();
            });

            limpiar.Click += (s, e) => Limpiar();

            botones.Controls.AddRange(
                new Control[] { guardar, editar, eliminar, limpiar });

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
                if (e.RowIndex < 0) return;

                if (tabla.Rows[e.RowIndex].DataBoundItem
                    is not Autor autor) return;

                autorID = autor.AutorID;
                txtNombre.Text = autor.Nombre;
                txtApellido.Text = autor.Apellido;
                txtPais.Text = autor.Pais ?? "";
            };

            Controls.Add(tabla);
            Controls.Add(botones);
            Controls.Add(panel);

            Load += (s, e) => Ejecutar(Recargar);
        }

        private static void AgregarCampo(
            TableLayoutPanel panel,
            string nombre,
            TextBox campo,
            int fila)
        {
            panel.Controls.Add(new Label
            {
                Text = nombre,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            }, 0, fila);

            campo.Dock = DockStyle.Fill;
            panel.Controls.Add(campo, 1, fila);
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
            tabla.DataSource = null;
            tabla.DataSource = servicio.ObtenerAutores();
            Limpiar();
        }

        private void Limpiar()
        {
            autorID = 0;
            txtNombre.Clear();
            txtApellido.Clear();
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
