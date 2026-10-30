using System.Globalization;
using System.Text;
using BlazorApp.Services.Impressao;
using System.Runtime.Versioning;
using BlazorApp.Data;
using BlazorApp.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class ImpressaoComandaService(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    IPedidoService pedidoService,
    ImpressoraWindows impressoraWindows)
{


    public async Task<List<long>> ListarPendentesAsync()
    {
        await using var context =
            await contextFactory.CreateDbContextAsync();

        var agora = DateTime.UtcNow;
        var limiteTentativa = agora.AddMinutes(-1);

        // Encaminha para revisão as comandas que atingiram
        // o limite de tentativas.
        await context.ImpressoesComandas
            .Where(x =>
                x.Status == "Erro" &&
                x.Tentativas >= 3)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, "Revisar")
                .SetProperty(
                    x => x.MensagemErro,
                    "Limite de 3 tentativas de impressão atingido. Verifique a impressora e a situação da comanda antes de reenviar."));

        return await context.ImpressoesComandas
            .AsNoTracking()
            .Where(x =>
                x.Status == "Pendente" ||
                (x.Status == "Erro" &&
                 x.Tentativas < 3 &&
                 (x.UltimaTentativaEm == null ||
                  x.UltimaTentativaEm <= limiteTentativa)))
            .OrderBy(x => x.CriadoEm)
            .Select(x => x.PedidoId)
            .ToListAsync();
    }

    public async Task<ComandaDto?> ObterComandaAsync(long pedidoId)
    {
        return await pedidoService.ObterComandaAsync(pedidoId);
    }

    public async Task AtualizarStatusAsync(
        long pedidoId,
        string status,
        string? mensagemErro = null)
    {
        await using var context =
            await contextFactory.CreateDbContextAsync();

        var impressao = await context.ImpressoesComandas
            .FirstOrDefaultAsync(x => x.PedidoId == pedidoId);

        if (impressao is null)
            return;

        impressao.Status = status;
        impressao.MensagemErro = mensagemErro;

        if (status == "Impresso")
        {
            impressao.ImpressoEm = DateTime.UtcNow;
            impressao.MensagemErro = null;
        }

        await context.SaveChangesAsync();
    }

    public async Task<string?> MontarTextoComandaAsync(long pedidoId)
    {
        var comanda = await ObterComandaAsync(pedidoId);

        if (comanda is null)
            return null;

        var texto = new StringBuilder();
        var cultura = CultureInfo.GetCultureInfo("pt-BR");

        texto.AppendLine("================================");
        texto.AppendLine("       COMANDA DE PREPARO");
        texto.AppendLine("================================");
        texto.AppendLine($"PEDIDO: #{comanda.Id}");
        texto.AppendLine($"CLIENTE: {comanda.NomeCliente}");
        texto.AppendLine(
            $"DATA: {comanda.CriadoEm.ToLocalTime():dd/MM/yyyy HH:mm}");
        texto.AppendLine("--------------------------------");

        foreach (var item in comanda.Itens)
        {
            texto.AppendLine(
                $"{item.Quantidade}x {item.NomeProduto}");

            texto.AppendLine(
                $"   Unit.: {item.PrecoUnitario.ToString("C", cultura)}");

            texto.AppendLine(
                $"   Total: {item.TotalItem.ToString("C", cultura)}");

            if (item.IngredientesRemovidos.Count > 0)
            {
                texto.AppendLine("   SEM:");

                foreach (var ingrediente in item.IngredientesRemovidos)
                    texto.AppendLine($"   - {ingrediente}");
            }

            if (item.Adicionais.Count > 0)
            {
                texto.AppendLine("   ADICIONAIS:");

                foreach (var adicional in item.Adicionais)
                {
                    texto.AppendLine(
                        $"   - {adicional.Quantidade}x {adicional.NomeAdicional}");
                }
            }

            if (!string.IsNullOrWhiteSpace(item.Observacao))
            {
                texto.AppendLine($"   OBS.: {item.Observacao}");
            }

            texto.AppendLine("--------------------------------");
        }

        if (!string.IsNullOrWhiteSpace(comanda.Observacao))
        {
            texto.AppendLine("OBSERVACAO DO PEDIDO:");
            texto.AppendLine(comanda.Observacao);
            texto.AppendLine("--------------------------------");
        }

        texto.AppendLine(
            $"TOTAL: {comanda.ValorTotal.ToString("C", cultura)}");

        texto.AppendLine("================================");
        texto.AppendLine();

        return texto.ToString();
    }

    [SupportedOSPlatform("windows6.1")]
    public async Task<bool> ImprimirComandaAsync(
        long pedidoId,
        string? nomeImpressora = null)
    {
        try
        {
            var texto = await MontarTextoComandaAsync(pedidoId);

            if (texto is null)
            {
                await AtualizarStatusAsync(
                    pedidoId,
                    "Erro",
                    "Pedido não encontrado.");

                return false;
            }

            impressoraWindows.ImprimirTexto(texto, nomeImpressora);

            await AtualizarStatusAsync(pedidoId, "Impresso");

            return true;
        }
        catch (Exception ex)
        {
            try
            {
                await AtualizarStatusAsync(
                    pedidoId,
                    "Revisar",
                    $"Resultado da impressão incerto. Verifique se a comanda saiu antes de reenviar. Detalhes: {ex.Message}");
            }
            catch (Exception erroAtualizacao)
            {
                // O estado no banco também pode estar inconsistente.
                // Registre esse problema no log da aplicação.
                System.Diagnostics.Trace.TraceError(
                    $"Falha ao atualizar o status do pedido {pedidoId}: {erroAtualizacao}");
            }

            return false;
        }
    }

    public async Task<bool> ReservarComandaAsync(long pedidoId)
    {
        await using var context =
            await contextFactory.CreateDbContextAsync();

        var quantidade = await context.ImpressoesComandas
            .Where(x =>
                x.PedidoId == pedidoId &&
                (x.Status == "Pendente" ||
                 (x.Status == "Erro" &&
                  (x.UltimaTentativaEm == null ||
                   x.UltimaTentativaEm <= DateTime.UtcNow.AddMinutes(-1)))))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, "Processando")
                .SetProperty(x => x.UltimaTentativaEm, DateTime.UtcNow)
                .SetProperty(x => x.Tentativas, x => x.Tentativas + 1)
                .SetProperty(x => x.MensagemErro, (string?)null));

        return quantidade == 1;
    }

    public async Task<int> MarcarProcessandoParaRevisaoAsync()
    {
        await using var context =
            await contextFactory.CreateDbContextAsync();

        return await context.ImpressoesComandas
            .Where(x => x.Status == "Processando")
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, "Revisar")
                .SetProperty(
                    x => x.MensagemErro,
                    "A aplicação foi reiniciada durante o processamento. Verifique se a comanda foi impressa antes de reenviar."));
    }

    public async Task<List<ImpressaoComandaDto>> ListarComandasParaRevisaoAsync()
    {
        await using var context =
            await contextFactory.CreateDbContextAsync();

        return await context.ImpressoesComandas
            .AsNoTracking()
            .Where(x =>
                x.Status == "Erro" ||
                x.Status == "Revisar")
            .OrderByDescending(x => x.CriadoEm)
            .Select(x => new ImpressaoComandaDto
            {
                PedidoId = x.PedidoId,
                NomeCliente = x.Pedido.NomeCliente,
                Status = x.Status,
                Tentativas = x.Tentativas,
                CriadoEm = x.CriadoEm,
                UltimaTentativaEm = x.UltimaTentativaEm,
                ImpressoEm = x.ImpressoEm,
                MensagemErro = x.MensagemErro
            })
            .ToListAsync();
    }

    public async Task<bool> ReprocessarManualAsync(long pedidoId)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            throw new PlatformNotSupportedException(
                "A impressão manual está disponível somente no Windows.");

        await using var context =
            await contextFactory.CreateDbContextAsync();

        var quantidade = await context.ImpressoesComandas
            .Where(x =>
                x.PedidoId == pedidoId &&
                (x.Status == "Erro" || x.Status == "Revisar"))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, "Processando")
                .SetProperty(x => x.UltimaTentativaEm, DateTime.UtcNow)
                .SetProperty(x => x.Tentativas, x => x.Tentativas + 1)
                .SetProperty(x => x.MensagemErro, (string?)null));

        if (quantidade != 1)
            return false;

        return await ImprimirComandaAsync(pedidoId);
    }

}
