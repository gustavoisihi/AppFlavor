
using BlazorApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Data;

public static class CardapioSeeder
{
    private sealed record ProdutoInicial(
        string Categoria,
        string Nome,
        string Descricao,
        decimal Preco,
        int Ordem,
        string[] Ingredientes,
        string[] Adicionais);

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var factory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

        await using var db = await factory.CreateDbContextAsync();

        // 1. Categorias
        var categoriasIniciais = new[]
        {
            ("Hambúrgueres", 1),
            ("Combos", 2),
            ("Porções", 3),
            ("Bebidas", 4),
            ("Sobremesas", 5)
        };

        var categorias = await db.Categorias.ToListAsync();

        foreach (var (nome, ordem) in categoriasIniciais)
        {
            if (!categorias.Any(c =>
                string.Equals(c.Nome, nome,
                    StringComparison.OrdinalIgnoreCase)))
            {
                var categoria = new Categoria
                {
                    Nome = nome,
                    OrdemExibicao = ordem,
                    Ativo = true
                };

                db.Categorias.Add(categoria);
                categorias.Add(categoria);
            }
        }

        await db.SaveChangesAsync();

        // 2. Ingredientes
        var nomesIngredientes = new[]
        {
            "Pão brioche",
            "Hambúrguer bovino 100g",
            "Hambúrguer bovino 150g",
            "Queijo cheddar",
            "Queijo muçarela",
            "Alface",
            "Tomate",
            "Cebola roxa",
            "Picles",
            "Bacon",
            "Ovo",
            "Cebola caramelizada",
            "Molho da casa",
            "Maionese",
            "Ketchup",
            "Mostarda"
        };

        var ingredientes = await db.Ingredientes.ToListAsync();

        foreach (var nome in nomesIngredientes)
        {
            if (!ingredientes.Any(i =>
                string.Equals(i.Nome, nome,
                    StringComparison.OrdinalIgnoreCase)))
            {
                var ingrediente = new Ingrediente
                {
                    Nome = nome,
                    Ativo = true,
                    Disponivel = true
                };

                db.Ingredientes.Add(ingrediente);
                ingredientes.Add(ingrediente);
            }
        }

        // 3. Adicionais
        var adicionaisIniciais = new (string Nome, decimal Preco)[]
        {
            ("Bacon extra", 4.90m),
            ("Cheddar extra", 3.90m),
            ("Hambúrguer extra 100g", 6.90m),
            ("Ovo", 2.90m),
            ("Cebola caramelizada extra", 2.90m),
            ("Molho da casa extra", 1.90m),
            ("Muçarela extra", 3.50m)
        };

        var adicionais = await db.Adicionais.ToListAsync();

        foreach (var (nome, preco) in adicionaisIniciais)
        {
            if (!adicionais.Any(a =>
                string.Equals(a.Nome, nome,
                    StringComparison.OrdinalIgnoreCase)))
            {
                var adicional = new Adicional
                {
                    Nome = nome,
                    Preco = preco,
                    Ativo = true,
                    Disponivel = true
                };

                db.Adicionais.Add(adicional);
                adicionais.Add(adicional);
            }
        }

        await db.SaveChangesAsync();

