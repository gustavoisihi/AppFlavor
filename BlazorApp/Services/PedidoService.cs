using BlazorApp.Data;
using BlazorApp.Models.Dtos;
using BlazorApp.Models.Entities;
using BlazorApp.Models.Enums;
using BlazorApp.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class PedidoService : IPedidoService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public PedidoService(
        IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<long> CriarPedidoAsync(CriarPedidoRequest request)
    {
        if (request.Itens.Count == 0)
            throw new InvalidOperationException(
                "O pedido precisa ter pelo menos um item.");

        if (request.Itens.Any(i => i.Quantidade < 1 || i.Quantidade > 99))
            throw new InvalidOperationException(
                "A quantidade dos produtos deve estar entre 1 e 99.");

        if (string.IsNullOrWhiteSpace(request.NomeCliente))
            throw new InvalidOperationException(
                "Informe o nome do cliente.");

        if (request.NomeCliente.Trim().Length > 100)
            throw new InvalidOperationException(
                "O nome do cliente deve ter no máximo 100 caracteres.");

        await using var context =
            await _contextFactory.CreateDbContextAsync();

        await using var transaction =
            await context.Database.BeginTransactionAsync();

        var pedido = new Pedido
        {
            NomeCliente = request.NomeCliente.Trim(),
            CriadoEm = DateTime.UtcNow,
            StatusPreparo = StatusPreparo.Novo,
            StatusPagamento = StatusPagamento.Pendente,
            Observacao = request.Observacao?.Trim()
        };

        decimal valorTotal = 0;

        foreach (var itemRequest in request.Itens)
        {
            var produto = await context.Produtos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Ingredientes)
                    .ThenInclude(pi => pi.Ingrediente)
                .Include(p => p.Adicionais)
                    .ThenInclude(pa => pa.Adicional)
                .FirstOrDefaultAsync(p =>
                    p.Id == itemRequest.ProdutoId);

            if (produto is null ||
                !produto.Ativo ||
                !produto.Disponivel ||
                !produto.Categoria.Ativo)
            {
                throw new InvalidOperationException(
                    "Um dos produtos não está disponível.");
            }

            var ingredientesRemovidos =
                itemRequest.IngredientesRemovidosIds.Distinct().ToList();

            foreach (var ingredienteId in ingredientesRemovidos)
            {
                var ingredienteProduto = produto.Ingredientes
                    .FirstOrDefault(pi =>
                        pi.IngredienteId == ingredienteId);

                if (ingredienteProduto is null ||
                    !ingredienteProduto.Ingrediente.Ativo ||
                    !ingredienteProduto.InclusoPorPadrao ||
                    !ingredienteProduto.PermiteRemover)
                {
                    throw new InvalidOperationException(
                        "Um ingrediente removido não é válido para esse produto.");
                }
            }

            var adicionaisSolicitados = itemRequest.Adicionais
                .GroupBy(a => a.AdicionalId)
                .Select(g => new
                {
                    AdicionalId = g.Key,
                    Quantidade = g.Sum(a => a.Quantidade)
                })
                .ToList();

            if (adicionaisSolicitados.Any(a =>
                    a.Quantidade < 1 || a.Quantidade > 99))
            {
                throw new InvalidOperationException(
                    "A quantidade de um adicional é inválida.");
            }

            decimal totalAdicionais = 0;

            var itemPedido = new PedidoItem
            {
                ProdutoId = produto.Id,
                NomeProduto = produto.Nome,
                PrecoUnitario = produto.Preco,
                Quantidade = itemRequest.Quantidade
            };

            foreach (var ingredienteId in ingredientesRemovidos)
            {
                var ingrediente = produto.Ingredientes
                    .First(pi => pi.IngredienteId == ingredienteId)
                    .Ingrediente;

                itemPedido.IngredientesRemovidos.Add(
                    new PedidoItemIngrediente
                    {
                        IngredienteId = ingrediente.Id,
                        NomeIngrediente = ingrediente.Nome
                    });
            }

            foreach (var adicionalRequest in adicionaisSolicitados)
            {
                var adicionalProduto = produto.Adicionais
                    .FirstOrDefault(pa =>
                        pa.AdicionalId == adicionalRequest.AdicionalId);

                if (adicionalProduto is null ||
                    !adicionalProduto.Adicional.Ativo ||
                    !adicionalProduto.Adicional.Disponivel)
                {
                    throw new InvalidOperationException(
                        "Um adicional não está disponível para esse produto.");
                }

                var adicional = adicionalProduto.Adicional;

                itemPedido.Adicionais.Add(new PedidoItemAdicional
                {
                    AdicionalId = adicional.Id,
                    NomeAdicional = adicional.Nome,
                    PrecoUnitario = adicional.Preco,
                    Quantidade = adicionalRequest.Quantidade
                });

                totalAdicionais +=
                    adicional.Preco * adicionalRequest.Quantidade;
            }

            itemPedido.TotalItem =
                (produto.Preco + totalAdicionais) * itemRequest.Quantidade;

            valorTotal += itemPedido.TotalItem;
            pedido.Itens.Add(itemPedido);
        }

        pedido.ValorTotal = valorTotal;

        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync();

        // Registra a comanda na fila de impressão.
        context.ImpressoesComandas.Add(new ImpressaoComanda
        {
            PedidoId = pedido.Id,
            Status = "Pendente"
        });

        await context.SaveChangesAsync();

        await transaction.CommitAsync();

        return pedido.Id;
    }

    public async Task<List<PedidoCozinhaDto>> ListarPedidosCozinhaAsync()
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var pedidos = await context.Pedidos
            .AsNoTracking()
            .Where(p =>
                p.StatusPreparo != StatusPreparo.Entregue &&
                p.StatusPreparo != StatusPreparo.Cancelado)
            .OrderBy(p => p.CriadoEm)
            .Select(p => new PedidoCozinhaDto
            {
                Id = p.Id,
                CriadoEm = p.CriadoEm,
                NomeCliente = p.NomeCliente,
                StatusPreparo = p.StatusPreparo.ToString(),
                Observacao = p.Observacao,

                Itens = p.Itens.Select(i => new ItemPedidoCozinhaDto
                {
                    NomeProduto = i.NomeProduto,
                    Quantidade = i.Quantidade,
                    Observacao = i.Observacao,

                    IngredientesRemovidos = i.IngredientesRemovidos
                        .Select(ing => ing.NomeIngrediente)
                        .ToList(),

                    Adicionais = i.Adicionais
                        .Select(a =>
                            a.Quantidade > 1
                                ? $"{a.Quantidade}x {a.NomeAdicional}"
                                : a.NomeAdicional)
                        .ToList()
                }).ToList()
            })
            .ToListAsync();

        return pedidos;
    }

    public async Task AtualizarStatusPreparoAsync(
        long pedidoId,
        StatusPreparo novoStatus)
    {
        var transicoesPermitidas = novoStatus switch
        {
            StatusPreparo.EmPreparo =>
                new[] { StatusPreparo.Novo },

            StatusPreparo.Pronto =>
                new[] { StatusPreparo.EmPreparo },

            StatusPreparo.Entregue =>
                new[] { StatusPreparo.Pronto },

            StatusPreparo.Cancelado =>
                new[]
                {
                StatusPreparo.Novo,
                StatusPreparo.EmPreparo
                },

            _ => Array.Empty<StatusPreparo>()
        };

        if (transicoesPermitidas.Length == 0)
        {
            throw new InvalidOperationException(
                "Transição de status inválida.");
        }

        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var pedido = await context.Pedidos
            .FirstOrDefaultAsync(p => p.Id == pedidoId);

        if (pedido is null)
        {
            throw new InvalidOperationException(
                "Pedido não encontrado.");
        }

        if (!transicoesPermitidas.Contains(pedido.StatusPreparo))
        {
            throw new InvalidOperationException(
                "O pedido não pode passar para esse status.");
        }

        pedido.StatusPreparo = novoStatus;

        await context.SaveChangesAsync();
    }


    public async Task<ResumoVendasDto> ObterResumoVendasAsync(
        DateTime inicio,
        DateTime fim)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var pedidos = await context.Pedidos
            .AsNoTracking()
            .Include(p => p.Itens)
            .Where(p =>
                p.CriadoEm >= inicio &&
                p.CriadoEm < fim &&
                p.StatusPreparo != StatusPreparo.Cancelado)
            .ToListAsync();

        var resumo = new ResumoVendasDto
        {
            QuantidadePedidos = pedidos.Count,
            QuantidadeItens = pedidos
                .SelectMany(p => p.Itens)
                .Sum(i => i.Quantidade),

            ValorTotalPedidos = pedidos
                .Sum(p => p.ValorTotal),

            Produtos = pedidos
                .SelectMany(p => p.Itens)
                .GroupBy(i => i.NomeProduto)
                .Select(grupo => new ProdutoVendidoDto
                {
                    NomeProduto = grupo.Key,
                    Quantidade = grupo.Sum(i => i.Quantidade),
                    ValorTotal = grupo.Sum(i => i.TotalItem)
                })
                .OrderByDescending(p => p.Quantidade)
                .ToList()
        };

        return resumo;
    }


    public async Task<ComandaDto?> ObterComandaAsync(long pedidoId)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var pedido = await context.Pedidos
            .AsNoTracking()
            .Where(p => p.Id == pedidoId)
            .Select(p => new ComandaDto
            {
                Id = p.Id,
                CriadoEm = p.CriadoEm,
                NomeCliente = p.NomeCliente,
                Observacao = p.Observacao,
                ValorTotal = p.ValorTotal,

                Itens = p.Itens.Select(i => new ItemComandaDto
                {
                    NomeProduto = i.NomeProduto,
                    PrecoUnitario = i.PrecoUnitario,
                    Quantidade = i.Quantidade,
                    TotalItem = i.TotalItem,
                    Observacao = i.Observacao,

                    IngredientesRemovidos = i.IngredientesRemovidos
                        .Select(ing => ing.NomeIngrediente)
                        .ToList(),

                    Adicionais = i.Adicionais
                        .Select(a => new AdicionalComandaDto
                        {
                            NomeAdicional = a.NomeAdicional,
                            PrecoUnitario = a.PrecoUnitario,
                            Quantidade = a.Quantidade
                        })
                        .ToList()
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return pedido;
    }

}