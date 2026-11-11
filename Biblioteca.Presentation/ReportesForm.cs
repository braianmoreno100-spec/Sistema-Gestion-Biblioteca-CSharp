
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Biblioteca.Data.Database;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Presentation
{
    public class ReportesForm : Form
    {
        private readonly Color azul = Color.FromArgb(38, 78, 133);
        private readonly Color fondo = Color.FromArgb(245, 248, 252);
        private readonly Color verde = Color.FromArgb(35, 105, 75);
        private readonly Color rojo = Color.FromArgb(170, 55, 65);

        private readonly DataGridView tabla = new();
        private readonly ComboBox cboReporte = new();
        private readonly DateTimePicker dtpDesde = new();
        private readonly DateTimePicker dtpHasta = new();
        private readonly TextBox txtBuscar = new();
        private readonly Label lblResumen = new();
        private readonly Label lblActualizacion = new();
        private readonly Panel panelGrafico = new();

        private readonly Dictionary<string, Label> indicadores = new();
        private readonly List<(string Mes, int Cantidad)> meses = new();

        private DataTable datosActuales = new();
        private string tituloActual = "Inventario general";
        private bool inicializado = false;

        private PrintDocument? documentoImpresion;
        private int filaImpresion = 0;

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

        public ReportesForm()
        {
            Text = "Biblioteca | Dashboard de Analítica";
            Size = new Size(1350, 950);
            MinimumSize = new Size(1020, 760);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = fondo;
            Font = new Font("Segoe UI", 10F);
            AutoScaleMode = AutoScaleMode.Font;

            ConstruirInterfaz();

            Load += (s, e) =>
            {
                inicializado = true;
                Ejecutar(ActualizarDashboard);
            };
        }

        private void ConstruirInterfaz()
        {
            SuspendLayout();

            // ENCABEZADO
            var encabezado = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = azul
            };

            encabezado.Controls.Add(new Label
            {
                Text = "DASHBOARD DE ANALÍTICA",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(25, 10)
            });

            encabezado.Controls.Add(new Label
            {
                Text = "Indicadores, tendencias y reportes de la biblioteca",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(220, 232, 248),
                AutoSize = true,
                Location = new Point(27, 54)
            });

            // CONTENIDO DESPLAZABLE
            var contenedor = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = fondo
            };

            var contenido = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(22, 18, 22, 22),
                BackColor = fondo
            };

            contenido.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            contenido.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 172));
            contenido.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 90));
            contenido.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 220));
            contenido.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 105));
            contenido.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 370));
            contenido.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 40));

            contenedor.Controls.Add(contenido);

            // Adaptar el contenido al ancho disponible.
            contenedor.Resize += (s, e) =>
            {
                int ancho = Math.Max(
                    940, contenedor.ClientSize.Width - 3);

                if (contenido.Width != ancho)
                    contenido.Width = ancho;
            };

            // INDICADORES
            var panelIndicadores = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                Margin = new Padding(0, 0, 0, 12)
            };

            for (int i = 0; i < 4; i++)
                panelIndicadores.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, 25));

            for (int i = 0; i < 2; i++)
                panelIndicadores.RowStyles.Add(
                    new RowStyle(SizeType.Percent, 50));

            contenido.Controls.Add(panelIndicadores, 0, 0);

            AgregarIndicador(panelIndicadores, 0, 0,
                "LIBROS REGISTRADOS", "Libros", azul);

            AgregarIndicador(panelIndicadores, 1, 0,
                "EJEMPLARES TOTALES", "Ejemplares", azul);

            AgregarIndicador(panelIndicadores, 2, 0,
                "EJEMPLARES DISPONIBLES", "Disponibles", verde);

            AgregarIndicador(panelIndicadores, 3, 0,
                "USUARIOS", "Usuarios", azul);

            AgregarIndicador(panelIndicadores, 0, 1,
                "PRÉSTAMOS ACTIVOS", "Activos", azul);

            AgregarIndicador(panelIndicadores, 1, 1,
                "PRÉSTAMOS VENCIDOS", "Vencidos", rojo);

            AgregarIndicador(panelIndicadores, 2, 1,
                "DEVOLUCIONES", "Devoluciones", verde);

            AgregarIndicador(panelIndicadores, 3, 1,
                "PRÉSTAMOS HISTÓRICOS", "Historicos", azul);

            // FILTROS DE FECHA
            var panelFiltros = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 12)
            };

            contenido.Controls.Add(panelFiltros, 0, 1);

            CrearEtiqueta(panelFiltros,
                "DESDE", 16, 8);

            dtpDesde.Format = DateTimePickerFormat.Short;
            dtpDesde.Value = DateTime.Today.AddMonths(-12);
            dtpDesde.Location = new Point(16, 33);
            dtpDesde.Width = 175;
            panelFiltros.Controls.Add(dtpDesde);

            CrearEtiqueta(panelFiltros,
                "HASTA", 212, 8);

            dtpHasta.Format = DateTimePickerFormat.Short;
            dtpHasta.Value = DateTime.Today;
            dtpHasta.Location = new Point(212, 33);
            dtpHasta.Width = 175;
            panelFiltros.Controls.Add(dtpHasta);

            var btnAplicar = CrearBoton(
                "APLICAR FECHAS", azul, 170);

            btnAplicar.Location = new Point(405, 28);
            panelFiltros.Controls.Add(btnAplicar);

            btnAplicar.Click += (s, e) =>
                Ejecutar(ActualizarDashboard);

            var btnActualizar = CrearBoton(
                "ACTUALIZAR TODO", verde, 185);

            btnActualizar.Location = new Point(590, 28);
            panelFiltros.Controls.Add(btnActualizar);

            btnActualizar.Click += (s, e) =>
                Ejecutar(ActualizarDashboard);

            // GRÁFICO
            var panelGraficoExterior = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 12)
            };

            contenido.Controls.Add(panelGraficoExterior, 0, 2);

            panelGraficoExterior.Controls.Add(new Label
            {
                Text = "EVOLUCIÓN MENSUAL DE PRÉSTAMOS",
                Font = new Font(
                    "Segoe UI", 11F, FontStyle.Bold),
                ForeColor = azul,
                AutoSize = true,
                Location = new Point(18, 12)
            });

            panelGrafico.Dock = DockStyle.Bottom;
            panelGrafico.Height = 160;
            panelGrafico.BackColor = Color.White;

            panelGrafico.Paint += DibujarGrafico;
            panelGraficoExterior.Controls.Add(panelGrafico);

            // SELECTOR Y BÚSQUEDA
            var panelReportes = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                BackColor = fondo
            };

            contenido.Controls.Add(panelReportes, 0, 3);

            CrearEtiqueta(panelReportes,
                "SELECCIONAR REPORTE", 0, 3);

            cboReporte.Location = new Point(0, 30);
            cboReporte.Size = new Size(340, 32);
            cboReporte.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cboReporte.Items.AddRange(new object[]
            {
                "Inventario general",
                "Libros disponibles",
                "Libros agotados",
                "Préstamos activos",
                "Préstamos vencidos",
                "Historial completo",
                "Libros más prestados",
                "Usuarios más activos",
                "Libros por editorial"
            });

            cboReporte.SelectedIndex = 0;
            panelReportes.Controls.Add(cboReporte);

            cboReporte.SelectedIndexChanged += (s, e) =>
            {
                if (inicializado)
                    Ejecutar(CargarReporte);
            };

            CrearEtiqueta(panelReportes,
                "BUSCAR EN EL REPORTE", 360, 3);

            txtBuscar.Location = new Point(360, 30);
            txtBuscar.Size = new Size(330, 32);
            txtBuscar.PlaceholderText =
                "Filtrar los resultados...";
            txtBuscar.BorderStyle = BorderStyle.FixedSingle;

            panelReportes.Controls.Add(txtBuscar);

            txtBuscar.TextChanged += (s, e) =>
                FiltrarTabla();

            var btnExcel = CrearBoton(
                "EXPORTAR EXCEL", verde, 160);

            btnExcel.Location = new Point(710, 27);
            panelReportes.Controls.Add(btnExcel);

            btnExcel.Click += (s, e) =>
                Ejecutar(ExportarCsv);

            var btnPdf = CrearBoton(
                "IMPRIMIR / PDF", azul, 155);

            btnPdf.Location = new Point(880, 27);
            panelReportes.Controls.Add(btnPdf);

            btnPdf.Click += (s, e) =>
                Ejecutar(ImprimirReporte);

            lblResumen.Location = new Point(0, 76);
            lblResumen.AutoSize = true;
            lblResumen.ForeColor = azul;
            lblResumen.Font = new Font(
                "Segoe UI", 9F, FontStyle.Bold);

            panelReportes.Controls.Add(lblResumen);

            // TABLA
            var panelTabla = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = Padding.Empty
            };

            contenido.Controls.Add(panelTabla, 0, 4);

            ConfigurarTabla();
            panelTabla.Controls.Add(tabla);

            // FECHA DE ACTUALIZACIÓN
            lblActualizacion.Dock = DockStyle.Fill;
            lblActualizacion.ForeColor =
                Color.FromArgb(95, 105, 120);
            lblActualizacion.TextAlign =
                ContentAlignment.MiddleLeft;

            contenido.Controls.Add(lblActualizacion, 0, 5);

            // ORDEN CORRECTO DE DOCK
            Controls.Add(encabezado);
            Controls.Add(contenedor);

            Controls.SetChildIndex(contenedor, 0);
            Controls.SetChildIndex(encabezado, 1);

            ResumeLayout(true);
        }

        private void AgregarIndicador(
            TableLayoutPanel panel,
            int columna,
            int fila,
            string titulo,
            string clave,
            Color color)
        {
            var tarjeta = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 10, 10),
                Padding = new Padding(12)
            };

            tarjeta.Controls.Add(new Label
            {
                Text = titulo,
                Dock = DockStyle.Top,
                Height = 23,
                Font = new Font(
                    "Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(90, 100, 115)
            });

            var valor = new Label
            {
                Text = "—",
                Dock = DockStyle.Bottom,
                Height = 38,
                Font = new Font(
                    "Segoe UI", 22F, FontStyle.Bold),
                ForeColor = color
            };

            tarjeta.Controls.Add(valor);
            indicadores[clave] = valor;

            panel.Controls.Add(tarjeta, columna, fila);
        }

        private static void CrearEtiqueta(
            Control padre, string texto, int x, int y)
        {
            padre.Controls.Add(new Label
            {
                Text = texto,
                Font = new Font(
                    "Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(38, 78, 133),
                Location = new Point(x, y),
                AutoSize = true
            });
        }

        private static Button CrearBoton(
            string texto, Color color, int ancho)
        {
            var boton = new Button
            {
                Text = texto,
                Size = new Size(ancho, 36),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(
                    "Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            boton.FlatAppearance.BorderSize = 0;
            return boton;
        }

        private void ConfigurarTabla()
        {
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
            tabla.RowTemplate.Height = 36;

            tabla.EnableHeadersVisualStyles = false;
            tabla.ColumnHeadersHeight = 48;
            tabla.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            tabla.ColumnHeadersDefaultCellStyle.BackColor = azul;
            tabla.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            tabla.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            tabla.DefaultCellStyle.Font =
                new Font("Segoe UI", 9F);

            tabla.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(210, 225, 245);

            tabla.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(25, 40, 65);

            tabla.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(244, 248, 253);

            tabla.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.Value == null)
                    return;

                if (tabla.Columns[e.ColumnIndex].Name == "Estado")
                {
                    var celda = tabla.Rows[e.RowIndex]
                        .Cells[e.ColumnIndex];

                    string estado = e.Value.ToString() ?? "";

                    celda.Style.ForeColor =
                        estado.Equals(
                            "Activo",
                            StringComparison.OrdinalIgnoreCase)
                        ? rojo
                        : verde;
                }
            };
        }

        private void ValidarFechas()
        {
            if (dtpDesde.Value.Date > dtpHasta.Value.Date)
                throw new Exception(
                    "La fecha inicial no puede superar la final.");
        }

        private void ActualizarDashboard()
        {
            ValidarFechas();

            CargarIndicadores();
            CargarGrafico();
            CargarReporte();

            lblActualizacion.Text =
                "Última actualización: " +
                DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") +
                "  |  Fuente: BibliotecaDB";
        }

        private SqlCommand CrearComando(
            SqlConnection conexion, string sql,
            bool incluirFechas = false)
        {
            var comando = new SqlCommand(sql, conexion)
            {
                CommandTimeout = 30
            };

            if (incluirFechas)
            {
                comando.Parameters.Add(
                    "@Desde", SqlDbType.Date).Value =
                    dtpDesde.Value.Date;

                // Límite superior exclusivo.
                comando.Parameters.Add(
                    "@Hasta", SqlDbType.DateTime2).Value =
                    dtpHasta.Value.Date.AddDays(1);
            }

            return comando;
        }

        private DataTable Consultar(
            string sql, bool incluirFechas = false)
        {
            using var conexion =
                ConnectionFactory.CreateConnection();

            using var comando =
                CrearComando(conexion, sql, incluirFechas);

            using var adaptador =
                new SqlDataAdapter(comando);

            var datos = new DataTable();
            adaptador.Fill(datos);
            return datos;
        }

        private void CargarIndicadores()
        {
            const string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM Libros) AS Libros,
                    (SELECT COALESCE(SUM(TotalEjemplares), 0)
                     FROM Libros) AS Ejemplares,
                    (SELECT COALESCE(SUM(EjemplaresDisponibles), 0)
                     FROM Libros) AS Disponibles,
                    (SELECT COUNT(*) FROM Usuarios) AS Usuarios,
                    (SELECT COUNT(*) FROM DetallePrestamo
                     WHERE Estado = 'Activo') AS Activos,
                    (SELECT COUNT(*)
                     FROM DetallePrestamo d
                     INNER JOIN Prestamos p
                         ON d.PrestamoID = p.PrestamoID
                     WHERE d.Estado = 'Activo'
                       AND p.FechaVencimiento <
                           CAST(GETDATE() AS DATE)) AS Vencidos,
                    (SELECT COUNT(*) FROM DetallePrestamo
                     WHERE Estado = 'Devuelto') AS Devoluciones,
                    (SELECT COUNT(*) FROM DetallePrestamo)
                        AS Historicos";

            var datos = Consultar(sql);

            if (datos.Rows.Count == 0)
                return;

            DataRow fila = datos.Rows[0];

            foreach (var indicador in indicadores)
            {
                indicador.Value.Text =
                    Convert.ToInt64(fila[indicador.Key])
                    .ToString("N0");
            }
        }

        private void CargarGrafico()
        {
            const string sql = @"
                SELECT
                    YEAR(FechaPrestamo) AS Anio,
                    MONTH(FechaPrestamo) AS Mes,
                    COUNT(*) AS Cantidad
                FROM Prestamos
                WHERE FechaPrestamo >= @Desde
                  AND FechaPrestamo < @Hasta
                GROUP BY YEAR(FechaPrestamo),
                         MONTH(FechaPrestamo)
                ORDER BY Anio, Mes";

            // Este gráfico muestra los últimos 12 meses
            // y no depende del filtro del reporte.
            DateTime inicio = new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month, 1).AddMonths(-11);

            DateTime fin = new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month, 1).AddMonths(1);

            using var conexion =
                ConnectionFactory.CreateConnection();

            using var comando = new SqlCommand(sql, conexion);
            comando.CommandTimeout = 30;

            comando.Parameters.Add(
                "@Desde", SqlDbType.DateTime2).Value = inicio;

            comando.Parameters.Add(
                "@Hasta", SqlDbType.DateTime2).Value = fin;

            var datos = new DataTable();

            using (var adaptador = new SqlDataAdapter(comando))
                adaptador.Fill(datos);

            var valores = new Dictionary<string, int>();

            foreach (DataRow fila in datos.Rows)
            {
                string clave =
                    $"{fila["Anio"]}-{Convert.ToInt32(fila["Mes"]):00}";

                valores[clave] =
                    Convert.ToInt32(fila["Cantidad"]);
            }

            meses.Clear();

            for (int i = 0; i < 12; i++)
            {
                DateTime mes = inicio.AddMonths(i);
                string clave = mes.ToString("yyyy-MM");

                meses.Add((
                    mes.ToString("MMM yy", new CultureInfo("es-CO")),
                    valores.TryGetValue(clave, out int cantidad)
                        ? cantidad
                        : 0
                ));
            }

            panelGrafico.Invalidate();
        }

        private void DibujarGrafico(
            object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);

            if (meses.Count == 0)
                return;

            int ancho = panelGrafico.ClientSize.Width;
            int alto = panelGrafico.ClientSize.Height;

            int izquierda = 45;
            int derecha = 20;
            int arriba = 15;
            int abajo = 35;

            int anchoDisponible =
                Math.Max(100, ancho - izquierda - derecha);

            int altoDisponible =
                Math.Max(40, alto - arriba - abajo);

            int maximo = Math.Max(
                1, meses.Max(m => m.Cantidad));

            using var fuente =
                new Font("Segoe UI", 8F);

            using var brochaAzul = new SolidBrush(azul);
            using var brochaTexto =
                new SolidBrush(Color.FromArgb(85, 95, 110));

            using var lapiz =
                new Pen(Color.FromArgb(225, 230, 238));

            // Líneas de referencia.
            for (int i = 0; i <= 4; i++)
            {
                int y = arriba +
                    (altoDisponible * i / 4);

                g.DrawLine(
                    lapiz, izquierda, y,
                    ancho - derecha, y);

                int valor = maximo * (4 - i) / 4;

                g.DrawString(
                    valor.ToString(),
                    fuente,
                    brochaTexto,
                    5, y - 8);
            }

            float anchoGrupo =
                anchoDisponible / (float)meses.Count;

            float anchoBarra =
                Math.Max(8, anchoGrupo * 0.52f);

            for (int i = 0; i < meses.Count; i++)
            {
                var mes = meses[i];

                float x = izquierda +
                    i * anchoGrupo +
                    (anchoGrupo - anchoBarra) / 2;

                float alturaBarra =
                    mes.Cantidad * altoDisponible /
                    (float)maximo;

                float y = arriba +
                    altoDisponible - alturaBarra;

                if (mes.Cantidad > 0)
                {
                    g.FillRectangle(
                        brochaAzul,
                        x, y,
                        anchoBarra, alturaBarra);
                }

                var nombre = g.MeasureString(
                    mes.Mes, fuente);

                g.DrawString(
                    mes.Mes,
                    fuente,
                    brochaTexto,
                    izquierda + i * anchoGrupo +
                    (anchoGrupo - nombre.Width) / 2,
                    alto - abajo + 8);
            }
        }

        private void CargarReporte()
        {
            ValidarFechas();

            tituloActual =
                cboReporte.SelectedItem?.ToString()
                ?? "Inventario general";

            string sql;
            bool fechas = false;

            switch (tituloActual)
            {
                case "Libros disponibles":
                    sql = @"
                        SELECT LibroID, Titulo, ISBN,
                               TotalEjemplares,
                               EjemplaresDisponibles
                        FROM Libros
                        WHERE EjemplaresDisponibles > 0
                        ORDER BY Titulo";
                    break;

                case "Libros agotados":
                    sql = @"
                        SELECT LibroID, Titulo, ISBN,
                               TotalEjemplares,
                               EjemplaresDisponibles
                        FROM Libros
                        WHERE EjemplaresDisponibles = 0
                        ORDER BY Titulo";
                    break;

                case "Préstamos activos":
                    sql = ConsultaPrestamos + @"
                        WHERE d.Estado = 'Activo'
                        ORDER BY p.FechaVencimiento";
                    break;

                case "Préstamos vencidos":
                    sql = ConsultaPrestamos + @"
                        WHERE d.Estado = 'Activo'
                          AND p.FechaVencimiento <
                              CAST(GETDATE() AS DATE)
                        ORDER BY p.FechaVencimiento";
                    break;

                case "Historial completo":
                    sql = ConsultaPrestamos + @"
                        WHERE p.FechaPrestamo >= @Desde
                          AND p.FechaPrestamo < @Hasta
                        ORDER BY p.FechaPrestamo DESC";
                    fechas = true;
                    break;

                case "Libros más prestados":
                    sql = @"
                        SELECT TOP (20)
                            l.LibroID,
                            l.Titulo AS Libro,
                            COUNT(*) AS TotalPrestamos
                        FROM DetallePrestamo d
                        INNER JOIN Libros l
                            ON d.LibroID = l.LibroID
                        INNER JOIN Prestamos p
                            ON d.PrestamoID = p.PrestamoID
                        WHERE p.FechaPrestamo >= @Desde
                          AND p.FechaPrestamo < @Hasta
                        GROUP BY l.LibroID, l.Titulo
                        ORDER BY TotalPrestamos DESC, l.Titulo";
                    fechas = true;
                    break;

                case "Usuarios más activos":
                    sql = @"
                        SELECT TOP (20)
                            u.UsuarioID,
                            u.Nombre + ' ' + u.Apellido AS Usuario,
                            COUNT(*) AS TotalPrestamos
                        FROM Prestamos p
                        INNER JOIN DetallePrestamo d
                            ON p.PrestamoID = d.PrestamoID
                        INNER JOIN Usuarios u
                            ON p.UsuarioID = u.UsuarioID
                        WHERE p.FechaPrestamo >= @Desde
                          AND p.FechaPrestamo < @Hasta
                        GROUP BY u.UsuarioID, u.Nombre, u.Apellido
                        ORDER BY TotalPrestamos DESC, Usuario";
                    fechas = true;
                    break;

                case "Libros por editorial":
                    sql = @"
                        SELECT
                            e.EditorialID,
                            e.Nombre AS Editorial,
                            e.Pais,
                            COUNT(l.LibroID) AS TotalLibros,
                            COALESCE(SUM(l.TotalEjemplares), 0)
                                AS TotalEjemplares
                        FROM Editoriales e
                        LEFT JOIN Libros l
                            ON e.EditorialID = l.EditorialID
                        GROUP BY e.EditorialID, e.Nombre, e.Pais
                        ORDER BY TotalLibros DESC, Editorial";
                    break;

                default:
                    tituloActual = "Inventario general";
                    sql = @"
                        SELECT
                            l.LibroID,
                            l.Titulo,
                            l.ISBN,
                            e.Nombre AS Editorial,
                            l.AnioPublicacion,
                            l.TotalEjemplares,
                            l.EjemplaresDisponibles
                        FROM Libros l
                        LEFT JOIN Editoriales e
                            ON l.EditorialID = e.EditorialID
                        ORDER BY l.Titulo";
                    break;
            }

            datosActuales = Consultar(sql, fechas);

            FiltrarTabla();
        }

        private void FiltrarTabla()
        {
            if (datosActuales == null)
                return;

            var vista = new DataView(datosActuales);
            string busqueda = txtBuscar.Text.Trim();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                // Escapar caracteres especiales de RowFilter.
                string valor = busqueda
                    .Replace("'", "''")
                    .Replace("[", "[[]")
                    .Replace("]", "[]]")
                    .Replace("%", "[%]")
                    .Replace("*", "[*]");

                var condiciones = new List<string>();

                foreach (DataColumn columna in datosActuales.Columns)
                {
                    string nombre =
                        columna.ColumnName.Replace("]", "]]");

                    condiciones.Add(
                        $"CONVERT([{nombre}], 'System.String') " +
                        $"LIKE '%{valor}%'");
                }

                if (condiciones.Count > 0)
                    vista.RowFilter =
                        "(" + string.Join(" OR ", condiciones) + ")";
            }

            tabla.DataSource = null;
            tabla.DataSource = vista;
            tabla.ClearSelection();

            foreach (DataGridViewColumn columna in tabla.Columns)
            {
                columna.HeaderText = columna.Name
                    .Replace("TotalEjemplares", "TOTAL")
                    .Replace("EjemplaresDisponibles", "DISPONIBLES")
                    .Replace("FechaPrestamo", "FECHA PRÉSTAMO")
                    .Replace("FechaVencimiento", "VENCIMIENTO")
                    .Replace("FechaDevolucion", "DEVOLUCIÓN")
                    .Replace("TotalPrestamos", "PRÉSTAMOS")
                    .Replace("TotalLibros", "LIBROS")
                    .Replace("AnioPublicacion", "AÑO");

                if (columna.ValueType == typeof(DateTime))
                    columna.DefaultCellStyle.Format = "dd/MM/yyyy";
            }

            lblResumen.Text =
                $"{tituloActual}  |  " +
                $"{vista.Count:N0} registros encontrados";
        }

        private void ExportarCsv()
        {
            if (tabla.Rows.Count == 0)
                throw new Exception(
                    "No hay datos para exportar.");

            using var dialogo = new SaveFileDialog
            {
                Filter = "Archivo CSV compatible con Excel (*.csv)|*.csv",
                FileName = "Reporte_Biblioteca_" +
                    DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv",
                Title = "Exportar reporte a Excel"
            };

            if (dialogo.ShowDialog() != DialogResult.OK)
                return;

            using var escritor = new StreamWriter(
                dialogo.FileName,
                false,
                new UTF8Encoding(true));

            var columnas = tabla.Columns
                .Cast<DataGridViewColumn>()
                .Where(c => c.Visible)
                .OrderBy(c => c.DisplayIndex)
                .ToList();

            // Separador compatible con Excel en español.
            escritor.WriteLine("sep=;");

            escritor.WriteLine(string.Join(";",
                columnas.Select(c => EscaparCsv(c.HeaderText))));

            foreach (DataGridViewRow fila in tabla.Rows)
            {
                if (fila.IsNewRow)
                    continue;

                escritor.WriteLine(string.Join(";",
                    columnas.Select(c =>
                        EscaparCsv(
                            FormatearValor(
                                fila.Cells[c.Index].Value)))));
            }

            MessageBox.Show(
                "Reporte exportado correctamente.\n\n" +
                dialogo.FileName,
                "Exportación exitosa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private static string FormatearValor(object? valor)
        {
            if (valor == null || valor == DBNull.Value)
                return "";

            if (valor is DateTime fecha)
                return fecha.ToString("dd/MM/yyyy HH:mm");

            return Convert.ToString(
                valor, CultureInfo.CurrentCulture) ?? "";
        }

        private static string EscaparCsv(string texto)
        {
            // Evitar que Excel interprete datos como fórmulas.
            if (!string.IsNullOrEmpty(texto) &&
                "=+-@\t\r".Contains(texto[0]))
            {
                texto = "'" + texto;
            }

            return "\"" +
                texto.Replace("\"", "\"\"") +
                "\"";
        }

        private void ImprimirReporte()
        {
            if (tabla.Rows.Count == 0)
                throw new Exception(
                    "No hay datos para imprimir.");

            using var documento = new PrintDocument();
            using var dialogo = new PrintDialog();

            documento.DefaultPageSettings.Landscape = true;
            documento.DocumentName = tituloActual;

            documentoImpresion = documento;
            filaImpresion = 0;

            documento.PrintPage += Documento_PrintPage;

            dialogo.Document = documento;
            dialogo.UseEXDialog = true;

            if (dialogo.ShowDialog() == DialogResult.OK)
            {
                // Se puede seleccionar "Microsoft Print to PDF".
                documento.Print();
            }

            documentoImpresion = null;
        }

        private void Documento_PrintPage(
            object sender, PrintPageEventArgs e)
        {
            if (e.Graphics == null)
                return;

            var g = e.Graphics;
            Rectangle area = e.MarginBounds;

            using var fuenteTitulo =
                new Font("Segoe UI", 15F, FontStyle.Bold);

            using var fuente =
                new Font("Segoe UI", 8F);

            using var fuenteCabecera =
                new Font("Segoe UI", 8F, FontStyle.Bold);

            using var brocha = new SolidBrush(Color.Black);
            using var linea = new Pen(Color.LightGray);

            float y = area.Top;

            g.DrawString(
                "BIBLIOTECA - " + tituloActual,
                fuenteTitulo, brocha, area.Left, y);

            y += 38;

            g.DrawString(
                "Generado: " +
                DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                fuente, brocha, area.Left, y);

            y += 30;

            var columnas = tabla.Columns
                .Cast<DataGridViewColumn>()
                .Where(c => c.Visible)
                .OrderBy(c => c.DisplayIndex)
                .ToList();

            if (columnas.Count == 0)
                return;

            float anchoColumna =
                area.Width / (float)columnas.Count;

            float altoFila = 28;

            // Encabezados.
            for (int i = 0; i < columnas.Count; i++)
            {
                var rect = new RectangleF(
                    area.Left + i * anchoColumna,
                    y, anchoColumna - 4, altoFila);

                g.DrawString(
                    columnas[i].HeaderText,
                    fuenteCabecera, brocha, rect);
            }

            y += 35;

            while (filaImpresion < tabla.Rows.Count)
            {
                if (y + altoFila > area.Bottom)
                {
                    e.HasMorePages = true;
                    return;
                }

                var fila = tabla.Rows[filaImpresion];

                for (int i = 0; i < columnas.Count; i++)
                {
                    string valor = FormatearValor(
                        fila.Cells[columnas[i].Index].Value);

                    var rect = new RectangleF(
                        area.Left + i * anchoColumna,
                        y, anchoColumna - 6, altoFila);

                    g.DrawString(
                        valor, fuente, brocha, rect);
                }

                g.DrawLine(
                    linea,
                    area.Left, y + altoFila,
                    area.Right, y + altoFila);

                y += altoFila;
                filaImpresion++;
            }

            e.HasMorePages = false;
            filaImpresion = 0;
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
                    "Error en el dashboard",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
