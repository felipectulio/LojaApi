using Microsoft.EntityFrameworkCore;
using LojaApi.Models;

namespace LojaApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Produto> Produtos { get; set; }
        // Novas tabelas da avaliação:
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Vendedor> Vendedores { get; set; }
    }
}