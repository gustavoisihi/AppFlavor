using BlazorApp.Models.Dtos;
using BlazorApp.Models.Entities;
using BlazorApp.Models.Enums;
using BlazorApp.Models.Requests;

namespace BlazorApp.Services;

public interface IPedidoService
{
    Task<long> CriarPedidoAsync(CriarPedidoRequest request);
    Task<List<PedidoCozinhaDto>> ListarPedidosCozinhaAsync();
    Task AtualizarStatusPreparoAsync(long pedidoId,StatusPreparo novoStatus);
    Task<ResumoVendasDto> ObterResumoVendasAsync(DateTime inicio,DateTime fim);
}