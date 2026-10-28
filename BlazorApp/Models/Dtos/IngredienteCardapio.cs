namespace BlazorApp.Models.Dtos;

public class IngredienteCardapioDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool InclusoPorPadrao { get; set; }
    public bool PermiteRemover { get; set; }
}