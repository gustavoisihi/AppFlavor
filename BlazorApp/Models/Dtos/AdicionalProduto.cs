
namespace BlazorApp.Models.Dtos;

public class AdicionalProdutoDto
{
    public int AdicionalId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public bool Selecionado { get; set; }
}
