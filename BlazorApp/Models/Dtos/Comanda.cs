
namespace BlazorApp.Models.Dtos;

public class ComandaDto
{
    public long Id { get; set; }
    public DateTime CriadoEm { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string? Observacao { get; set; }

    public decimal ValorTotal { get; set; }

    public List<ItemComandaDto> Itens { get; set; } = new();
}

public class ItemComandaDto
{
    public string NomeProduto { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Quantidade { get; set; }
    public decimal TotalItem { get; set; }
    public string? Observacao { get; set; }

    public List<string> IngredientesRemovidos { get; set; } = new();

    public List<AdicionalComandaDto> Adicionais { get; set; } = new();
}

public class AdicionalComandaDto
{
    public string NomeAdicional { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Quantidade { get; set; }
}
