using BlazorApp.Models.Dtos;
using BlazorApp.Models.Requests;
using BlazorApp.Services.Common;

namespace BlazorApp.Services
{
    // Services/IUsuarioService.cs
    public interface IUsuarioService
    {
        Task<Result<List<UsuarioListItem>>> List(string? busca = null);
        Task<Result<UsuarioListItem>> GetById(string id);
        Task<Result> Create(CriarUsuarioRequest request);
        Task<Result> Update(string id, AtualizarUsuarioRequest request, string usuarioLogadoId);
        Task<Result> Desativar(string id, string usuarioLogadoId);
        Task<Result> Reativar(string id);
        Task<Result> RedefinirSenha(string id, string novaSenha);
    }
}
