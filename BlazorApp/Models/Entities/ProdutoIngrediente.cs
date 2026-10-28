
namespace BlazorApp.Models.Entities;

public class ProdutoIngrediente
{
    public int ProdutoId { get; set; }

    public int IngredienteId { get; set; }

    public bool InclusoPorPadrao { get; set; } = true;

    public bool PermiteRemover { get; set; } = true;

    public Produto Produto { get; set; } = null!;

    public Ingrediente Ingrediente { get; set; } = null!;
}
