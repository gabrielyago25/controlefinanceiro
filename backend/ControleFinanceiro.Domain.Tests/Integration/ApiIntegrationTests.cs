using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ControleFinanceiro.Domain.Tests.Integration;

public sealed class ApiIntegrationTests
{
    [Fact]
    public async Task Endpoint_protegido_deve_retornar_401_sem_token()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard?mes=7&ano=2026");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Cadastro_nao_deve_retornar_senha_hash_e_email_duplicado_deve_retornar_409()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();
        var request = new
        {
            nome = "Gabriel",
            email = "gabriel@exemplo.com",
            senha = "Senha123"
        };

        var cadastro = await client.PostAsJsonAsync("/api/autenticacao/cadastro", request);
        var duplicado = await client.PostAsJsonAsync("/api/autenticacao/cadastro", request);
        var body = await cadastro.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, cadastro.StatusCode);
        Assert.DoesNotContain("senhaHash", body, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.Conflict, duplicado.StatusCode);
    }

    [Fact]
    public async Task Usuario_nao_deve_editar_categoria_de_outro_usuario()
    {
        await using var factory = new ApiFactory();
        var usuario1 = factory.CreateClient();
        var usuario2 = factory.CreateClient();

        var token1 = await CadastrarAsync(usuario1, "ana@exemplo.com");
        usuario1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var categoriaResponse = await usuario1.PostAsJsonAsync("/api/categorias-despesa", new { nome = "Moradia" });
        var categoria = await categoriaResponse.Content.ReadFromJsonAsync<ItemCriado>();

        var token2 = await CadastrarAsync(usuario2, "bruno@exemplo.com");
        usuario2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var tentativa = await usuario2.PutAsJsonAsync($"/api/categorias-despesa/{categoria!.Id}", new { nome = "Categoria invadida" });

        Assert.Equal(HttpStatusCode.Created, categoriaResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, tentativa.StatusCode);
    }

    [Fact]
    public async Task Usuario_nao_deve_acessar_dados_de_outro_usuario()
    {
        await using var factory = new ApiFactory();
        var proprietario = factory.CreateClient();
        var outroUsuario = factory.CreateClient();

        var tokenProprietario = await CadastrarAsync(proprietario, "proprietario@exemplo.com");
        proprietario.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenProprietario);
        var receitaResponse = await proprietario.PostAsJsonAsync("/api/receitas", new
        {
            descricao = "Receita privada",
            valor = 100m,
            dataRecebimento = new DateOnly(2026, 7, 10),
            mes = 7,
            ano = 2026,
            observacoes = "Somente do proprietário"
        });

        var tokenOutroUsuario = await CadastrarAsync(outroUsuario, "visitante@exemplo.com");
        outroUsuario.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenOutroUsuario);

        var receitasOutroUsuario = await outroUsuario.GetFromJsonAsync<List<ItemCriado>>("/api/receitas?mes=7&ano=2026");

        Assert.Equal(HttpStatusCode.Created, receitaResponse.StatusCode);
        Assert.Empty(receitasOutroUsuario!);
    }

    [Fact]
    public async Task Refresh_deve_rotacionar_cookie_e_logout_deve_revogar_refresh_token()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();

        await CadastrarAsync(client, "maria@exemplo.com");
        var refresh = await client.PostAsync("/api/autenticacao/refresh", null);
        var logout = await client.PostAsync("/api/autenticacao/logout", null);
        var refreshDepoisLogout = await client.PostAsync("/api/autenticacao/refresh", null);

        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshDepoisLogout.StatusCode);
    }

    [Fact]
    public async Task Compras_e_limite_utilizado_devem_respeitar_o_fechamento_do_cartao()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();
        var token = await CadastrarAsync(client, "cartoes@exemplo.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var cartaoResponse = await client.PostAsJsonAsync("/api/cartoes", new
        {
            nome = "Cartão mensal",
            banco = "Banco Teste",
            bandeira = "Visa",
            limite = 1000m,
            diaFechamento = 10,
            diaVencimento = 17,
            cor = "#1239c5"
        });
        cartaoResponse.EnsureSuccessStatusCode();
        var cartao = await cartaoResponse.Content.ReadFromJsonAsync<ItemCriado>();

        await CriarCompraAsync(client, cartao!.Id, "Compra antes do fechamento", 300m, new DateOnly(2026, 7, 9), 3);
        await CriarCompraAsync(client, cartao.Id, "Compra após o fechamento", 80m, new DateOnly(2026, 7, 11), 1);

        var comprasJulho = await client.GetFromJsonAsync<List<CompraResposta>>($"/api/cartoes/{cartao.Id}/compras?mes=7&ano=2026");
        var comprasAgosto = await client.GetFromJsonAsync<List<CompraResposta>>($"/api/cartoes/{cartao.Id}/compras?mes=8&ano=2026");
        var comprasSetembro = await client.GetFromJsonAsync<List<CompraResposta>>($"/api/cartoes/{cartao.Id}/compras?mes=9&ano=2026");
        var cartoesJulho = await client.GetFromJsonAsync<List<CartaoResposta>>("/api/cartoes?mes=7&ano=2026");
        var cartoesAgosto = await client.GetFromJsonAsync<List<CartaoResposta>>("/api/cartoes?mes=8&ano=2026");
        var cartoesSetembro = await client.GetFromJsonAsync<List<CartaoResposta>>("/api/cartoes?mes=9&ano=2026");

        Assert.Collection(comprasJulho!, compra => Assert.Equal("Compra antes do fechamento", compra.Descricao));
        Assert.Collection(comprasAgosto!, compra => Assert.Equal("Compra após o fechamento", compra.Descricao));
        Assert.Empty(comprasSetembro!);
        Assert.Equal(300m, Assert.Single(cartoesJulho!).LimiteUtilizado);
        Assert.Equal(80m, Assert.Single(cartoesAgosto!).LimiteUtilizado);
        Assert.Equal(0m, Assert.Single(cartoesSetembro!).LimiteUtilizado);
    }

    private static async Task<string> CadastrarAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/autenticacao/cadastro", new
        {
            nome = "Usuário Teste",
            email,
            senha = "Senha123"
        });

        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResposta>();
        return auth!.AccessToken;
    }

    private static async Task CriarCompraAsync(HttpClient client, Guid cartaoId, string descricao, decimal valorTotal, DateOnly dataCompra, int quantidadeParcelas)
    {
        var response = await client.PostAsJsonAsync($"/api/cartoes/{cartaoId}/compras", new
        {
            descricao,
            valorTotal,
            dataCompra,
            quantidadeParcelas
        });

        response.EnsureSuccessStatusCode();
    }

    private sealed record AuthResposta(string AccessToken);
    private sealed record ItemCriado(Guid Id);
    private sealed record CompraResposta(string Descricao);
    private sealed record CartaoResposta(decimal LimiteUtilizado);
}
