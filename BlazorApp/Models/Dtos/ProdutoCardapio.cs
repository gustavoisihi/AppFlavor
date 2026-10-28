namespace BlazorApp.Models.Dtos;

public class ProdutoCardapioDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? ImagemUrl { get; set; }
    public decimal Preco { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;

    public List<IngredienteCardapioDto> Ingredientes { get; set; } = new();

    public List<AdicionalCardapioDto> Adicionais { get; set; } = new();
}