        // 4. Produtos e seus relacionamentos
        var produtosIniciais = new[]
        {
            new ProdutoInicial(
                "Hambúrgueres",
                "Smash Burger",
                "Pão brioche, carne smash, cheddar e molho da casa.",
                24.90m, 1,
                new[] { "Pão brioche", "Hambúrguer bovino 100g",
                    "Queijo cheddar", "Molho da casa" },
                new[] { "Bacon extra", "Cheddar extra",
                    "Hambúrguer extra 100g", "Ovo",
                    "Cebola caramelizada extra", "Molho da casa extra" }),

            new ProdutoInicial(
                "Hambúrgueres",
                "Cheeseburger Artesanal",
                "Pão brioche, hambúrguer bovino, muçarela e molho da casa.",
                27.90m, 2,
                new[] { "Pão brioche", "Hambúrguer bovino 150g",
                    "Queijo muçarela", "Molho da casa" },
                new[] { "Bacon extra", "Cheddar extra",
                    "Hambúrguer extra 100g", "Ovo",
                    "Cebola caramelizada extra", "Molho da casa extra",
                    "Muçarela extra" }),

            new ProdutoInicial(
                "Hambúrgueres",
                "Bacon Burger",
                "Hambúrguer bovino, cheddar, bacon e molho da casa.",
                29.90m, 3,
                new[] { "Pão brioche", "Hambúrguer bovino 150g",
                    "Queijo cheddar", "Bacon", "Molho da casa" },
                new[] { "Bacon extra", "Cheddar extra",
                    "Hambúrguer extra 100g", "Ovo",
                    "Cebola caramelizada extra", "Molho da casa extra" }),

            new ProdutoInicial(
                "Hambúrgueres",
                "Duplo Cheddar",
                "Dois hambúrgueres, cheddar cremoso e pão brioche.",
                36.90m, 4,
                new[] { "Pão brioche", "Hambúrguer bovino 150g",
                    "Queijo cheddar", "Molho da casa" },
                new[] { "Bacon extra", "Cheddar extra",
                    "Hambúrguer extra 100g", "Ovo",
                    "Molho da casa extra" }),

            new ProdutoInicial(
                "Hambúrgueres",
                "Especial da Casa",
                "Hambúrguer artesanal com cheddar, bacon, cebola caramelizada e molho da casa.",
                34.90m, 5,
                new[] { "Pão brioche", "Hambúrguer bovino 150g",
                    "Queijo cheddar", "Bacon", "Cebola caramelizada",
                    "Molho da casa", "Alface", "Tomate" },
                new[] { "Bacon extra", "Cheddar extra",
                    "Hambúrguer extra 100g", "Ovo",
                    "Cebola caramelizada extra", "Molho da casa extra" }),

            new ProdutoInicial(
                "Combos",
                "Combo Smash",
                "Smash Burger acompanhado de batata pequena e refrigerante em lata.",
                39.90m, 1,
                Array.Empty<string>(),
                Array.Empty<string>()),

            new ProdutoInicial(
                "Combos",
                "Combo Bacon",
                "Bacon Burger acompanhado de batata pequena e refrigerante em lata.",
                44.90m, 2,
                Array.Empty<string>(),
                Array.Empty<string>()),

            new ProdutoInicial(
                "Porções",
                "Batata Frita Pequena",
                "Porção individual de batatas fritas.",
                12.90m, 1,
                Array.Empty<string>(),
                Array.Empty<string>()),

            new ProdutoInicial(
                "Porções",
                "Batata Frita Média",
                "Porção média de batatas fritas.",
                16.90m, 2,
                Array.Empty<string>(),
                Array.Empty<string>()),

            new ProdutoInicial(
                "Porções",
                "Batata Frita Grande",
                "Porção grande de batatas fritas.",
                21.90m, 3,
                Array.Empty<string>(),
                Array.Empty<string>()),

            new ProdutoInicial(
                "Porções",
                "Batata com Cheddar e Bacon",
                "Batatas fritas cobertas com cheddar e bacon.",
                29.90m, 4,
                Array.Empty<string>(),
                Array.Empty<string>()),

            new ProdutoInicial(
                "Porções",
                "Onion Rings",
                "Porção de anéis de cebola empanados.",
                19.90m, 5,
                Array.Empty<string>(),
                Array.Empty<string>()),

            new ProdutoInicial(
                "Porções",
                "Nuggets 10 unidades",
                "Porção com dez nuggets.",
                16.90m, 6,
                Array.Empty<string>(),
                Array.Empty<string>()),

            new ProdutoInicial(
                "Bebidas",
                "Refrigerante Lata",
                "Refrigerante em lata de 350 ml. Consulte os sabores disponíveis.",
                6.90m, 1,
                Array.Empty<string>(),
                Array.Empty<string>()),

            new ProdutoInicial(
                "Bebidas",
                "Refrigerante 600 ml",
                "Refrigerante de 600 ml. Consulte os sabores disponíveis.",
                8.90m, 2,
                Array.Empty<string>(),
                Array.Empty<string>()),

            new ProdutoInicial(
                "Bebidas",
                "Água Mineral sem Gás",
                "Garrafa de água mineral sem gás.",
                4.90m, 3,
                Array.Empty<string>(),
                Array.Empty<string>()),

            new ProdutoInicial(
                "Bebidas",
                "Água Mineral com Gás",
                "Garrafa de água mineral com gás.",
                5.90m, 4,
                Array.Empty<string>(),
                Array.Empty<string>()),

            new ProdutoInicial(
                "Bebidas",
                "Suco Natural",
                "Suco natural. Consulte os sabores disponíveis.",
                7.90m, 5,
                Array.Empty<string>(),
                Array.Empty<string>()),

            new ProdutoInicial(
                "Sobremesas",
                "Brownie",
                "Brownie de chocolate em porção individual.",
                9.90m, 1,
                Array.Empty<string>(),
                Array.Empty<string>()),

            new ProdutoInicial(
                "Sobremesas",
                "Sorvete",
                "Porção individual de sorvete. Consulte os sabores disponíveis.",
                7.90m, 2,
                Array.Empty<string>(),
                Array.Empty<string>())
        };

        // Recarrega para incluir registros que já existiam no banco.
        categorias = await db.Categorias.ToListAsync();
        ingredientes = await db.Ingredientes.ToListAsync();
        adicionais = await db.Adicionais.ToListAsync();

        var produtos = await db.Produtos.ToListAsync();

