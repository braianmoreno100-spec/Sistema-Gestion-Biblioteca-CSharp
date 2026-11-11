
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Biblioteca.Presentation
{
    public class MenuPrincipalForm : Form
    {
        public MenuPrincipalForm()
        {
            Text = "Sistema de Gestión de Biblioteca";
            Size = new Size(950, 620);
            MinimumSize = new Size(750, 500);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 247, 251);

            var encabezado = new Label
            {
                Dock = DockStyle.Top,
                Height = 95,
                Font = new Font("Segoe UI", 21, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(35, 55, 90)
            };

            var subtitulo = new Label
            {
                Text = "Panel principal de administración",
                Dock = DockStyle.Top,
                Height = 45,
                Font = new Font("Segoe UI", 12),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(35)
            };

            AgregarBoton(panel, "LIBROS",
                () => new Form1());

            AgregarBoton(panel, "AUTORES",
                () => new AutoresForm());

            AgregarBoton(panel, "EDITORIALES",
                () => new EditorialesForm());

            AgregarBoton(panel, "USUARIOS",
                () => new UsuariosForm());

            AgregarBoton(panel, "PRÉSTAMOS Y DEVOLUCIONES",
                () => new PrestamosForm());

            AgregarBoton(panel, "REPORTES",
                () => new ReportesForm());

            Controls.Add(panel);
            Controls.Add(subtitulo);
            Controls.Add(encabezado);
        }

        private static void AgregarBoton(
            FlowLayoutPanel panel,
            string titulo,
            Func<Form> crearFormulario)
        {
            var boton = new Button
            {
                Text = titulo,
                Width = 250,
                Height = 105,
                Margin = new Padding(12),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(38, 76, 130),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            boton.FlatAppearance.BorderSize = 0;

            boton.Click += (s, e) =>
            {
                using var formulario = crearFormulario();
                formulario.ShowDialog();
            };

            panel.Controls.Add(boton);
        }
    }
}
