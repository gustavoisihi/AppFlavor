
namespace BlazorApp.Models.Entities;

public class ImpressaoComanda
{
    public long Id { get; set; }

    public long PedidoId { get; set; }

    public string Status { get; set; } = "Pendente";

    public int Tentativas { get; set; } = 0;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public DateTime? UltimaTentativaEm { get; set; }

    public DateTime? ImpressoEm { get; set; }

    public string? MensagemErro { get; set; }

    public Pedido Pedido { get; set; } = null!;
}
