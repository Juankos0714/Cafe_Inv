using PaloCafeInventory.Models;
using System.Drawing;
using System.Drawing.Printing;

namespace PaloCafeInventory.Services
{
    public class PrintService
    {
        private ResumenTurno? _resumenTurno;
        private Font _fontTitulo = new Font("Arial", 12, FontStyle.Bold);
        private Font _fontSubtitulo = new Font("Arial", 10, FontStyle.Bold);
        private Font _fontNormal = new Font("Arial", 9);
        private Font _fontPequena = new Font("Arial", 8);

        public void ImprimirCierreTurno(ResumenTurno resumen)
        {
            _resumenTurno = resumen;

            try
            {
                var printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = "EPSON"; // Buscar impresora Epson
                
                // Si no encuentra Epson, usar impresora por defecto
                if (!printDocument.PrinterSettings.IsValid)
                {
                    printDocument.PrinterSettings.PrinterName = "";
                }

                printDocument.PrintPage += PrintDocument_PrintPage;
                printDocument.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error de Impresión", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (_resumenTurno == null || e.Graphics == null)
                return;

            float yPos = 10;
            float leftMargin = 10;
            float rightMargin = e.PageBounds.Width - 10;

            // Header de la empresa
            var headerText = new[]
            {
                "PALO DE CAFÉ",
                "Café de origen",
                "NIT: 41904612-7",
                "Régimen Simplificado",
                "CRA 13 # 1N35",
                "Clínica Central del Quindío",
                "Cel: 3244213193"
            };

            // Centrar header
            foreach (var line in headerText)
            {
                var font = line == headerText[0] ? _fontTitulo : _fontPequena;
                var size = e.Graphics.MeasureString(line, font);
                var xPos = (e.PageBounds.Width - size.Width) / 2;
                e.Graphics.DrawString(line, font, Brushes.Black, xPos, yPos);
                yPos += size.Height + 2;
            }

            yPos += 10;
            e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, rightMargin, yPos);
            yPos += 10;

            // Información del turno
            var infoLines = new[]
            {
                $"CIERRE DE TURNO #{_resumenTurno.NumeroTurno}",
                $"Fecha: {_resumenTurno.Fecha:dd/MM/yyyy}",
                $"Vendedor: {_resumenTurno.Usuario}",
                $"Inicio: {_resumenTurno.HoraInicio:HH:mm} - Cierre: {_resumenTurno.HoraCierre:HH:mm}"
            };

            foreach (var line in infoLines)
            {
                var font = line.StartsWith("CIERRE") ? _fontSubtitulo : _fontNormal;
                e.Graphics.DrawString(line, font, Brushes.Black, leftMargin, yPos);
                yPos += font.GetHeight(e.Graphics) + 3;
            }

            yPos += 5;
            e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, rightMargin, yPos);
            yPos += 10;

            // Detalle de ventas
            e.Graphics.DrawString("DETALLE DE VENTAS", _fontSubtitulo, Brushes.Black, leftMargin, yPos);
            yPos += _fontSubtitulo.GetHeight(e.Graphics) + 5;

            // Encabezados de tabla
            e.Graphics.DrawString("HORA", _fontPequena, Brushes.Black, leftMargin, yPos);
            e.Graphics.DrawString("PRODUCTO", _fontPequena, Brushes.Black, leftMargin + 50, yPos);
            e.Graphics.DrawString("CANT", _fontPequena, Brushes.Black, leftMargin + 200, yPos);
            e.Graphics.DrawString("PRECIO", _fontPequena, Brushes.Black, leftMargin + 240, yPos);
            e.Graphics.DrawString("TOTAL", _fontPequena, Brushes.Black, leftMargin + 290, yPos);
            
            yPos += _fontPequena.GetHeight(e.Graphics) + 3;
            e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, rightMargin, yPos);
            yPos += 5;

            // Ventas
            foreach (var venta in _resumenTurno.Ventas)
            {
                e.Graphics.DrawString(venta.Hora.ToString("HH:mm"), _fontPequena, Brushes.Black, leftMargin, yPos);
                e.Graphics.DrawString(venta.Producto, _fontPequena, Brushes.Black, leftMargin + 50, yPos);
                e.Graphics.DrawString(venta.Cantidad.ToString(), _fontPequena, Brushes.Black, leftMargin + 200, yPos);
                e.Graphics.DrawString($"${venta.PrecioUnitario:N0}", _fontPequena, Brushes.Black, leftMargin + 240, yPos);
                e.Graphics.DrawString($"${venta.Total:N0}", _fontPequena, Brushes.Black, leftMargin + 290, yPos);
                
                yPos += _fontPequena.GetHeight(e.Graphics) + 2;

                // Verificar si necesitamos nueva página
                if (yPos > e.PageBounds.Height - 100)
                {
                    e.HasMorePages = true;
                    return;
                }
            }

            yPos += 10;
            e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, rightMargin, yPos);
            yPos += 10;

            // Totales
            e.Graphics.DrawString($"TOTAL VENTAS: {_resumenTurno.TotalVentas}", _fontNormal, Brushes.Black, leftMargin, yPos);
            yPos += _fontNormal.GetHeight(e.Graphics) + 3;
            
            e.Graphics.DrawString($"TOTAL INGRESOS: ${_resumenTurno.TotalIngresos:N0}", _fontSubtitulo, Brushes.Black, leftMargin, yPos);
            yPos += _fontSubtitulo.GetHeight(e.Graphics) + 10;

            // Footer
            e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, rightMargin, yPos);
            yPos += 5;
            
            var footer = $"Impreso: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            var footerSize = e.Graphics.MeasureString(footer, _fontPequena);
            var footerX = (e.PageBounds.Width - footerSize.Width) / 2;
            e.Graphics.DrawString(footer, _fontPequena, Brushes.Black, footerX, yPos);
        }
    }
}