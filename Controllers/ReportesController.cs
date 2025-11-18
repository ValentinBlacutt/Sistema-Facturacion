using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistemaFacturacion.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

public class ReportesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(ApplicationDbContext context, ILogger<ReportesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> RankingVentas(DateTime? fechaInicio, DateTime? fechaFin)
    {
        try
        {
            // Ajuste de fechas para incluir todo el día
            var fechaInicioFiltro = (fechaInicio ?? DateTime.Now.AddDays(-30)).Date;
            var fechaFinFiltro = (fechaFin ?? DateTime.Now).Date.AddDays(1).AddTicks(-1);

            _logger.LogInformation($"Filtrando facturas entre {fechaInicioFiltro} y {fechaFinFiltro}");

            // Consulta optimizada que une directamente Facturas con DetalleFacturas
            var query = from f in _context.Facturas
                        join df in _context.DetalleFacturas.Include(d => d.Producto) on f.Id equals df.FacturaId
                        where f.Fecha >= fechaInicioFiltro && f.Fecha <= fechaFinFiltro
                        select df;

            var productosVendidos = await query
                .GroupBy(df => new { df.Producto.Nombre })
                .Select(g => new ProductoVendido
                {
                    Nombre = g.Key.Nombre,
                    Cantidad = g.Sum(df => df.Cantidad)
                })
                .ToListAsync();

            var totalFacturado = await _context.Facturas
                .Where(f => f.Fecha >= fechaInicioFiltro && f.Fecha <= fechaFinFiltro)
                .SumAsync(f => f.Total);

            var viewModel = new RankingViewModel
            {
                MasVendidos = productosVendidos
                    .OrderByDescending(p => p.Cantidad)
                    .Take(10)
                    .ToList(),

                MenosVendidos = productosVendidos
                    .OrderBy(p => p.Cantidad)
                    .Take(10)
                    .ToList(),

                FechaInicio = fechaInicioFiltro,
                FechaFin = fechaFinFiltro,

                TotalFacturado = totalFacturado,
                CantidadTotalProductos = productosVendidos.Sum(p => p.Cantidad)
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de ranking de ventas");
            TempData["Error"] = "Ocurrió un error al generar el reporte";
            return RedirectToAction("Index", "Home");
        }
    }
}