using ClosedXML.Excel;
using PaloCafeInventory.Models;

namespace PaloCafeInventory.Services
{
    public class ExcelService
    {
        public string GenerarReporteTurno(ResumenTurno resumen)
        {
            var nombreArchivo = $"Turno_{resumen.NumeroTurno}_{resumen.Fecha:yyyyMMdd}_{resumen.HoraCierre:HHmmss}.xlsx";
            var rutaArchivo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), nombreArchivo);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Resumen Turno");

            // Encabezado de la empresa
            worksheet.Cell(1, 1).Value = "PALO DE CAFÉ";
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Range(1, 1, 1, 5).Merge();

            worksheet.Cell(2, 1).Value = "Café de origen";
            worksheet.Cell(3, 1).Value = "NIT: 41904612-7";
            worksheet.Cell(4, 1).Value = "Régimen Simplificado";
            worksheet.Cell(5, 1).Value = "CRA 13 # 1N35 - Clínica Central del Quindío";
            worksheet.Cell(6, 1).Value = "Cel: 3244213193";

            // Información del turno
            worksheet.Cell(8, 1).Value = "RESUMEN DE TURNO";
            worksheet.Cell(8, 1).Style.Font.FontSize = 14;
            worksheet.Cell(8, 1).Style.Font.Bold = true;

            worksheet.Cell(10, 1).Value = "Turno #:";
            worksheet.Cell(10, 2).Value = resumen.NumeroTurno;
            worksheet.Cell(11, 1).Value = "Fecha:";
            worksheet.Cell(11, 2).Value = resumen.Fecha.ToString("dd/MM/yyyy");
            worksheet.Cell(12, 1).Value = "Vendedor:";
            worksheet.Cell(12, 2).Value = resumen.Usuario;
            worksheet.Cell(13, 1).Value = "Hora Inicio:";
            worksheet.Cell(13, 2).Value = resumen.HoraInicio.ToString("HH:mm:ss");
            worksheet.Cell(14, 1).Value = "Hora Cierre:";
            worksheet.Cell(14, 2).Value = resumen.HoraCierre.ToString("HH:mm:ss");

            // Encabezados de ventas
            var filaInicio = 16;
            worksheet.Cell(filaInicio, 1).Value = "DETALLE DE VENTAS";
            worksheet.Cell(filaInicio, 1).Style.Font.Bold = true;

            filaInicio++;
            worksheet.Cell(filaInicio, 1).Value = "Hora";
            worksheet.Cell(filaInicio, 2).Value = "Producto";
            worksheet.Cell(filaInicio, 3).Value = "Cantidad";
            worksheet.Cell(filaInicio, 4).Value = "Precio Unit.";
            worksheet.Cell(filaInicio, 5).Value = "Total";

            // Aplicar formato a encabezados
            var encabezados = worksheet.Range(filaInicio, 1, filaInicio, 5);
            encabezados.Style.Font.Bold = true;
            encabezados.Style.Fill.BackgroundColor = XLColor.LightGray;
            encabezados.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

            // Datos de ventas
            filaInicio++;
            foreach (var venta in resumen.Ventas)
            {
                worksheet.Cell(filaInicio, 1).Value = venta.Hora.ToString("HH:mm:ss");
                worksheet.Cell(filaInicio, 2).Value = venta.Producto;
                worksheet.Cell(filaInicio, 3).Value = venta.Cantidad;
                worksheet.Cell(filaInicio, 4).Value = venta.PrecioUnitario;
                worksheet.Cell(filaInicio, 4).Style.NumberFormat.Format = "$#,##0";
                worksheet.Cell(filaInicio, 5).Value = venta.Total;
                worksheet.Cell(filaInicio, 5).Style.NumberFormat.Format = "$#,##0";
                filaInicio++;
            }

            // Resumen totales
            filaInicio += 2;
            worksheet.Cell(filaInicio, 1).Value = "RESUMEN";
            worksheet.Cell(filaInicio, 1).Style.Font.Bold = true;

            filaInicio++;
            worksheet.Cell(filaInicio, 1).Value = "Total de Ventas:";
            worksheet.Cell(filaInicio, 2).Value = resumen.TotalVentas;

