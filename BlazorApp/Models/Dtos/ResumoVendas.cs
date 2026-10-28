
namespace BlazorApp.Models.Dtos;

public class ResumoVendasDto
{
    public int QuantidadePedidos { get; set; }
    public int QuantidadeItens { get; set; }
    public decimal ValorTotalPedidos { get; set; }

    public List<ProdutoVendidoDto> Produtos { get; set; } = new();
}

public class ProdutoVendidoDto
{
    public string NomeProduto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorTotal { get; set; }
}
