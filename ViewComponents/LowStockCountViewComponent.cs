using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistemaFacturacion.Models;
using System.Linq;
using System.Threading.Tasks;

namespace sistemaFacturacion.ViewComponents
{
    // Este componente de vista va a de obtener conteo de productos con bajo stock.
    public class LowStockCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public LowStockCountViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        // El InvokeAsync se ejecuta cuando el componente es llamado.
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var lowStockCount = await _context.Productos
                // ✨ Y también aquí
                .Where(p => p.Stock <= p.AlertaStockMinimo && !p.EstaEliminado)
                .CountAsync();

            return View(lowStockCount);
        }
    }
}