        foreach (var item in produtosIniciais)
        {
            var categoria = categorias.First(c =>
                string.Equals(c.Nome, item.Categoria,
                    StringComparison.OrdinalIgnoreCase));

            var produto = produtos.FirstOrDefault(p =>
                string.Equals(p.Nome, item.Nome,
                    StringComparison.OrdinalIgnoreCase));

            if (produto is null)
            {
                produto = new Produto
                {
                    Nome = item.Nome,
                    Descricao = item.Descricao,
                    CategoriaId = categoria.Id,
                    Preco = item.Preco,
                    OrdemExibicao = item.Ordem,
                    Ativo = true,
                    Disponivel = true,
                    ImagemUrl = ObterImagem(item.Nome)
                };

                db.Produtos.Add(produto);
                await db.SaveChangesAsync();
                produtos.Add(produto);
            }
            else if (string.IsNullOrWhiteSpace(produto.ImagemUrl))
            {
                produto.ImagemUrl = ObterImagem(item.Nome);
            }

            // Não altera associações que já existam.
            var ingredientesAssociados = await db.ProdutosIngredientes
                .Where(pi => pi.ProdutoId == produto.Id)
                .Select(pi => pi.IngredienteId)
                .ToListAsync();

            foreach (var nomeIngrediente in item.Ingredientes)
            {
                var ingrediente = ingredientes.First(i =>
                    string.Equals(i.Nome, nomeIngrediente,
                        StringComparison.OrdinalIgnoreCase));

                if (!ingredientesAssociados.Contains(ingrediente.Id))
                {
                    db.ProdutosIngredientes.Add(new ProdutoIngrediente
                    {
                        ProdutoId = produto.Id,
                        IngredienteId = ingrediente.Id,
                        InclusoPorPadrao = true,
                        PermiteRemover = true
                    });
                }
            }

            var adicionaisAssociados = await db.ProdutosAdicionais
                .Where(pa => pa.ProdutoId == produto.Id)
                .Select(pa => pa.AdicionalId)
                .ToListAsync();

            foreach (var nomeAdicional in item.Adicionais)
            {
                var adicional = adicionais.First(a =>
                    string.Equals(a.Nome, nomeAdicional,
                        StringComparison.OrdinalIgnoreCase));

                if (!adicionaisAssociados.Contains(adicional.Id))
                {
                    db.ProdutosAdicionais.Add(new ProdutoAdicional
                    {
                        ProdutoId = produto.Id,
                        AdicionalId = adicional.Id
                    });
                }
            }

            await db.SaveChangesAsync();
        }
    }


    private static string? ObterImagem(string nomeProduto)
    {
        return nomeProduto switch
        {
            "Smash Burger" =>
                "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=800&q=85",

            "Cheeseburger Artesanal" =>
                "https://images.unsplash.com/photo-1550547660-d9450f859349?auto=format&fit=crop&w=800&q=85",

            "Bacon Burger" =>
                "https://images.unsplash.com/photo-1553979459-d2229ba7433a?auto=format&fit=crop&w=800&q=85",

            "Duplo Cheddar" =>
                "https://images.unsplash.com/photo-1571091718767-18b5b1457add?auto=format&fit=crop&w=800&q=85",

            "Especial da Casa" =>
                "https://images.unsplash.com/photo-1561758033-d89a9ad46330?auto=format&fit=crop&w=800&q=85",

            "Combo Smash" or "Combo Bacon" =>
                "https://images.unsplash.com/photo-1551782450-a2132b4ba21d?auto=format&fit=crop&w=800&q=85",

            "Batata Frita Pequena" or "Batata Frita Média"
                or "Batata Frita Grande" =>
                "https://images.unsplash.com/photo-1573080496219-bb080dd4f877?auto=format&fit=crop&w=800&q=85",

            "Batata com Cheddar e Bacon" =>
                "https://images.unsplash.com/photo-1630384060421-cb20d0e0649d?auto=format&fit=crop&w=800&q=85",

            "Onion Rings" =>
                "https://images.unsplash.com/photo-1639024471283-03518883512d?auto=format&fit=crop&w=800&q=85",

            "Nuggets 10 unidades" =>
                "https://images.unsplash.com/photo-1562967914-608f82629710?auto=format&fit=crop&w=800&q=85",

            "Refrigerante Lata" or "Refrigerante 600 ml" =>
                "https://images.unsplash.com/photo-1581636625402-29b2a704ef13?auto=format&fit=crop&w=800&q=85",

            "Água Mineral sem Gás" or "Água Mineral com Gás" =>
                "https://images.unsplash.com/photo-1523362628745-0c100150b5f7?auto=format&fit=crop&w=800&q=85",

            "Suco Natural" =>
                "https://images.unsplash.com/photo-1600271886742-f049cd451bba?auto=format&fit=crop&w=800&q=85",

            "Brownie" =>
                "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?auto=format&fit=crop&w=800&q=85",

            "Sorvete" =>
                "https://images.unsplash.com/photo-1563805042-7684c019e1cb?auto=format&fit=crop&w=800&q=85",

            _ => null
        };
    }

}
