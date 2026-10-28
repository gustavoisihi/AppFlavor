namespace BlazorApp.Models.Requests
{
    // Models/Requests/CriarUsuarioRequest.cs
    public class CriarUsuarioRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Role { get; set; } = "Usuario";
    }

    // Models/Requests/AtualizarUsuarioRequest.cs
    public class AtualizarUsuarioRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Role { get; set; } = "Usuario";
    }
}
