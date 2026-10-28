
namespace BlazorApp.Models.Dtos;

public class ItemCarrinhoDto
{
    public Guid Chave { get; set; } = Guid.NewGuid();

    public int ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Quantidade { get; set; }
    public List<int> IngredientesRemovidosIds { get; set; } = new();
    public List<string> IngredientesRemovidos { get; set; } = new();
    public List<AdicionalCarrinhoDto> Adicionais { get; set; } = new();

    public decimal Total =>
        (PrecoUnitario +
         Adicionais.Sum(a => a.Preco * a.Quantidade)) * Quantidade;
}

public class AdicionalCarrinhoDto
{
    public int AdicionalId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int Quantidade { get; set; }
}
