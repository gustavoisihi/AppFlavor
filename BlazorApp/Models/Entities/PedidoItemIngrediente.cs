namespace BlazorApp.Models.Entities;

public class PedidoItemIngrediente
{
    public long Id { get; set; }
    public long PedidoItemId { get; set; }
    public int IngredienteId { get; set; }

    public string NomeIngrediente { get; set; } = string.Empty;

    public PedidoItem PedidoItem { get; set; } = null!;
    public Ingrediente Ingrediente { get; set; } = null!;
}