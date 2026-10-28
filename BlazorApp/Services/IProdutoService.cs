
using BlazorApp.Models.Dtos;
using BlazorApp.Models.Entities;
namespace BlazorApp.Services;

public interface IProdutoService
{
    Task<List<Produto>> ListarTodosAsync();
    Task<List<Produto>> ListarDisponiveisAsync();
    Task<Produto?> ObterPorIdAsync(int id);
    Task CriarAsync(Produto produto);
    Task AtualizarAsync(Produto produto);
    Task<List<IngredienteProdutoDto>> ListarIngredientesDoProdutoAsync(int produtoId);
    Task SalvarIngredientesDoProdutoAsync(int produtoId,List<IngredienteProdutoDto> ingredientes);
    Task<List<AdicionalProdutoDto>> ListarAdicionaisDoProdutoAsync(int produtoId);
    Task SalvarAdicionaisDoProdutoAsync(int produtoId,List<AdicionalProdutoDto> adicionais);
    Task<List<ProdutoCardapioDto>> ListarCardapioAsync();

}
