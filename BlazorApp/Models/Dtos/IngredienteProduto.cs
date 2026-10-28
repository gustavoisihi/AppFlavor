
namespace BlazorApp.Models.Dtos;

public class IngredienteProdutoDto
{
    public int IngredienteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool InclusoPorPadrao { get; set; } = true;
    public bool PermiteRemover { get; set; } = true;
}