            filaInicio++;
            worksheet.Cell(filaInicio, 1).Value = "Total Ingresos:";
            worksheet.Cell(filaInicio, 2).Value = resumen.TotalIngresos;
            worksheet.Cell(filaInicio, 2).Style.NumberFormat.Format = "$#,##0";
            worksheet.Cell(filaInicio, 2).Style.Font.Bold = true;

            // Productos más vendidos
            if (resumen.ProductosMasVendidos.Any())
            {
                filaInicio += 3;
                worksheet.Cell(filaInicio, 1).Value = "PRODUCTOS MÁS VENDIDOS";
                worksheet.Cell(filaInicio, 1).Style.Font.Bold = true;

                filaInicio++;
                worksheet.Cell(filaInicio, 1).Value = "Producto";
                worksheet.Cell(filaInicio, 2).Value = "Cantidad";
                worksheet.Range(filaInicio, 1, filaInicio, 2).Style.Font.Bold = true;
                worksheet.Range(filaInicio, 1, filaInicio, 2).Style.Fill.BackgroundColor = XLColor.LightGray;

                filaInicio++;
                foreach (var producto in resumen.ProductosMasVendidos.OrderByDescending(p => p.Value).Take(5))
                {
                    worksheet.Cell(filaInicio, 1).Value = producto.Key;
                    worksheet.Cell(filaInicio, 2).Value = producto.Value;
                    filaInicio++;
                }
            }

            // Ajustar ancho de columnas
            worksheet.ColumnsUsed().AdjustToContents();

            try
            {
                workbook.SaveAs(rutaArchivo);
                return rutaArchivo;
            }
            catch
            {
                // Si no se puede guardar en el desktop, guardar en temp
                var rutaTemp = Path.Combine(Path.GetTempPath(), nombreArchivo);
                workbook.SaveAs(rutaTemp);
                return rutaTemp;
            }
        }

        public string GenerarReporteInventario(List<Producto> productos)
        {
            var nombreArchivo = $"Inventario_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var rutaArchivo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), nombreArchivo);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Inventario");

            // Encabezado
            worksheet.Cell(1, 1).Value = "REPORTE DE INVENTARIO - PALO DE CAFÉ";
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Range(1, 1, 1, 6).Merge();

            worksheet.Cell(2, 1).Value = $"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";

            // Encabezados de columnas
            var filaInicio = 4;
            worksheet.Cell(filaInicio, 1).Value = "Código";
            worksheet.Cell(filaInicio, 2).Value = "Producto";
            worksheet.Cell(filaInicio, 3).Value = "Categoría";
            worksheet.Cell(filaInicio, 4).Value = "Precio";
            worksheet.Cell(filaInicio, 5).Value = "Stock";
            worksheet.Cell(filaInicio, 6).Value = "Estado";

            var encabezados = worksheet.Range(filaInicio, 1, filaInicio, 6);
            encabezados.Style.Font.Bold = true;
            encabezados.Style.Fill.BackgroundColor = XLColor.LightGray;
            encabezados.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

            // Datos
            filaInicio++;
            foreach (var producto in productos.OrderBy(p => p.Nombre))
            {
                worksheet.Cell(filaInicio, 1).Value = producto.Codigo;
                worksheet.Cell(filaInicio, 2).Value = producto.Nombre;
                worksheet.Cell(filaInicio, 3).Value = producto.Categoria;
                worksheet.Cell(filaInicio, 4).Value = producto.Precio;
                worksheet.Cell(filaInicio, 4).Style.NumberFormat.Format = "$#,##0";
                worksheet.Cell(filaInicio, 5).Value = producto.StockActual;
                worksheet.Cell(filaInicio, 6).Value = producto.Activo ? "Activo" : "Inactivo";
                
                // Resaltar productos con bajo stock
                if (producto.StockActual <= 10)
                {
                    worksheet.Range(filaInicio, 1, filaInicio, 6).Style.Fill.BackgroundColor = XLColor.Orange;
                }

                filaInicio++;
            }

            worksheet.ColumnsUsed().AdjustToContents();

            try
            {
                workbook.SaveAs(rutaArchivo);
                return rutaArchivo;
            }
            catch
            {
                var rutaTemp = Path.Combine(Path.GetTempPath(), nombreArchivo);
                workbook.SaveAs(rutaTemp);
                return rutaTemp;
            }
        }
    }
}