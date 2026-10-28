
using BlazorApp.Models.Entities;

namespace BlazorApp.Services;

public interface IAdicionalService
{
    Task<List<Adicional>> ListarTodosAsync();
    Task<Adicional?> ObterPorIdAsync(int id);
    Task CriarAsync(Adicional adicional);
    Task AtualizarAsync(Adicional adicional);
}
