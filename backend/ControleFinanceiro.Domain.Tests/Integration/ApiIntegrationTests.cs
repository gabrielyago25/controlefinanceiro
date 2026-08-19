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

        var response = await client.GetAsync("/api/perfis");

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
    public async Task Usuario_nao_deve_editar_perfil_de_outro_usuario()
    {
        await using var factory = new ApiFactory();
        var usuario1 = factory.CreateClient();
        var usuario2 = factory.CreateClient();

        var token1 = await CadastrarAsync(usuario1, "ana@exemplo.com");
        usuario1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var perfilResponse = await usuario1.PostAsJsonAsync("/api/perfis", new { nome = "Casa", codigoMoeda = "BRL" });
        var perfil = await perfilResponse.Content.ReadFromJsonAsync<PerfilCriado>();

        var token2 = await CadastrarAsync(usuario2, "bruno@exemplo.com");
        usuario2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var tentativa = await usuario2.PutAsJsonAsync($"/api/perfis/{perfil!.Id}", new { nome = "Perfil invadido" });

        Assert.Equal(HttpStatusCode.Created, perfilResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, tentativa.StatusCode);
    }

    [Fact]
    public async Task Usuario_nao_deve_acessar_modulos_de_perfil_de_outro_usuario()
    {
        await using var factory = new ApiFactory();
        var proprietario = factory.CreateClient();
        var outroUsuario = factory.CreateClient();

        var tokenProprietario = await CadastrarAsync(proprietario, "proprietario@exemplo.com");
        proprietario.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenProprietario);
        var perfilResponse = await proprietario.PostAsJsonAsync("/api/perfis", new { nome = "Privado", codigoMoeda = "BRL" });
        var perfil = await perfilResponse.Content.ReadFromJsonAsync<PerfilCriado>();

        var tokenOutroUsuario = await CadastrarAsync(outroUsuario, "visitante@exemplo.com");
        outroUsuario.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenOutroUsuario);

        var caminhos = new[]
        {
            $"/api/perfis/{perfil!.Id}/categorias-despesa",
            $"/api/perfis/{perfil.Id}/despesas?mes=7&ano=2026",
            $"/api/perfis/{perfil.Id}/receitas?mes=7&ano=2026",
            $"/api/perfis/{perfil.Id}/cartoes",
            $"/api/perfis/{perfil.Id}/dashboard?mes=7&ano=2026"
        };

        foreach (var caminho in caminhos)
        {
            var resposta = await outroUsuario.GetAsync(caminho);
            Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        }
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

    private sealed record AuthResposta(string AccessToken);
    private sealed record PerfilCriado(Guid Id);
}
