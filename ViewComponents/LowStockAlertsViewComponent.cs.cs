using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistemaFacturacion.Models;
using System.Linq;
using System.Threading.Tasks;

namespace sistemaFacturacion.ViewComponents
{
    // Este componente de vista se encargará de buscar productos con bajo stock.
    public class LowStockAlertsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public LowStockAlertsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        // El método InvokeAsync se ejecuta cuando el componente es llamado.
        // Aquí es donde se implementa la lógica para obtener los productos con bajo stock.
        // ...
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var lowStockProducts = await _context.Productos
                // ✨ Cambia la lógica de filtrado aquí
                .Where(p => p.Stock <= p.AlertaStockMinimo && !p.EstaEliminado)
                .ToListAsync();

            return View(lowStockProducts);
        }
       
    }
}
