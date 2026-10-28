using BlazorApp.Models.Enums;

namespace BlazorApp.Models.Entities;

public class Pagamento
{
    public long Id { get; set; }
    public long PedidoId { get; set; }

    public MetodoPagamento Metodo { get; set; }
    public decimal Valor { get; set; }
    public DateTime PagoEm { get; set; } = DateTime.UtcNow;

    public string RegistradoPorUsuarioId { get; set; } = string.Empty;

    public Pedido Pedido { get; set; } = null!;
}