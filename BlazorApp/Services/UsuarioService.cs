using BlazorApp.Data;
using BlazorApp.Models.Dtos;
using BlazorApp.Models.Requests;
using BlazorApp.Services.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services
{
    // Services/UsuarioService.cs
    public class UsuarioService : IUsuarioService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private static readonly string[] RolesValidas = ["Admin", "Supervisor" , "Usuario"];

        public UsuarioService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<Result<List<UsuarioListItem>>> List(string? busca = null)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var query = userManager.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                query = query.Where(u => u.Nome.ToLower().Contains(termo) || u.Email!.ToLower().Contains(termo));
            }

            var usuarios = await EntityFrameworkQueryableExtensions.ToListAsync(query);

            var lista = new List<UsuarioListItem>();
            foreach (var u in usuarios)
            {
                var roles = await userManager.GetRolesAsync(u);
                lista.Add(new UsuarioListItem(u.Id, u.Nome, u.Email!, roles.FirstOrDefault() ?? "", u.Ativo));
            }

            return Result<List<UsuarioListItem>>.Ok(lista);
        }

        public async Task<Result<UsuarioListItem>> GetById(string id)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var usuario = await userManager.FindByIdAsync(id);
            if (usuario is null)
                return Result<UsuarioListItem>.Falha("Usuário não encontrado.");

            var roles = await userManager.GetRolesAsync(usuario);
            return Result<UsuarioListItem>.Ok(
                new UsuarioListItem(usuario.Id, usuario.Nome, usuario.Email!, roles.FirstOrDefault() ?? "", usuario.Ativo));
        }

        public async Task<Result> Create(CriarUsuarioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nome) || request.Nome.Trim().Length < 2)
                return Result.Falha("Nome deve ter ao menos 2 caracteres.");

            if (!RolesValidas.Contains(request.Role))
                return Result.Falha("Role inválida.");

            await using var scope = _scopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (await userManager.FindByEmailAsync(request.Email) is not null)
                return Result.Falha("Já existe um usuário com este e-mail.");

            var usuario = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true,
                Nome = request.Nome.Trim(),
                Ativo = true
            };

            var resultado = await userManager.CreateAsync(usuario, request.Senha);
            if (!resultado.Succeeded)
                return Result.Falha(TraduzirErros(resultado));

            await userManager.AddToRoleAsync(usuario, request.Role);
            return Result.Ok();
        }

        public async Task<Result> Update(string id, AtualizarUsuarioRequest request, string usuarioLogadoId)
        {
            if (string.IsNullOrWhiteSpace(request.Nome) || request.Nome.Trim().Length < 2)
                return Result.Falha("Nome deve ter ao menos 2 caracteres.");

            if (!RolesValidas.Contains(request.Role))
                return Result.Falha("Role inválida.");

            if (id == usuarioLogadoId && request.Role != "Admin")
                return Result.Falha("Você não pode remover sua própria permissão de Admin.");

            await using var scope = _scopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var usuario = await userManager.FindByIdAsync(id);
            if (usuario is null)
                return Result.Falha("Usuário não encontrado.");

            usuario.Nome = request.Nome.Trim();
            var resultado = await userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
                return Result.Falha(TraduzirErros(resultado));

            var rolesAtuais = await userManager.GetRolesAsync(usuario);
            if (!rolesAtuais.Contains(request.Role))
            {
                await userManager.RemoveFromRolesAsync(usuario, rolesAtuais);
                await userManager.AddToRoleAsync(usuario, request.Role);
            }

            return Result.Ok();
        }

        public async Task<Result> Desativar(string id, string usuarioLogadoId)
        {
            if (id == usuarioLogadoId)
                return Result.Falha("Você não pode desativar seu próprio usuário.");

            await using var scope = _scopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var usuario = await userManager.FindByIdAsync(id);
            if (usuario is null)
                return Result.Falha("Usuário não encontrado.");
            if (!usuario.Ativo)
                return Result.Falha("Usuário já está inativo.");

            usuario.Ativo = false;
            var resultado = await userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
                return Result.Falha(TraduzirErros(resultado));

            // Derruba qualquer circuito/sessão aberta desse usuário
            await userManager.UpdateSecurityStampAsync(usuario);

            return Result.Ok();
        }

        public async Task<Result> Reativar(string id)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var usuario = await userManager.FindByIdAsync(id);
            if (usuario is null)
                return Result.Falha("Usuário não encontrado.");
            if (usuario.Ativo)
                return Result.Falha("Usuário já está ativo.");

            usuario.Ativo = true;
            var resultado = await userManager.UpdateAsync(usuario);
            return resultado.Succeeded ? Result.Ok() : Result.Falha(TraduzirErros(resultado));
        }

        public async Task<Result> RedefinirSenha(string id, string novaSenha)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var usuario = await userManager.FindByIdAsync(id);
            if (usuario is null)
                return Result.Falha("Usuário não encontrado.");

            var token = await userManager.GeneratePasswordResetTokenAsync(usuario);
            var resultado = await userManager.ResetPasswordAsync(usuario, token, novaSenha);
            return resultado.Succeeded ? Result.Ok() : Result.Falha(TraduzirErros(resultado));
        }

        private static string TraduzirErros(IdentityResult resultado) =>
            string.Join(" ", resultado.Errors.Select(e => e.Description));
    }
}
