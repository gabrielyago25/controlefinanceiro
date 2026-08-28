import { useQuery } from "@tanstack/react-query";
import { Check, ChevronRight, CircleAlert, Clock3, CreditCard, Info, PieChart as PieChartIcon, TrendingDown, TrendingUp, Wallet } from "lucide-react";
import { Link } from "react-router-dom";
import { CartesianGrid, Cell, Line, LineChart, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { api } from "../api";
import { usePeriodo } from "../App";
import { money, PageHeader } from "../components/ui";
import { mesNome } from "./periodo";
import "../styles/pages/DashboardPage.css";

type Dashboard = {
  totalReceitas: number;
  totalDespesas: number;
  saldoMensal: number;
  totalContasPagas: number;
  totalContasPendentes: number;
  totalContasAtrasadas: number;
  valorFaturasCartoes: number;
  distribuicaoDespesasPorCategoria: { categoria: string; total: number }[];
  evolucaoFinanceiraMensal: { competencia: string; receitas: number; despesas: number }[];
};

type Categoria = { id: string; nome: string; ativo: boolean };
const CORES_CATEGORIAS = ["#1239c5", "#00a878", "#ff9500", "#6930c3", "#6f7d9d", "#ef476f"];

export function DashboardPage() {
  const { periodo } = usePeriodo();
  const dashboard = useQuery({
    queryKey: ["dashboard", periodo],
    queryFn: () => api<Dashboard>(`/api/dashboard?mes=${periodo.mes}&ano=${periodo.ano}`)
  });
  const categorias = useQuery({ queryKey: ["categorias"], queryFn: () => api<Categoria[]>("/api/categorias-despesa") });

  const data = dashboard.data;
  const evolucao = data?.evolucaoFinanceiraMensal?.map(item => ({
    ...item,
    mes: new Intl.DateTimeFormat("pt-BR", { month: "short", year: "2-digit" }).format(new Date(`${item.competencia}T00:00:00`)).replace(" de ", "/")
  })) ?? [];
  const mesAnterior = evolucao.at(-2);
  const distribuicao = (categorias.data?.filter(item => item.ativo) ?? []).map((categoria, indice) => ({
    categoria: categoria.nome,
    total: data?.distribuicaoDespesasPorCategoria.find(item => item.categoria === categoria.nome)?.total ?? 0,
    cor: CORES_CATEGORIAS[indice % CORES_CATEGORIAS.length]
  }));
  const distribuicaoComValor = distribuicao.filter(item => item.total > 0);
  const referenciaAnterior = mesNome(periodo.mes === 1 ? 12 : periodo.mes - 1, periodo.mes === 1 ? periodo.ano - 1 : periodo.ano);

  return (
    <section className="page-stack dashboard-page">
      <PageHeader title="Resumo financeiro" description="Dashboard completo de indicadores financeiros." />

      <div className="metrics">
        <Metric icon={<TrendingUp size={22} />} label="Receitas do mês" value={data?.totalReceitas} description="Total de entradas" previous={mesAnterior?.receitas} reference={referenciaAnterior} tone="income" />
        <Metric icon={<TrendingDown size={22} />} label="Despesas do mês" value={data?.totalDespesas} description="Total de saídas" previous={mesAnterior?.despesas} reference={referenciaAnterior} tone="expense" />
        <Metric icon={<CreditCard size={22} />} label="Faturas de cartões" value={data?.valorFaturasCartoes} description="Total de faturas" reference={referenciaAnterior} tone="card" />
        <Metric icon={<Wallet size={22} />} label="Saldo mensal" value={data?.saldoMensal} description="Receitas - Despesas" reference={referenciaAnterior} tone="balance" />
      </div>

      <div className="dashboard-layout">
        <div className="dashboard-main-column">
          <article className="surface chart-panel overview-panel">
            <div className="panel-title">
              <div><h2>Visão geral</h2><span>Acompanhe a evolução das receitas e despesas nos últimos meses.</span></div>
              <span className="panel-select">Últimos 6 meses</span>
            </div>
            <div className="chart-legend"><span><i className="dot dot-income" />Receitas</span><span><i className="dot dot-expense" />Despesas</span></div>
            <ResponsiveContainer width="100%" height={235}>
              <LineChart data={evolucao} margin={{ top: 12, right: 12, left: 0, bottom: 0 }}>
                <CartesianGrid vertical={false} strokeDasharray="4 4" stroke="#dce3f1" />
                <XAxis dataKey="mes" tickLine={false} axisLine={false} tick={{ fill: "#6d7892", fontSize: 11 }} />
                <YAxis tickLine={false} axisLine={false} width={58} tick={{ fill: "#6d7892", fontSize: 11 }} tickFormatter={valor => `R$ ${Number(valor) / 1000}k`} />
                <Tooltip formatter={(value) => money(Number(value))} labelStyle={{ color: "#07163d", fontWeight: 700 }} />
                <Line type="monotone" dataKey="receitas" name="Receitas" stroke="#1239c5" strokeWidth={2.5} dot={{ r: 3, fill: "#fff", strokeWidth: 2 }} activeDot={{ r: 5 }} />
                <Line type="monotone" dataKey="despesas" name="Despesas" stroke="#ef476f" strokeWidth={2.5} dot={{ r: 3, fill: "#fff", strokeWidth: 2 }} activeDot={{ r: 5 }} />
              </LineChart>
            </ResponsiveContainer>
          </article>

          <article className="surface category-panel">
            <div className="panel-title">
              <div><h2>Despesas por categoria</h2><span>Valores consolidados na competência selecionada.</span></div>
              <span className="panel-select">Exibir: Valor</span>
            </div>
            <div className="category-content">
              <div className="category-chart">
                {distribuicaoComValor.length ? (
                  <ResponsiveContainer width="100%" height={160}>
                    <PieChart><Pie data={distribuicaoComValor} dataKey="total" nameKey="categoria" innerRadius={45} outerRadius={68} paddingAngle={2}>{distribuicaoComValor.map(item => <Cell key={item.categoria} fill={item.cor} />)}</Pie><Tooltip formatter={(value) => money(Number(value))} /></PieChart>
                  </ResponsiveContainer>
                ) : (
                  <div className="category-empty"><PieChartIcon size={28} /><strong>Sem despesas no período</strong><span>Quando houver contas lançadas,<br />o gráfico aparecerá aqui.</span></div>
                )}
              </div>
              <div className="category-summary">
                {distribuicao.length ? distribuicao.slice(0, 6).map(item => <div key={item.categoria}><span><i style={{ background: item.cor }} />{item.categoria}</span><strong>{money(item.total)}</strong></div>) : <span className="category-no-data">Cadastre categorias para acompanhar a distribuição.</span>}
                <Link to="/configuracoes">Ver todas as categorias <ChevronRight size={16} /></Link>
              </div>
            </div>
          </article>
        </div>

        <article className="surface status-panel">
          <div className="panel-title"><div><h2>Situação das contas</h2><span>Separação entre pagas, pendentes e atrasadas.</span></div></div>
          <div className="status-list">
            <StatusLine icon={<Check size={22} />} label="Pagas" description="Contas quitadas em dia" value={data?.totalContasPagas} tone="success" />
            <StatusLine icon={<Clock3 size={22} />} label="Pendentes" description="Contas a vencer" value={data?.totalContasPendentes} tone="warning" />
            <StatusLine icon={<CircleAlert size={22} />} label="Atrasadas" description="Contas vencidas" value={data?.totalContasAtrasadas} tone="danger" />
          </div>
          <Link className="status-link" to="/despesas">Ver todas as contas <ChevronRight size={17} /></Link>
        </article>
      </div>

      <div className="dashboard-note"><Info size={16} /><span>Os valores apresentados são referentes ao mês selecionado. Para visualizar detalhes, acesse as opções no menu lateral.</span></div>
    </section>
  );
}

function Metric({ icon, label, value, description, previous, reference, tone }: { icon: React.ReactNode; label: string; value?: number; description: string; previous?: number; reference: string; tone: "income" | "expense" | "balance" | "card" }) {
  const atual = value ?? 0;
  const variacao = previous ? Math.round(((atual - previous) / Math.abs(previous)) * 100) : atual ? 100 : 0;
  return (
    <article className={`metric metric-${tone}`}>
      <span className="metric-icon">{icon}</span>
      <div className="metric-body"><small>{label}</small><strong>{money(atual)}</strong><span>{description}</span></div>
      <div className="metric-comparison"><b>{variacao > 0 ? "+" : variacao < 0 ? "−" : "—"} {Math.abs(variacao)}%</b><span>vs. {reference}</span></div>
    </article>
  );
}

function StatusLine({ icon, label, description, value, tone }: { icon: React.ReactNode; label: string; description: string; value?: number; tone: "success" | "warning" | "danger" }) {
  return (
    <div className={`status-line status-line-${tone}`}>
      <span className="status-icon">{icon}</span>
      <div><strong>{label}</strong><span>{description}</span></div>
      <div className="status-value"><strong>{money(value)}</strong><span>{(value ?? 0) > 0 ? "Com lançamentos" : "Nenhuma conta"}</span></div>
      <ChevronRight size={18} />
    </div>
  );
}
