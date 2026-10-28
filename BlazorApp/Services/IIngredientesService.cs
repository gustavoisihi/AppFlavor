
using BlazorApp.Models.Entities;

namespace BlazorApp.Services;

public interface IIngredientesService
{
    Task<List<Ingrediente>> ListarTodosAsync();
    Task<Ingrediente?> ObterPorIdAsync(int id);
    Task CriarAsync(Ingrediente ingrediente);
    Task AtualizarAsync(Ingrediente ingrediente);
}
