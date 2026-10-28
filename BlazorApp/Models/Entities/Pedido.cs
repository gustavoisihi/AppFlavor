using BlazorApp.Models.Enums;

namespace BlazorApp.Models.Entities;

public class Pedido
{
    public long Id { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public StatusPreparo StatusPreparo { get; set; } = StatusPreparo.Novo;
    public StatusPagamento StatusPagamento { get; set; } = StatusPagamento.Pendente;

    public decimal ValorTotal { get; set; }
    public string? Observacao { get; set; }

    public ICollection<PedidoItem> Itens { get; set; } = new List<PedidoItem>();

    public Pagamento? Pagamento { get; set; }
}