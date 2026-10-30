
namespace BlazorApp.Models.Dtos;

public class ImpressaoComandaDto
{
    public long PedidoId { get; set; }

    public string NomeCliente { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int Tentativas { get; set; }

    public DateTime CriadoEm { get; set; }

    public DateTime? UltimaTentativaEm { get; set; }

    public DateTime? ImpressoEm { get; set; }

    public string? MensagemErro { get; set; }
}
