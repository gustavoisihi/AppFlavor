
using BlazorApp.Data;
using BlazorApp.Models.Dtos;
using BlazorApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class ProdutoService : IProdutoService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ProdutoService(
        IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Produto>> ListarTodosAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .OrderBy(p => p.OrdemExibicao)
            .ThenBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<List<Produto>> ListarDisponiveisAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => p.Ativo && p.Disponivel && p.Categoria.Ativo)
            .OrderBy(p => p.OrdemExibicao)
            .ThenBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<Produto?> ObterPorIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Ingredientes)
                .ThenInclude(pi => pi.Ingrediente)
            .Include(p => p.Adicionais)
                .ThenInclude(pa => pa.Adicional)
            .FirstOrDefaultAsync(p => p.Id == id);
    }


    public async Task CriarAsync(Produto produto)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        context.Produtos.Add(produto);

        await context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Produto produto)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var produtoExistente = await context.Produtos
            .FirstOrDefaultAsync(p => p.Id == produto.Id);

        if (produtoExistente is null)
        {
            throw new InvalidOperationException("Produto não encontrado.");
        }

        produtoExistente.Nome = produto.Nome;
        produtoExistente.Descricao = produto.Descricao;
        produtoExistente.ImagemUrl = produto.ImagemUrl;
        produtoExistente.CategoriaId = produto.CategoriaId;
        produtoExistente.Preco = produto.Preco;
        produtoExistente.Ativo = produto.Ativo;
        produtoExistente.Disponivel = produto.Disponivel;
        produtoExistente.OrdemExibicao = produto.OrdemExibicao;

        await context.SaveChangesAsync();
    }




    public async Task<List<IngredienteProdutoDto>>
        ListarIngredientesDoProdutoAsync(int produtoId)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        return await context.ProdutosIngredientes
            .AsNoTracking()
            .Where(pi =>
                pi.ProdutoId == produtoId &&
                pi.Ingrediente.Ativo)
            .OrderBy(pi => pi.Ingrediente.Nome)
            .Select(pi => new IngredienteProdutoDto
            {
                IngredienteId = pi.IngredienteId,
                Nome = pi.Ingrediente.Nome,
                InclusoPorPadrao = pi.InclusoPorPadrao,
                PermiteRemover = pi.PermiteRemover
            })
            .ToListAsync();
    }



    public async Task SalvarIngredientesDoProdutoAsync(
        int produtoId,
        List<IngredienteProdutoDto> ingredientes)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var produtoExiste = await context.Produtos
            .AnyAsync(p => p.Id == produtoId);

        if (!produtoExiste)
        {
            throw new InvalidOperationException("Produto não encontrado.");
        }

        var ingredientesIds = ingredientes
            .Select(i => i.IngredienteId)
            .Distinct()
            .ToList();

        var ingredientesValidos = await context.Ingredientes
            .Where(i => ingredientesIds.Contains(i.Id) && i.Ativo)
            .Select(i => i.Id)
            .ToListAsync();

        if (ingredientesValidos.Count != ingredientesIds.Count)
        {
            throw new InvalidOperationException(
                "Um ou mais ingredientes são inválidos ou estão inativos.");
        }

        var atuais = await context.ProdutosIngredientes
            .Where(pi => pi.ProdutoId == produtoId)
            .ToListAsync();

        context.ProdutosIngredientes.RemoveRange(atuais);

        foreach (var ingrediente in ingredientes)
        {
            context.ProdutosIngredientes.Add(new ProdutoIngrediente
            {
                ProdutoId = produtoId,
                IngredienteId = ingrediente.IngredienteId,
                InclusoPorPadrao = ingrediente.InclusoPorPadrao,
                PermiteRemover = ingrediente.PermiteRemover
            });
        }

        await context.SaveChangesAsync();
    }


    public async Task<List<AdicionalProdutoDto>>
        ListarAdicionaisDoProdutoAsync(int produtoId)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var associados = await context.ProdutosAdicionais
            .AsNoTracking()
            .Where(pa => pa.ProdutoId == produtoId)
            .Select(pa => pa.AdicionalId)
            .ToListAsync();

        return await context.Adicionais
            .AsNoTracking()
            .Where(a => a.Ativo)
            .OrderBy(a => a.Nome)
            .Select(a => new AdicionalProdutoDto
            {
                AdicionalId = a.Id,
                Nome = a.Nome,
                Preco = a.Preco,
                Selecionado = associados.Contains(a.Id)
            })
            .ToListAsync();
    }

    public async Task SalvarAdicionaisDoProdutoAsync(
        int produtoId,
        List<AdicionalProdutoDto> adicionais)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var produtoExiste = await context.Produtos
            .AnyAsync(p => p.Id == produtoId);

        if (!produtoExiste)
        {
            throw new InvalidOperationException(
                "Produto não encontrado.");
        }

        var selecionados = adicionais
            .Where(a => a.Selecionado)
            .Select(a => a.AdicionalId)
            .Distinct()
            .ToList();

        var validos = await context.Adicionais
            .Where(a => selecionados.Contains(a.Id) && a.Ativo)
            .Select(a => a.Id)
            .ToListAsync();

        if (validos.Count != selecionados.Count)
        {
            throw new InvalidOperationException(
                "Um ou mais adicionais são inválidos ou estão inativos.");
        }

        var atuais = await context.ProdutosAdicionais
            .Where(pa => pa.ProdutoId == produtoId)
            .ToListAsync();

        context.ProdutosAdicionais.RemoveRange(atuais);

        foreach (var adicionalId in selecionados)
        {
            context.ProdutosAdicionais.Add(new ProdutoAdicional
            {
                ProdutoId = produtoId,
                AdicionalId = adicionalId
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task<List<ProdutoCardapioDto>> ListarCardapioAsync()
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var produtos = await context.Produtos
            .AsNoTracking()
            .Where(p =>
                p.Ativo &&
                p.Disponivel &&
                p.Categoria.Ativo)
            .OrderBy(p => p.Categoria.OrdemExibicao)
            .ThenBy(p => p.OrdemExibicao)
            .ThenBy(p => p.Nome)
            .Select(p => new ProdutoCardapioDto
            {
                Id = p.Id,
                Nome = p.Nome,
                Descricao = p.Descricao,
                ImagemUrl = p.ImagemUrl,
                Preco = p.Preco,
                NomeCategoria = p.Categoria.Nome,

                Ingredientes = p.Ingredientes
                    .Where(pi =>
                        pi.Ingrediente.Ativo &&
                        pi.Ingrediente.Disponivel)
                    .OrderBy(pi => pi.Ingrediente.Nome)
                    .Select(pi => new IngredienteCardapioDto
                    {
                        Id = pi.IngredienteId,
                        Nome = pi.Ingrediente.Nome,
                        InclusoPorPadrao = pi.InclusoPorPadrao,
                        PermiteRemover = pi.PermiteRemover
                    })
                    .ToList(),

                Adicionais = p.Adicionais
                    .Where(pa =>
                        pa.Adicional.Ativo &&
                        pa.Adicional.Disponivel)
                    .OrderBy(pa => pa.Adicional.Nome)
                    .Select(pa => new AdicionalCardapioDto
                    {
                        Id = pa.AdicionalId,
                        Nome = pa.Adicional.Nome,
                        Preco = pa.Adicional.Preco
                    })
                    .ToList()
            })
            .ToListAsync();

        return produtos;
    }



    public async Task<List<IngredienteProdutoDto>>
        ListarIngredientesDisponiveisAsync(int produtoId)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var associadosIds = await context.ProdutosIngredientes
            .AsNoTracking()
            .Where(pi => pi.ProdutoId == produtoId)
            .Select(pi => pi.IngredienteId)
            .ToListAsync();

        return await context.Ingredientes
            .AsNoTracking()
            .Where(i => i.Ativo && !associadosIds.Contains(i.Id))
            .OrderBy(i => i.Nome)
            .Select(i => new IngredienteProdutoDto
            {
                IngredienteId = i.Id,
                Nome = i.Nome,
                InclusoPorPadrao = true,
                PermiteRemover = true
            })
            .ToListAsync();
    }


}
