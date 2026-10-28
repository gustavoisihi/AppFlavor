namespace BlazorApp.Models.Entities;

public class PedidoItemAdicional
{
    public long Id { get; set; }
    public long PedidoItemId { get; set; }
    public int AdicionalId { get; set; }

    public string NomeAdicional { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Quantidade { get; set; } = 1;

    public PedidoItem PedidoItem { get; set; } = null!;
    public Adicional Adicional { get; set; } = null!;
}