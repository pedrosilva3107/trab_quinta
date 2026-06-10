using FarmaciaSistema.Helpers;
using FarmaciaSistema.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSistema.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<LoteEstoque> LotesEstoque => Set<LoteEstoque>();
    public DbSet<Farmaceutico> Farmaceuticos => Set<Farmaceutico>();
    public DbSet<RetencaoSngpc> RetencoesSngpc => Set<RetencaoSngpc>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>()
            .HasIndex(p => p.CodigoBarras)
            .IsUnique();

        // Farmacêutico de demonstração: usuário "ana", senha "1234" (RF03)
        modelBuilder.Entity<Farmaceutico>().HasData(new Farmaceutico
        {
            Id = 1,
            Nome = "ana",
            SenhaHash = CryptoHelper.HashSenha("1234")
        });
    }
}
