namespace ControleFinanceiro.Application.DTOs;

public sealed record PerfilResponse(Guid Id, string Nome, string CodigoMoeda, bool Ativo, DateTime CriadoEm);
public sealed record CriarPerfilRequest(string Nome, string? CodigoMoeda);
public sealed record AlterarPerfilRequest(string Nome);
