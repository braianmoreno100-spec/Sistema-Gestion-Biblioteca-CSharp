
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Biblioteca.Data.Database;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Presentation
{
    public class ReportesForm : Form
    {
        private readonly DataGridView tabla = new();
        private readonly Label lblResumen = new();

        public ReportesForm()
        {
            Text = "Reportes de Biblioteca";
            Size = new Size(1050, 650);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(800, 500);

            var encabezado = new Label
            {
                Text = "REPORTES DEL SISTEMA",
                Dock = DockStyle.Top,
                Height = 70,
                Font = new Font("Segoe UI", 19, FontStyle.Bold),
                ForeColor = Color.FromArgb(35, 55, 90),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var botones = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(12, 8, 0, 0)
            };

            AgregarBoton(botones, "LIBROS DISPONIBLES",
                () => MostrarReporte(
                    @"SELECT LibroID, Titulo, ISBN,
                             TotalEjemplares, EjemplaresDisponibles
                      FROM Libros
                      WHERE EjemplaresDisponibles > 0
                      ORDER BY Titulo",
                    "Libros disponibles"));

            AgregarBoton(botones, "PRÉSTAMOS ACTIVOS",
                () => MostrarReporte(
                    ConsultaPrestamos +
                    " WHERE d.Estado = 'Activo' ORDER BY p.FechaVencimiento",
                    "Préstamos activos"));

            AgregarBoton(botones, "PRÉSTAMOS VENCIDOS",
                () => MostrarReporte(
                    ConsultaPrestamos +
                    @" WHERE d.Estado = 'Activo'
                       AND p.FechaVencimiento < CAST(GETDATE() AS DATE)
                       ORDER BY p.FechaVencimiento",
                    "Préstamos vencidos"));

            AgregarBoton(botones, "HISTORIAL COMPLETO",
                () => MostrarReporte(
                    ConsultaPrestamos +
                    " ORDER BY p.PrestamoID DESC",
                    "Historial de préstamos"));

            lblResumen.Dock = DockStyle.Bottom;
            lblResumen.Height = 45;
            lblResumen.Font = new Font("Segoe UI", 11);
            lblResumen.TextAlign = ContentAlignment.MiddleLeft;
            lblResumen.Padding = new Padding(15, 0, 0, 0);

            tabla.Dock = DockStyle.Fill;
            tabla.ReadOnly = true;
            tabla.AllowUserToAddRows = false;
            tabla.AllowUserToDeleteRows = false;
            tabla.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            tabla.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            Controls.Add(tabla);
            Controls.Add(lblResumen);
            Controls.Add(botones);
            Controls.Add(encabezado);

            Load += (s, e) => MostrarReporte(
                "SELECT LibroID, Titulo, ISBN, TotalEjemplares, " +
                "EjemplaresDisponibles FROM Libros ORDER BY Titulo",
                "Inventario de libros");
        }

        private const string ConsultaPrestamos = @"
            SELECT
                p.PrestamoID,
                d.DetalleID,
                u.Nombre + ' ' + u.Apellido AS Usuario,
                l.Titulo AS Libro,
                p.FechaPrestamo,
                p.FechaVencimiento,
                d.FechaDevolucion,
                d.Estado
            FROM DetallePrestamo d
            INNER JOIN Prestamos p
                ON d.PrestamoID = p.PrestamoID
            INNER JOIN Usuarios u
                ON p.UsuarioID = u.UsuarioID
            INNER JOIN Libros l
                ON d.LibroID = l.LibroID";

        private static void AgregarBoton(
            FlowLayoutPanel panel,
            string titulo,
            Action accion)
        {
            var boton = new Button
            {
                Text = titulo,
                Width = 190,
                Height = 38,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            boton.Click += (s, e) => accion();
            panel.Controls.Add(boton);
        }

        private void MostrarReporte(string sql, string titulo)
        {
            try
            {
                using var conexion =
                    ConnectionFactory.CreateConnection();

                conexion.Open();

                using var adaptador =
                    new SqlDataAdapter(sql, conexion);

                var datos = new DataTable();
                adaptador.Fill(datos);

                tabla.DataSource = datos;

                lblResumen.Text =
                    $"{titulo} | Registros encontrados: {datos.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al generar reporte: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
