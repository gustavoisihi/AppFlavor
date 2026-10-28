
namespace BlazorApp.Models.Entities;

public class Adicional
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public bool Ativo { get; set; } = true;

    public bool Disponivel { get; set; } = true;

    public ICollection<ProdutoAdicional> Produtos { get; set; }
        = new List<ProdutoAdicional>();
}
