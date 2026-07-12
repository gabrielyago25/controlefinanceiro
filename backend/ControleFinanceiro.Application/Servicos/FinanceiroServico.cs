using ControleFinanceiro.Application.Abstracoes;
using ControleFinanceiro.Application.Dtos;
using ControleFinanceiro.Application.Excecoes;
using ControleFinanceiro.Domain.Despesas;
using ControleFinanceiro.Domain.Receitas;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Application.Servicos;

public sealed class FinanceiroServico(IControleFinanceiroDbContext db, PerfisServico perfisServico)
{
    public async Task<IReadOnlyList<CategoriaDespesaResponse>> ListarCategoriasAsync(Guid perfilId, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        return await db.CategoriasDespesa.AsNoTracking()
            .Where(categoria => categoria.PerfilId == perfilId)
            .OrderBy(categoria => categoria.Nome)
            .Select(categoria => new CategoriaDespesaResponse(categoria.Id, categoria.Nome, categoria.Ativo))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoriaDespesaResponse> CriarCategoriaAsync(Guid perfilId, SalvarCategoriaDespesaRequest request, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        var categoria = new CategoriaDespesa(request.Nome, perfilId);
        db.CategoriasDespesa.Add(categoria);
        await db.SaveChangesAsync(cancellationToken);
        return new CategoriaDespesaResponse(categoria.Id, categoria.Nome, categoria.Ativo);
    }

    public async Task<CategoriaDespesaResponse> AlterarCategoriaAsync(Guid perfilId, Guid id, SalvarCategoriaDespesaRequest request, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        var categoria = await db.CategoriasDespesa.FirstOrDefaultAsync(c => c.Id == id && c.PerfilId == perfilId, cancellationToken)
            ?? throw new NaoEncontradoException();
        categoria.AlterarNome(request.Nome);
        await db.SaveChangesAsync(cancellationToken);
        return new CategoriaDespesaResponse(categoria.Id, categoria.Nome, categoria.Ativo);
    }

    public async Task DefinirCategoriaAtivaAsync(Guid perfilId, Guid id, bool ativo, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        var categoria = await db.CategoriasDespesa.FirstOrDefaultAsync(c => c.Id == id && c.PerfilId == perfilId, cancellationToken)
            ?? throw new NaoEncontradoException();
        if (ativo) categoria.Ativar(); else categoria.Desativar();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DespesaResponse>> ListarDespesasAsync(Guid perfilId, int mes, int ano, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        var competencia = ObterCompetencia(mes, ano);
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        return await db.Despesas.AsNoTracking()
            .Where(despesa => despesa.PerfilId == perfilId && despesa.Competencia == competencia)
            .OrderBy(despesa => despesa.DataVencimento)
            .Select(despesa => new DespesaResponse(despesa.Id, despesa.Descricao, despesa.Valor, despesa.DataVencimento, despesa.DataPagamento, despesa.Competencia, despesa.Status.ToString(), despesa.Status == StatusDespesa.Pendente && despesa.DataVencimento < hoje, despesa.Observacoes, despesa.CategoriaDespesaId))
            .ToListAsync(cancellationToken);
    }

    public async Task<DespesaResponse> CriarDespesaAsync(Guid perfilId, SalvarDespesaRequest request, CancellationToken cancellationToken)
    {
        await GarantirCategoriaDoPerfilAsync(perfilId, request.CategoriaDespesaId, cancellationToken);
        var despesa = new Despesa(request.Descricao, request.Valor, request.DataVencimento, ObterCompetencia(request.Mes, request.Ano), request.CategoriaDespesaId, perfilId, request.Observacoes);
        db.Despesas.Add(despesa);
        await db.SaveChangesAsync(cancellationToken);
        return MapearDespesa(despesa);
    }

    public async Task<DespesaResponse> AlterarDespesaAsync(Guid perfilId, Guid id, SalvarDespesaRequest request, CancellationToken cancellationToken)
    {
        await GarantirCategoriaDoPerfilAsync(perfilId, request.CategoriaDespesaId, cancellationToken);
        var despesa = await db.Despesas.FirstOrDefaultAsync(d => d.Id == id && d.PerfilId == perfilId, cancellationToken)
            ?? throw new NaoEncontradoException();
        despesa.Alterar(request.Descricao, request.Valor, request.DataVencimento, ObterCompetencia(request.Mes, request.Ano), request.CategoriaDespesaId, request.Observacoes);
        await db.SaveChangesAsync(cancellationToken);
        return MapearDespesa(despesa);
    }

    public async Task PagarDespesaAsync(Guid perfilId, Guid id, PagarDespesaRequest request, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        var despesa = await db.Despesas.FirstOrDefaultAsync(d => d.Id == id && d.PerfilId == perfilId, cancellationToken)
            ?? throw new NaoEncontradoException();
        despesa.Pagar(request.DataPagamento ?? DateOnly.FromDateTime(DateTime.UtcNow));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReabrirDespesaAsync(Guid perfilId, Guid id, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        var despesa = await db.Despesas.FirstOrDefaultAsync(d => d.Id == id && d.PerfilId == perfilId, cancellationToken)
            ?? throw new NaoEncontradoException();
        despesa.Reabrir();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReceitaResponse>> ListarReceitasAsync(Guid perfilId, int mes, int ano, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        var competencia = ObterCompetencia(mes, ano);
        return await db.Receitas.AsNoTracking()
            .Where(receita => receita.PerfilId == perfilId && receita.Competencia == competencia)
            .OrderBy(receita => receita.DataRecebimento)
            .Select(receita => new ReceitaResponse(receita.Id, receita.Descricao, receita.Valor, receita.DataRecebimento, receita.Competencia, receita.Observacoes))
            .ToListAsync(cancellationToken);
    }

    public async Task<ReceitaResponse> CriarReceitaAsync(Guid perfilId, SalvarReceitaRequest request, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        var receita = new Receita(request.Descricao, request.Valor, request.DataRecebimento, ObterCompetencia(request.Mes, request.Ano), perfilId, request.Observacoes);
        db.Receitas.Add(receita);
        await db.SaveChangesAsync(cancellationToken);
        return MapearReceita(receita);
    }

    public async Task<ReceitaResponse> AlterarReceitaAsync(Guid perfilId, Guid id, SalvarReceitaRequest request, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        var receita = await db.Receitas.FirstOrDefaultAsync(r => r.Id == id && r.PerfilId == perfilId, cancellationToken)
            ?? throw new NaoEncontradoException();
        receita.Alterar(request.Descricao, request.Valor, request.DataRecebimento, ObterCompetencia(request.Mes, request.Ano), request.Observacoes);
        await db.SaveChangesAsync(cancellationToken);
        return MapearReceita(receita);
    }

    public async Task ExcluirReceitaAsync(Guid perfilId, Guid id, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        var receita = await db.Receitas.FirstOrDefaultAsync(r => r.Id == id && r.PerfilId == perfilId, cancellationToken)
            ?? throw new NaoEncontradoException();
        db.Receitas.Remove(receita);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<DashboardResponse> ObterDashboardAsync(Guid perfilId, int mes, int ano, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        var competencia = ObterCompetencia(mes, ano);
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        var receitas = await db.Receitas.AsNoTracking().Where(r => r.PerfilId == perfilId && r.Competencia == competencia).ToListAsync(cancellationToken);
        var despesas = await db.Despesas.AsNoTracking().Where(d => d.PerfilId == perfilId && d.Competencia == competencia).ToListAsync(cancellationToken);
        var faturas = await db.FaturasCartao.AsNoTracking().Where(f => f.PerfilId == perfilId && f.MesReferencia == competencia).ToListAsync(cancellationToken);
        var faturaIds = faturas.Select(f => f.Id).ToArray();
        var parcelas = await db.ParcelasCartao.AsNoTracking().Where(p => faturaIds.Contains(p.FaturaCartaoId)).ToListAsync(cancellationToken);

        var distribuicao = await db.Despesas.AsNoTracking()
            .Where(d => d.PerfilId == perfilId && d.Competencia == competencia)
            .Join(db.CategoriasDespesa.AsNoTracking(), despesa => despesa.CategoriaDespesaId, categoria => categoria.Id, (despesa, categoria) => new { despesa.Valor, categoria.Nome })
            .GroupBy(item => item.Nome)
            .Select(grupo => new CategoriaResumoResponse(grupo.Key, grupo.Sum(item => item.Valor)))
            .ToListAsync(cancellationToken);

        var inicioEvolucao = competencia.AddMonths(-5);
        var receitasEvolucao = await db.Receitas.AsNoTracking().Where(r => r.PerfilId == perfilId && r.Competencia >= inicioEvolucao && r.Competencia <= competencia).ToListAsync(cancellationToken);
        var despesasEvolucao = await db.Despesas.AsNoTracking().Where(d => d.PerfilId == perfilId && d.Competencia >= inicioEvolucao && d.Competencia <= competencia).ToListAsync(cancellationToken);
        var evolucao = Enumerable.Range(0, 6)
            .Select(i => inicioEvolucao.AddMonths(i))
            .Select(c => new EvolucaoMensalResponse(c, receitasEvolucao.Where(r => r.Competencia == c).Sum(r => r.Valor), despesasEvolucao.Where(d => d.Competencia == c).Sum(d => d.Valor)))
            .ToList();

        var totalReceitas = receitas.Sum(r => r.Valor);
        var totalDespesas = despesas.Sum(d => d.Valor);
        var valorFaturas = parcelas.Sum(p => p.Valor);

        return new DashboardResponse(
            totalReceitas,
            totalDespesas,
            totalReceitas - totalDespesas - valorFaturas,
            despesas.Where(d => d.Status == StatusDespesa.Paga).Sum(d => d.Valor),
            despesas.Where(d => d.Status == StatusDespesa.Pendente && d.DataVencimento >= hoje).Sum(d => d.Valor),
            despesas.Where(d => d.Status == StatusDespesa.Pendente && d.DataVencimento < hoje).Sum(d => d.Valor),
            valorFaturas,
            despesas.Where(d => d.Status == StatusDespesa.Pendente).OrderBy(d => d.DataVencimento).Take(5).Select(d => new VencimentoResponse(d.Id, d.Descricao, d.DataVencimento, d.Valor)).ToList(),
            distribuicao,
            evolucao);
    }

    private async Task GarantirCategoriaDoPerfilAsync(Guid perfilId, Guid categoriaId, CancellationToken cancellationToken)
    {
        await perfisServico.ValidarPerfilDoUsuarioAsync(perfilId, cancellationToken);
        var categoriaExiste = await db.CategoriasDespesa.AsNoTracking().AnyAsync(c => c.Id == categoriaId && c.PerfilId == perfilId && c.Ativo, cancellationToken);
        if (!categoriaExiste)
        {
            throw new NaoEncontradoException();
        }
    }

    private static DateOnly ObterCompetencia(int mes, int ano)
    {
        if (mes is < 1 or > 12) throw new ValidacaoException("Mês inválido.");
        if (ano < 1900) throw new ValidacaoException("Ano inválido.");
        return new DateOnly(ano, mes, 1);
    }

    private static DespesaResponse MapearDespesa(Despesa despesa)
        => new(despesa.Id, despesa.Descricao, despesa.Valor, despesa.DataVencimento, despesa.DataPagamento, despesa.Competencia, despesa.Status.ToString(), despesa.EstaAtrasada(DateOnly.FromDateTime(DateTime.UtcNow)), despesa.Observacoes, despesa.CategoriaDespesaId);

    private static ReceitaResponse MapearReceita(Receita receita)
        => new(receita.Id, receita.Descricao, receita.Valor, receita.DataRecebimento, receita.Competencia, receita.Observacoes);
}
