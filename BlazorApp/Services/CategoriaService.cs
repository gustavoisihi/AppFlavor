using BlazorApp.Data;
using BlazorApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class CategoriaService : ICategoriaService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public CategoriaService(
        IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Categoria>> ListarTodasAsync()
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        return await context.Categorias
            .AsNoTracking()
            .OrderBy(c => c.OrdemExibicao)
            .ThenBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<Categoria?> ObterPorIdAsync(int id)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        return await context.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task CriarAsync(Categoria categoria)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        context.Categorias.Add(categoria);

        await context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Categoria categoria)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var categoriaExistente = await context.Categorias
            .FirstOrDefaultAsync(c => c.Id == categoria.Id);

        if (categoriaExistente is null)
        {
            throw new InvalidOperationException(
                "Categoria não encontrada.");
        }

        categoriaExistente.Nome = categoria.Nome;
        categoriaExistente.OrdemExibicao = categoria.OrdemExibicao;
        categoriaExistente.Ativo = categoria.Ativo;

        await context.SaveChangesAsync();
    }
}