namespace BlazorApp.Models.Requests;

public class CriarPedidoRequest
{
    public string NomeCliente { get; set; } = string.Empty;

    public string? Observacao { get; set; }

    public List<CriarPedidoItemRequest> Itens { get; set; } = new();
}