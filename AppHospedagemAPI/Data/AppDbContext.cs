using AppHospedagemAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AppHospedagemAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Quarto> Quartos { get; set; }
        public DbSet<Locacao> Locacoes { get; set; }

         public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurações de relacionamento
            modelBuilder.Entity<Locacao>()
                .HasOne(l => l.Cliente)
                .WithMany()
                .HasForeignKey(l => l.ClienteId);

            modelBuilder.Entity<Locacao>()
                .HasOne(l => l.Quarto)
                .WithMany(q => q.Locacoes)
                .HasForeignKey(l => l.QuartoId);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Login)
                .IsUnique();
                
            modelBuilder.Entity<Cliente>()
        .HasIndex(c => c.Documento)
        .IsUnique(); // Garante que o documento é único no banco de dados

        }
    }
}