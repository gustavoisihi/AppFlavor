
namespace BlazorApp.Models.Entities;

public class Ingrediente
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public bool Disponivel { get; set; } = true;
}
