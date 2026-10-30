
using BlazorApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Runtime.Versioning;

namespace BlazorApp.Services.Impressao;

[SupportedOSPlatform("windows6.1")]
public class ProcessadorFilaImpressao(
    IServiceScopeFactory scopeFactory,
    ILogger<ProcessadorFilaImpressao> logger,
    IConfiguration configuration)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var habilitada = configuration.GetValue<bool>(
            "Impressao:Habilitada");

        var intervalo = configuration.GetValue<int>(
            "Impressao:IntervaloSegundos", 10);

        if (intervalo < 1)
            intervalo = 10;

        if (!habilitada)
        {
            logger.LogInformation(
                "Impressão automática desabilitada pela configuração.");
        }
        else
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var servico = scope.ServiceProvider
                    .GetRequiredService<ImpressaoComandaService>();

                var quantidade = await servico
                    .MarcarProcessandoParaRevisaoAsync();

                if (quantidade > 0)
                {
                    logger.LogWarning(
                        "{Quantidade} comandas foram marcadas para revisão após a inicialização.",
                        quantidade);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Erro ao recuperar comandas interrompidas na inicialização.");
            }
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            if (habilitada)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();

                    var servico = scope.ServiceProvider
                        .GetRequiredService<ImpressaoComandaService>();

                    var pedidos = await servico.ListarPendentesAsync();


                    foreach (var pedidoId in pedidos)
                    {
                        if (stoppingToken.IsCancellationRequested)
                            break;

                        try
                        {
                            var reservado =
                                await servico.ReservarComandaAsync(pedidoId);

                            if (!reservado)
                                continue;

                            var sucesso =
                                await servico.ImprimirComandaAsync(pedidoId);

                            if (!sucesso)
                            {
                                logger.LogWarning(
                                    "Falha ao imprimir a comanda do pedido {PedidoId}.",
                                    pedidoId);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(
                                ex,
                                "Erro inesperado ao processar a comanda do pedido {PedidoId}.",
                                pedidoId);

                            try
                            {
                                await servico.AtualizarStatusAsync(
                                    pedidoId,
                                    "Revisar",
                                    "Ocorreu um erro inesperado durante o processamento. Verifique se a comanda foi impressa antes de reenviar.");
                            }
                            catch (Exception erroAtualizacao)
                            {
                                logger.LogError(
                                    erroAtualizacao,
                                    "Não foi possível atualizar o status da comanda {PedidoId}.",
                                    pedidoId);
                            }
                        }
                    }

                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Erro ao processar a fila de impressão.");
                }
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(intervalo),
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
