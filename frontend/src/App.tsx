import { Navigate, NavLink, Outlet, Route, Routes, useLocation, useNavigate } from "react-router-dom";
import { BarChart3, CalendarDays, ChevronDown, ChevronLeft, ChevronRight, Home, LogOut, Receipt, Settings, WalletCards } from "lucide-react";
import { createContext, lazy, Suspense, useContext, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api, AuthResponse, setAccessToken, Usuario } from "./api";
import { ToastProvider, useToast } from "./components/Toast";
import { competenciaAtual, mesNome } from "./pages/periodo";
import "./styles/common/layout.css";

const AuthPage = lazy(() => import("./pages/AuthPage").then(module => ({ default: module.AuthPage })));
const DashboardPage = lazy(() => import("./pages/DashboardPage").then(module => ({ default: module.DashboardPage })));
const DespesasPage = lazy(() => import("./pages/DespesasPage").then(module => ({ default: module.DespesasPage })));
const ReceitasPage = lazy(() => import("./pages/ReceitasPage").then(module => ({ default: module.ReceitasPage })));
const ConfiguracoesPage = lazy(() => import("./pages/ConfiguracoesPage").then(module => ({ default: module.ConfiguracoesPage })));
type AuthContextValue = {
  usuario: Usuario | null;
  setUsuario: (usuario: Usuario) => void;
  onAuth: (response: AuthResponse) => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);
export const useAuth = () => useContext(AuthContext)!;

type Periodo = { mes: number; ano: number };
const PeriodContext = createContext<{ periodo: Periodo; setPeriodo: (periodo: Periodo) => void } | null>(null);
export const usePeriodo = () => useContext(PeriodContext)!;

export function App() {
  const [usuario, setUsuario] = useState<Usuario | null>(null);
  const [periodo, setPeriodo] = useState(competenciaAtual());

  const value = useMemo(() => ({
    usuario,
    setUsuario,
    onAuth: (response: AuthResponse) => {
      setAccessToken(response.accessToken);
      setUsuario(response.usuario);
    }
  }), [usuario]);

  return (
    <ToastProvider>
      <AuthContext.Provider value={value}>
        <PeriodContext.Provider value={{ periodo, setPeriodo }}>
          <Suspense fallback={<div className="center">Carregando página...</div>}>
            <Routes>
              <Route path="/login" element={<AuthPage modo="login" />} />
              <Route path="/cadastro" element={<AuthPage modo="cadastro" />} />
              <Route path="/" element={<RequireAuth><Shell /></RequireAuth>}>
                <Route index element={<DashboardPage />} />
                <Route path="despesas" element={<DespesasPage />} />
                <Route path="receitas" element={<ReceitasPage />} />
                <Route path="configuracoes" element={<ConfiguracoesPage />} />
                <Route path="perfis" element={<Navigate to="/" replace />} />
              </Route>
            </Routes>
          </Suspense>
        </PeriodContext.Provider>
      </AuthContext.Provider>
    </ToastProvider>
  );
}

function RequireAuth({ children }: { children: React.ReactNode }) {
  const { usuario, onAuth } = useAuth();
  const me = useQuery({
    queryKey: ["me"],
    queryFn: async () => {
      const user = await api<Usuario>("/api/autenticacao/me");
      onAuth({ accessToken: localStorage.getItem("controleFinanceiro.accessToken") ?? "", expiraEm: "", usuario: user });
      return user;
    },
    enabled: !usuario,
    retry: false
  });

  if (usuario) return children;
  if (me.isLoading) return <div className="center">Carregando...</div>;
  if (me.isError) return <Navigate to="/login" replace />;
  return children;
}

function Shell() {
  const navigate = useNavigate();
  const location = useLocation();
  const queryClient = useQueryClient();
  const { usuario } = useAuth();
  const { showToast } = useToast();

  const logout = useMutation({
    mutationFn: () => api<void>("/api/autenticacao/logout", { method: "POST" }),
    onSettled: () => {
      setAccessToken(null);
      queryClient.clear();
      showToast({ kind: "info", title: "Sessão encerrada" });
      navigate("/login");
    }
  });

  const pageTitle = location.pathname === "/"
    ? "Resumo financeiro"
    : location.pathname.includes("despesas")
      ? "Controle de contas"
      : location.pathname.includes("receitas")
        ? "Receitas"
        : "Configurações";
  const iniciais = usuario?.nome
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map(parte => parte[0]?.toUpperCase())
    .join("") || "U";

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <span className="brand-mark"><WalletCards size={22} /></span>
          <div>
            <strong>ControleFinanceiro</strong>
            <span>Gestão inteligente</span>
          </div>
        </div>
        <nav aria-label="Navegação principal">
          <NavLink to="/" end><Home size={18} /> Resumo</NavLink>
          <NavLink to="/despesas"><Receipt size={18} /> Controle de contas</NavLink>
          <NavLink to="/receitas"><BarChart3 size={18} /> Receitas</NavLink>
          <NavLink to="/configuracoes"><Settings size={18} /> Configurações</NavLink>
        </nav>
        <div className="sidebar-user">
          <span className="user-avatar">{iniciais}</span>
          <div>
            <strong>{usuario?.nome ?? "Usuário"}</strong>
            <span>{usuario?.email ?? "Sessão ativa"}</span>
          </div>
          <ChevronDown size={15} />
        </div>
        <button className="icon-text" onClick={() => logout.mutate()} disabled={logout.isPending}>
          <LogOut size={18} /> Sair
        </button>
      </aside>
      <main className="content">
        <header className="topbar">
          <div>
            <span>Área atual</span>
            <strong>{pageTitle}</strong>
          </div>
          <div className="topbar-actions">
            <PeriodControl />
            <span className="topbar-divider" />
            <div className="topbar-account" aria-label={`Usuário: ${usuario?.nome ?? "Usuário"}`}>
              <span className="user-avatar user-avatar-light">{iniciais}</span>
              <ChevronDown size={15} />
            </div>
          </div>
        </header>
        <Outlet />
      </main>
    </div>
  );
}

function PeriodControl() {
  const { periodo, setPeriodo } = usePeriodo();
  const mover = (delta: number) => {
    const data = new Date(periodo.ano, periodo.mes - 1 + delta, 1);
    setPeriodo({ mes: data.getMonth() + 1, ano: data.getFullYear() });
  };

  return (
    <div className="period period-topbar" aria-label="Selecionar mês">
      <div className="period-label"><CalendarDays size={17} /><strong>{mesNome(periodo.mes, periodo.ano)}</strong><ChevronDown size={14} /></div>
      <button className="period-arrow" type="button" onClick={() => mover(-1)} aria-label="Mês anterior"><ChevronLeft size={17} /></button>
      <button className="period-arrow" type="button" onClick={() => mover(1)} aria-label="Próximo mês"><ChevronRight size={17} /></button>
    </div>
  );
}
