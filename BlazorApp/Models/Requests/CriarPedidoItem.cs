namespace BlazorApp.Models.Requests;

public class CriarPedidoItemRequest
{
    public int ProdutoId { get; set; }

    public int Quantidade { get; set; }

    public List<int> IngredientesRemovidosIds { get; set; } = new();

    public List<AdicionalPedidoRequest> Adicionais { get; set; } = new();
}