namespace ControleFinanceiro.Domain.CreditCards;

public static class CartaoCalendario
{
    public static DateOnly ObterDataValida(int ano, int mes, int dia)
    {
        var ultimoDia = DateTime.DaysInMonth(ano, mes);
        return new DateOnly(ano, mes, Math.Min(dia, ultimoDia));
    }

    public static DateOnly ObterFechamentoDaCompra(DateOnly dataCompra, int diaFechamento)
    {
        var fechamento = ObterDataValida(dataCompra.Year, dataCompra.Month, diaFechamento);
        if (dataCompra <= fechamento)
        {
            return fechamento;
        }

        var proximoMes = dataCompra.AddMonths(1);
        return ObterDataValida(proximoMes.Year, proximoMes.Month, diaFechamento);
    }

    public static DateOnly ObterVencimento(DateOnly dataFechamento, int diaVencimento)
    {
        var vencimento = ObterDataValida(dataFechamento.Year, dataFechamento.Month, diaVencimento);
        if (vencimento <= dataFechamento)
        {
            var proximoMes = dataFechamento.AddMonths(1);
            vencimento = ObterDataValida(proximoMes.Year, proximoMes.Month, diaVencimento);
        }

        return vencimento;
    }
}
