namespace BlazorApp.Services.Common
{
    // Services/Common/Result.cs
    public class Result
    {
        public bool Sucesso { get; }
        public string? Mensagem { get; }

        protected Result(bool sucesso, string? mensagem)
        {
            Sucesso = sucesso;
            Mensagem = mensagem;
        }

        public static Result Ok() => new(true, null);
        public static Result Falha(string mensagem) => new(false, mensagem);
    }

    public class Result<T> : Result
    {
        public T? Dados { get; }

        protected Result(bool sucesso, string? mensagem, T? dados) : base(sucesso, mensagem)
        {
            Dados = dados;
        }

        public static Result<T> Ok(T dados) => new(true, null, dados);
        public static new Result<T> Falha(string mensagem) => new(false, mensagem, default);
    }
}
