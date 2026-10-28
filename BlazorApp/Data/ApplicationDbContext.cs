
using BlazorApp.Models.Entities;
using BlazorApp.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Ingrediente> Ingredientes => Set<Ingrediente>();
    public DbSet<ProdutoIngrediente> ProdutosIngredientes => Set<ProdutoIngrediente>();
    public DbSet<Adicional> Adicionais => Set<Adicional>();
    public DbSet<ProdutoAdicional> ProdutosAdicionais => Set<ProdutoAdicional>();

    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoItem> PedidosItens => Set<PedidoItem>();
    public DbSet<PedidoItemIngrediente> PedidosItensIngredientes => Set<PedidoItemIngrediente>();
    public DbSet<PedidoItemAdicional> PedidosItensAdicionais => Set<PedidoItemAdicional>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Categoria e produto
        builder.Entity<Categoria>()
            .HasMany(c => c.Produtos)
            .WithOne(p => p.Categoria)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Produto e ingredientes
        builder.Entity<ProdutoIngrediente>()
            .HasKey(pi => new { pi.ProdutoId, pi.IngredienteId });

        builder.Entity<ProdutoIngrediente>()
            .HasOne(pi => pi.Produto)
            .WithMany(p => p.Ingredientes)
            .HasForeignKey(pi => pi.ProdutoId);

        builder.Entity<ProdutoIngrediente>()
            .HasOne(pi => pi.Ingrediente)
            .WithMany()
            .HasForeignKey(pi => pi.IngredienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Produto e adicionais
        builder.Entity<ProdutoAdicional>()
            .HasKey(pa => new { pa.ProdutoId, pa.AdicionalId });

        builder.Entity<ProdutoAdicional>()
            .HasOne(pa => pa.Produto)
            .WithMany(p => p.Adicionais)
            .HasForeignKey(pa => pa.ProdutoId);

        builder.Entity<ProdutoAdicional>()
            .HasOne(pa => pa.Adicional)
            .WithMany(a => a.Produtos)
            .HasForeignKey(pa => pa.AdicionalId)
            .OnDelete(DeleteBehavior.Restrict);

        // Valores monetários
        builder.Entity<Produto>()
            .Property(p => p.Preco)
            .HasPrecision(10, 2);

        builder.Entity<Adicional>()
            .Property(a => a.Preco)
            .HasPrecision(10, 2);

        // Pedido e itens
        builder.Entity<Pedido>()
            .Property(p => p.StatusPreparo)
            .HasConversion<string>();

        builder.Entity<Pedido>()
            .Property(p => p.StatusPagamento)
            .HasConversion<string>();

        builder.Entity<Pedido>()
            .Property(p => p.ValorTotal)
            .HasPrecision(10, 2);

        builder.Entity<PedidoItem>()
            .Property(i => i.PrecoUnitario)
            .HasPrecision(10, 2);

        builder.Entity<PedidoItem>()
            .Property(i => i.TotalItem)
            .HasPrecision(10, 2);

        builder.Entity<PedidoItem>()
            .HasOne(i => i.Produto)
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ingredientes removidos de cada item
        builder.Entity<PedidoItemIngrediente>()
            .HasOne(i => i.Ingrediente)
            .WithMany()
            .HasForeignKey(i => i.IngredienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Adicionais de cada item
        builder.Entity<PedidoItemAdicional>()
            .Property(i => i.PrecoUnitario)
            .HasPrecision(10, 2);

        builder.Entity<PedidoItemAdicional>()
            .HasOne(i => i.Adicional)
            .WithMany()
            .HasForeignKey(i => i.AdicionalId)
            .OnDelete(DeleteBehavior.Restrict);

        // Pagamento: um registro por pedido nesta primeira versão
        builder.Entity<Pagamento>()
            .HasIndex(p => p.PedidoId)
            .IsUnique();

        builder.Entity<Pagamento>()
            .Property(p => p.Metodo)
            .HasConversion<string>();

        builder.Entity<Pagamento>()
            .Property(p => p.Valor)
            .HasPrecision(10, 2);

        builder.Entity<Pagamento>()
            .HasOne(p => p.Pedido)
            .WithOne(p => p.Pagamento)
            .HasForeignKey<Pagamento>(p => p.PedidoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Funcionário que registrou o pagamento
        builder.Entity<Pagamento>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(p => p.RegistradoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
