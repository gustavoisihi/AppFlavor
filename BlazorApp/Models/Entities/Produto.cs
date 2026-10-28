
namespace BlazorApp.Models.Entities;

public class Produto
{
    public int Id { get; set; }

    public int CategoriaId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string? ImagemUrl { get; set; }

    public decimal Preco { get; set; }

    public bool Ativo { get; set; } = true;

    public bool Disponivel { get; set; } = true;

    public int OrdemExibicao { get; set; }

    public Categoria Categoria { get; set; } = null!;

    public ICollection<ProdutoIngrediente> Ingredientes { get; set; }
        = new List<ProdutoIngrediente>();

    public ICollection<ProdutoAdicional> Adicionais { get; set; }
        = new List<ProdutoAdicional>();
}
