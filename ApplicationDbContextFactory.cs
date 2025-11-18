using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;

// El namespace es el nombre de tu proyecto ya que está en la raíz
namespace sistemaFacturacion
{
    // Esta clase implementa IDesignTimeDbContextFactory para que las herramientas de migración
    // puedan crear una instancia de ApplicationDbContext.
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<Models.ApplicationDbContext>
    {
        public Models.ApplicationDbContext CreateDbContext(string[] args)
        {
            // La configuración se utiliza para leer el ConnectionString del archivo appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<Models.ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Aquí se especifica el proveedor de base de datos (SqlServer) y el connection string.
            // Asegúrate de que esta configuración coincida con la que tienes en Program.cs.
            optionsBuilder.UseSqlServer(connectionString);

            return new Models.ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
