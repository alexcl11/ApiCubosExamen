using ApiCubosExamen.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCubosExamen.Data
{
    public class ContextCubos:DbContext
    {
        public ContextCubos(DbContextOptions<ContextCubos> options) : base(options) { }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cubo> Cubos { get; set; }
        public DbSet<CompraCubo> Compras { get; set; }
    }

}
