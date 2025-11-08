using Korp.Faturamento.Models;
using Microsoft.EntityFrameworkCore;

namespace Korp.Faturamento.Data
{
    public class FaturamentoDbContext : DbContext
    {
        public FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> options) : base(options)
        {
        }

        public DbSet<NotaFiscal> NotasFiscais { get; set; }
        public DbSet<NotaFiscalItem> NotaFiscalItens { get; set; }
        public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // nota única 
            modelBuilder.Entity<NotaFiscal>()
                .HasIndex(n => n.NumeroSequencial)
                .IsUnique();

            modelBuilder.Entity<NotaFiscal>()
                .HasMany(n => n.Itens)
                .WithOne(i => i.NotaFiscal)
                .HasForeignKey(i => i.NotaFiscalId);
        }
    }
}
