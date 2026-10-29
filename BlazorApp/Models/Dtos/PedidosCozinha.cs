namespace BlazorApp.Models.Dtos;

public class PedidoCozinhaDto
{
    public long Id { get; set; }
    public DateTime CriadoEm { get; set; }
    public string StatusPreparo { get; set; } = string.Empty;
    public string? Observacao { get; set; }
    public List<ItemPedidoCozinhaDto> Itens { get; set; } = new();
    public string NomeCliente { get; set; } = string.Empty;
}

public class ItemPedidoCozinhaDto
{
    public string NomeProduto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public string? Observacao { get; set; }

    public List<string> IngredientesRemovidos { get; set; } = new();
    public List<string> Adicionais { get; set; } = new();
}