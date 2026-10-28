using BlazorApp.Models.Entities;

namespace BlazorApp.Services;

public interface ICategoriaService
{
    Task<List<Categoria>> ListarTodasAsync();
    Task<Categoria?> ObterPorIdAsync(int id);
    Task CriarAsync(Categoria categoria);
    Task AtualizarAsync(Categoria categoria);
}