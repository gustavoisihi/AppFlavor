
namespace BlazorApp.Models.Entities;

public class Categoria
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public int OrdemExibicao { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<Produto> Produtos { get; set; }
        = new List<Produto>();
}
