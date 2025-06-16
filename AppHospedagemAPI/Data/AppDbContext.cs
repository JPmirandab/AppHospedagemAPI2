using Microsoft.EntityFrameworkCore;
using AppHospedagemAPI.Models;
using System.Collections.Generic;

namespace AppHospedagemAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Quarto> Quartos { get; set; }
        public DbSet<Locacao> Locacoes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }


    }
}
