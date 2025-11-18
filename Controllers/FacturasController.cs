using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistemaFacturacion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sistemaFacturacion.Controllers
{
    public class FacturasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FacturasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin, decimal? totalMin, decimal? totalMax)
        {
            var facturas = _context.Facturas
                .Include(f => f.Detalles)
                .ThenInclude(df => df.Producto)
                .AsQueryable();

            if (fechaInicio.HasValue)
            {
                facturas = facturas.Where(f => f.Fecha >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                facturas = facturas.Where(f => f.Fecha <= fechaFin.Value.AddDays(1).AddMilliseconds(-1));
            }

            if (totalMin.HasValue)
            {
                facturas = facturas.Where(f => f.Total >= totalMin.Value);
            }

            if (totalMax.HasValue)
            {
                facturas = facturas.Where(f => f.Total <= totalMax.Value);
            }

            ViewData["CurrentFechaInicio"] = fechaInicio?.ToString("yyyy-MM-dd");
            ViewData["CurrentFechaFin"] = fechaFin?.ToString("yyyy-MM-dd");
            ViewData["TotalMin"] = totalMin?.ToString("0.00");
            ViewData["TotalMax"] = totalMax?.ToString("0.00");

            facturas = facturas.OrderByDescending(f => f.Fecha);

            return View(await facturas.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Factura factura, List<int> productoIds, List<int> cantidades)
        {
            if (productoIds == null || cantidades == null || productoIds.Count == 0 || productoIds.Count != cantidades.Count)
            {
                ModelState.AddModelError("", "Debe seleccionar al menos un producto y especificar su cantidad.");
                return View(factura);
            }

            factura.Fecha = DateTime.Now;
            factura.Total = 0;
            factura.Detalles = new List<DetalleFactura>();

            for (int i = 0; i < productoIds.Count; i++)
            {
                var productId = productoIds[i];
                var cantidad = cantidades[i];

                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == productId && !p.EstaEliminado);

                if (producto == null || cantidad <= 0 || producto.Stock < cantidad)
                {
                    ModelState.AddModelError("", $"Producto '{producto?.Nombre ?? "Desconocido"}' no válido, no disponible o stock insuficiente.");
                    return View(factura);
                }

                var detalle = new DetalleFactura
                {
                    ProductoId = productId,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.Precio,
                    Producto = producto // Asignamos el producto para poder calcular el desglose
                };

                // Actualizamos el desglose de impuestos
                detalle.ActualizarDesgloseImpuestos();

                factura.Detalles.Add(detalle);
                factura.Total += detalle.PrecioUnitario * detalle.Cantidad;

                producto.Stock -= cantidad;
                _context.Update(producto);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(factura);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "No se pudo crear la factura. Inténtelo de nuevo.");
                    Console.WriteLine($"Error al crear factura: {ex.Message}");
                    return View(factura);
                }
            }

            return View(factura);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var factura = await _context.Facturas
                .Include(f => f.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (factura == null) return NotFound();

            // Aseguramos que cada detalle tenga su producto cargado
            foreach (var detalle in factura.Detalles)
            {
                if (detalle.Producto == null)
                {
                    detalle.Producto = await _context.Productos.FindAsync(detalle.ProductoId);
                }
                detalle.ActualizarDesgloseImpuestos();
            }

            return View(factura);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDesgloseProducto(int productoId)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
            {
                return Json(new { success = false });
            }

            var detalleSimulado = new DetalleFactura
            {
                Producto = producto,
                PrecioUnitario = producto.Precio,
                Cantidad = 1
            };
            detalleSimulado.ActualizarDesgloseImpuestos();

            return Json(new
            {
                success = true,
                precio = detalleSimulado.PrecioUnitario,
                neto = detalleSimulado.Neto,
                iva = detalleSimulado.MontoIVA,
                impuestoInterno = detalleSimulado.MontoImpuestoInterno
            });
        }
    }
}