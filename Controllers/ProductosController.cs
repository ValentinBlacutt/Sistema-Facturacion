using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistemaFacturacion.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace sistemaFacturacion.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Productos
                .Where(p => !p.EstaEliminado)
                .OrderBy(p => p.Nombre)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id && !m.EstaEliminado);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        public IActionResult Create()
        {
            // Crear nuevo producto con valores por defecto
            var producto = new Producto
            {
                AlicuotaIVA = 0.21m, // IVA 21% por defecto
                Stock = 0,
                AlertaStockMinimo = 5,
                TieneImpuestoInterno = false,
                PorcentajeImpuestoInterno = 0,
            };

            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            // Validación de código de barras
            if (!string.IsNullOrEmpty(producto.CodigoBarras))
            {
                var productoExistente = await _context.Productos
                    .FirstOrDefaultAsync(p => p.CodigoBarras == producto.CodigoBarras);

                if (productoExistente != null && !productoExistente.EstaEliminado)
                {
                    ModelState.AddModelError("CodigoBarras", "Este código de barras ya está registrado");
                }
                else if (productoExistente != null && productoExistente.EstaEliminado)
                {
                    // Reactivar producto existente
                    productoExistente.Nombre = producto.Nombre;
                    productoExistente.Precio = producto.Precio;
                    productoExistente.EstaEliminado = false;
                    productoExistente.AlicuotaIVA = producto.AlicuotaIVA;
                    productoExistente.TieneImpuestoInterno = producto.TieneImpuestoInterno;
                    productoExistente.PorcentajeImpuestoInterno = producto.PorcentajeImpuestoInterno;

                    _context.Update(productoExistente);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Producto reactivado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            try
            {
                producto.EstaEliminado = false;
                _context.Add(producto);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Producto creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                return View(producto);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null || producto.EstaEliminado)
            {
                return NotFound();
            }

            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Nombre,CodigoBarras,Precio,Stock,AlertaStockMinimo,AlicuotaIVA,TieneImpuestoInterno,PorcentajeImpuestoInterno,TienePercepcion,PorcentajePercepcion,EstaEliminado")]
            Producto producto)
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            // Validación de código de barras único
            if (!string.IsNullOrEmpty(producto.CodigoBarras))
            {
                var existeCodigoBarras = await _context.Productos.AnyAsync(p => p.Id != producto.Id && p.CodigoBarras == producto.CodigoBarras);
                if (existeCodigoBarras)
                {
                    ModelState.AddModelError("CodigoBarras", "Este código de barras ya está registrado");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Producto actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                }
            }
            return View(producto);
        }

        // Nuevo método para obtener el desglose de precios
        [HttpGet]
        public IActionResult ObtenerDesglosePrecio([FromQuery] decimal precio,
                                                  [FromQuery] decimal alicuotaIVA,
                                                  [FromQuery] bool tieneImpInt,
                                                  [FromQuery] decimal porcentajeImpInt)
        {
            var productoSimulado = new Producto
            {
                Precio = precio,
                AlicuotaIVA = alicuotaIVA,
                TieneImpuestoInterno = tieneImpInt,
                PorcentajeImpuestoInterno = porcentajeImpInt
            };

            var desglose = DesglosePrecio.Calcular(productoSimulado);

            return Json(new
            {
                neto = desglose.Neto,
                iva = desglose.IVA,
                impuestoInterno = desglose.ImpuestoInterno,
                total = desglose.PrecioFinal
            });
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id && !e.EstaEliminado);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id && !m.EstaEliminado);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                producto.EstaEliminado = true;
                _context.Update(producto);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Producto eliminado exitosamente";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> BuscarPorCodigo(string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                return Json(new { success = false, message = "Código no proporcionado" });
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.CodigoBarras == codigo && !p.EstaEliminado);

            if (producto == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Producto no encontrado. Puede crear uno nuevo con este código."
                });
            }

            return Json(new
            {
                success = true,
                producto = new
                {
                    id = producto.Id,
                    nombre = producto.Nombre,
                    precio = producto.Precio,
                    stock = producto.Stock
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> Buscar(string termino)
        {
            if (string.IsNullOrEmpty(termino))
            {
                return Json(new { success = false, message = "Término de búsqueda vacío" });
            }

            var productos = await _context.Productos
                .Where(p => !p.EstaEliminado &&
                           (p.Nombre.Contains(termino) || p.CodigoBarras.Contains(termino)))
                .OrderBy(p => p.Nombre)
                .Select(p => new {
                    id = p.Id,
                    nombre = p.Nombre,
                    precio = p.Precio,
                    stock = p.Stock,
                    codigoBarras = p.CodigoBarras
                })
                .ToListAsync();

            return Json(new { success = true, productos });
        }

    }
}   