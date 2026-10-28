namespace BlazorApp.Models.Entities;

public class PedidoItem
{
    public long Id { get; set; }
    public long PedidoId { get; set; }
    public int ProdutoId { get; set; }

    public string NomeProduto { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Quantidade { get; set; }
    public decimal TotalItem { get; set; }

    public string? Observacao { get; set; }

    public Pedido Pedido { get; set; } = null!;
    public Produto Produto { get; set; } = null!;

    public ICollection<PedidoItemIngrediente> IngredientesRemovidos { get; set; }
        = new List<PedidoItemIngrediente>();

    public ICollection<PedidoItemAdicional> Adicionais { get; set; }
        = new List<PedidoItemAdicional>();
}