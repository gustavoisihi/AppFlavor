
using BlazorApp.Data;
using BlazorApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class AdicionalService : IAdicionalService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AdicionalService(
        IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Adicional>> ListarTodosAsync()
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        return await context.Adicionais
            .AsNoTracking()
            .OrderBy(a => a.Nome)
            .ToListAsync();
    }

    public async Task<Adicional?> ObterPorIdAsync(int id)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        return await context.Adicionais
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task CriarAsync(Adicional adicional)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        context.Adicionais.Add(adicional);

        await context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Adicional adicional)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var existente = await context.Adicionais
            .FirstOrDefaultAsync(a => a.Id == adicional.Id);

        if (existente is null)
        {
            throw new InvalidOperationException(
                "Adicional não encontrado.");
        }

        existente.Nome = adicional.Nome;
        existente.Preco = adicional.Preco;
        existente.Ativo = adicional.Ativo;
        existente.Disponivel = adicional.Disponivel;

        await context.SaveChangesAsync();
    }
}
