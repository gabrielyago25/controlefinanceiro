import { useQuery } from "@tanstack/react-query";
import { ChevronLeft, ChevronRight, CreditCard, Receipt, TrendingDown, TrendingUp, Wallet } from "lucide-react";
import { useState } from "react";
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { api } from "../api";
import { useAuth } from "../App";
import { EmptyState, money, PageHeader, StatusBadge } from "../components/ui";
import { competenciaAtual, mesNome } from "./periodo";
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
};

export function DashboardPage() {
  const { perfil } = useAuth();
  const [periodo, setPeriodo] = useState(competenciaAtual());
  const dashboard = useQuery({
    queryKey: ["dashboard", perfil?.id, periodo],
    queryFn: () => api<Dashboard>(`/api/perfis/${perfil!.id}/dashboard?mes=${periodo.mes}&ano=${periodo.ano}`),
    enabled: !!perfil
  });

  const data = dashboard.data;

  return (
    <section className="page-stack">
      <PageHeader
        title="Resumo financeiro"
        description="Acompanhe o resultado do mês, pendências e a distribuição das despesas."
        actions={<Periodo periodo={periodo} setPeriodo={setPeriodo} />}
      />

      <div className="metrics">
        <Metric icon={<TrendingUp size={20} />} label="Receitas do mês" value={data?.totalReceitas} tone="income" />
        <Metric icon={<TrendingDown size={20} />} label="Despesas do mês" value={data?.totalDespesas} tone="expense" />
        <Metric icon={<Wallet size={20} />} label="Saldo mensal" value={data?.saldoMensal} tone={(data?.saldoMensal ?? 0) >= 0 ? "income" : "danger" as const} />
        <Metric icon={<CreditCard size={20} />} label="Faturas de cartões" value={data?.valorFaturasCartoes} tone="card" />
      </div>

      <div className="dashboard-grid">
        <article className="surface chart-panel">
          <div className="panel-title">
            <div>
              <h2>Despesas por categoria</h2>
              <span>Valores consolidados na competência selecionada.</span>
            </div>
          </div>
          {data?.distribuicaoDespesasPorCategoria.length ? (
            <ResponsiveContainer width="100%" height={280}>
              <BarChart data={data.distribuicaoDespesasPorCategoria}>
                <CartesianGrid strokeDasharray="3 3" stroke="#dde3df" />
                <XAxis dataKey="categoria" />
                <YAxis />
                <Tooltip formatter={(value) => money(Number(value))} />
                <Bar dataKey="total" fill="#26766b" radius={[5, 5, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <EmptyState title="Sem despesas no período" description="Quando houver contas lançadas, o gráfico aparece aqui." />
          )}
        </article>

        <article className="surface">
          <div className="panel-title">
            <div>
              <h2>Situação das contas</h2>
              <span>Separação entre pagas, pendentes e atrasadas.</span>
            </div>
          </div>
          <div className="status-list">
            <StatusLine label="Pagas" value={data?.totalContasPagas} tone="success" />
            <StatusLine label="Pendentes" value={data?.totalContasPendentes} tone="warning" />
            <StatusLine label="Atrasadas" value={data?.totalContasAtrasadas} tone="danger" />
          </div>
        </article>
      </div>
    </section>
  );
}

export function Periodo({ periodo, setPeriodo }: { periodo: { mes: number; ano: number }; setPeriodo: (p: { mes: number; ano: number }) => void }) {
  const mover = (delta: number) => {
    const data = new Date(periodo.ano, periodo.mes - 1 + delta, 1);
    setPeriodo({ mes: data.getMonth() + 1, ano: data.getFullYear() });
  };

  return (
    <div className="period" aria-label="Selecionar mês">
      <button className="ghost-icon" type="button" onClick={() => mover(-1)} aria-label="Mês anterior"><ChevronLeft size={18} /></button>
      <strong>{mesNome(periodo.mes, periodo.ano)}</strong>
      <button className="ghost-icon" type="button" onClick={() => mover(1)} aria-label="Próximo mês"><ChevronRight size={18} /></button>
    </div>
  );
}

function Metric({ icon, label, value, tone }: { icon: React.ReactNode; label: string; value?: number; tone: "income" | "expense" | "card" | "danger" }) {
  return (
    <article className={`metric metric-${tone}`}>
      <span>{icon}</span>
      <div>
        <small>{label}</small>
        <strong>{money(value)}</strong>
      </div>
    </article>
  );
}

function StatusLine({ label, value, tone }: { label: string; value?: number; tone: "success" | "warning" | "danger" }) {
  return (
    <div className="status-line">
      <StatusBadge tone={tone}>{label}</StatusBadge>
      <strong>{money(value)}</strong>
      <Receipt size={18} />
    </div>
  );
}
