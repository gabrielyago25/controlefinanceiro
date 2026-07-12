import { Navigate, NavLink, Outlet, Route, Routes, useLocation, useNavigate } from "react-router-dom";
import { BarChart3, Home, LogOut, Receipt, Settings, UserRound, WalletCards } from "lucide-react";
import { createContext, lazy, Suspense, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api, AuthResponse, Perfil, setAccessToken, Usuario } from "./api";
import { ToastProvider, useToast } from "./components/Toast";
import "./styles/common/layout.css";

const AuthPage = lazy(() => import("./pages/AuthPage").then(module => ({ default: module.AuthPage })));
const DashboardPage = lazy(() => import("./pages/DashboardPage").then(module => ({ default: module.DashboardPage })));
const DespesasPage = lazy(() => import("./pages/DespesasPage").then(module => ({ default: module.DespesasPage })));
const ReceitasPage = lazy(() => import("./pages/ReceitasPage").then(module => ({ default: module.ReceitasPage })));
const ConfiguracoesPage = lazy(() => import("./pages/ConfiguracoesPage").then(module => ({ default: module.ConfiguracoesPage })));
const PerfilPage = lazy(() => import("./pages/PerfilPage").then(module => ({ default: module.PerfilPage })));

type AuthContextValue = {
  usuario: Usuario | null;
  perfil: Perfil | null;
  setPerfil: (perfil: Perfil | null) => void;
  setUsuario: (usuario: Usuario) => void;
  onAuth: (response: AuthResponse) => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);
export const useAuth = () => useContext(AuthContext)!;

export function App() {
  const [usuario, setUsuario] = useState<Usuario | null>(null);
  const [perfil, setPerfilState] = useState<Perfil | null>(null);

  const setPerfil = useCallback((novoPerfil: Perfil | null) => {
    setPerfilState(novoPerfil);
    if (novoPerfil && usuario) localStorage.setItem("controleFinanceiro.perfil", JSON.stringify({ usuarioId: usuario.id, perfilId: novoPerfil.id }));
    else localStorage.removeItem("controleFinanceiro.perfil");
  }, [usuario]);

  const value = useMemo(() => ({
    usuario,
    perfil,
    setPerfil,
    setUsuario,
    onAuth: (response: AuthResponse) => {
      setAccessToken(response.accessToken);
      if (usuario?.id && usuario.id !== response.usuario.id) setPerfil(null);
      setUsuario(response.usuario);
    }
  }), [usuario, perfil]);

  return (
    <ToastProvider>
      <AuthContext.Provider value={value}>
        <Suspense fallback={<div className="center">Carregando página...</div>}>
          <Routes>
            <Route path="/login" element={<AuthPage modo="login" />} />
            <Route path="/cadastro" element={<AuthPage modo="cadastro" />} />
            <Route path="/perfis" element={<RequireAuth><PerfilPage /></RequireAuth>} />
            <Route path="/" element={<RequireAuth><Shell /></RequireAuth>}>
              <Route index element={<RequirePerfil><DashboardPage /></RequirePerfil>} />
              <Route path="despesas" element={<RequirePerfil><DespesasPage /></RequirePerfil>} />
              <Route path="receitas" element={<RequirePerfil><ReceitasPage /></RequirePerfil>} />
              <Route path="configuracoes" element={<RequirePerfil><ConfiguracoesPage /></RequirePerfil>} />
            </Route>
          </Routes>
        </Suspense>
      </AuthContext.Provider>
    </ToastProvider>
  );
}

function RequirePerfil({ children }: { children: React.ReactNode }) {
  const { usuario, perfil, setPerfil } = useAuth();
  const [perfilValidado, setPerfilValidado] = useState(false);
  const perfis = useQuery({
    queryKey: ["perfis"],
    queryFn: () => api<Perfil[]>("/api/perfis"),
    enabled: !!usuario,
    retry: false
  });

  useEffect(() => {
    if (!usuario || !perfis.data) return;
    if (perfil?.ativo && perfis.data.some(p => p.id === perfil.id && p.ativo)) { setPerfilValidado(true); return; }
    try {
      const raw = localStorage.getItem("controleFinanceiro.perfil");
      const salvo = raw ? JSON.parse(raw) as { usuarioId?: string; perfilId?: string } : null;
      const restaurado = salvo?.usuarioId === usuario.id ? perfis.data.find(p => p.id === salvo.perfilId && p.ativo) : undefined;
      setPerfil(restaurado ?? null);
    } catch {
      setPerfil(null);
    }
    setPerfilValidado(true);
  }, [usuario, perfil, perfis.data, setPerfil]);

  if (perfis.isLoading) return <div className="center">Validando perfil...</div>;
  if (perfis.isError) return <Navigate to="/perfis" replace />;

  const perfilValido = perfil && perfil.ativo ? perfis.data?.find(p => p.id === perfil.id && p.ativo) : undefined;
  if (perfilValido) return children;

  if (!perfilValidado) return <div className="center">Restaurando perfil...</div>;
  return <Navigate to="/perfis" replace />;
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
  const { perfil, setPerfil, usuario } = useAuth();
  const { showToast } = useToast();

  const logout = useMutation({
    mutationFn: () => api<void>("/api/autenticacao/logout", { method: "POST" }),
    onSettled: () => {
      setAccessToken(null);
      setPerfil(null);
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

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <WalletCards size={25} />
          <div>
            <strong>ControleFinanceiro</strong>
            <span>Gestão por perfil</span>
          </div>
        </div>
        <nav aria-label="Navegação principal">
          <NavLink to="/" end><Home size={18} /> Resumo</NavLink>
          <NavLink to="/despesas"><Receipt size={18} /> Controle de contas</NavLink>
          <NavLink to="/receitas"><BarChart3 size={18} /> Receitas</NavLink>
          <NavLink to="/configuracoes"><Settings size={18} /> Configurações</NavLink>
        </nav>
        <div className="sidebar-user">
          <UserRound size={18} />
          <div>
            <strong>{usuario?.nome ?? "Usuário"}</strong>
            <span>{usuario?.email ?? "Sessão ativa"}</span>
          </div>
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
          <button className="profile-pill" onClick={() => navigate("/perfis")}>
            <span>Perfil</span>
            <strong>{perfil?.nome ?? "Selecionar perfil"}</strong>
          </button>
        </header>
        <Outlet />
      </main>
    </div>
  );
}
