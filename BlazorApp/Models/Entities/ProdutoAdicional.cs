
namespace BlazorApp.Models.Entities;

public class ProdutoAdicional
{
    public int ProdutoId { get; set; }

    public int AdicionalId { get; set; }

    public Produto Produto { get; set; } = null!;

    public Adicional Adicional { get; set; } = null!;
}
