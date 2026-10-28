
using BlazorApp.Data;
using BlazorApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class IngredientesService : IIngredientesService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public IngredientesService(
        IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Ingrediente>> ListarTodosAsync()
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        return await context.Ingredientes
            .AsNoTracking()
            .OrderBy(i => i.Nome)
            .ToListAsync();
    }

    public async Task<Ingrediente?> ObterPorIdAsync(int id)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        return await context.Ingredientes
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task CriarAsync(Ingrediente ingrediente)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        context.Ingredientes.Add(ingrediente);

        await context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Ingrediente ingrediente)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var existente = await context.Ingredientes
            .FirstOrDefaultAsync(i => i.Id == ingrediente.Id);

        if (existente is null)
        {
            throw new InvalidOperationException(
                "Ingrediente não encontrado.");
        }

        existente.Nome = ingrediente.Nome;
        existente.Ativo = ingrediente.Ativo;
        existente.Disponivel = ingrediente.Disponivel;

        await context.SaveChangesAsync();
    }
}
