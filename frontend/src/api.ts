export type Usuario = { id: string; nome: string; email: string };
export type Perfil = { id: string; nome: string; codigoMoeda: string; ativo: boolean };
export type AuthResponse = { accessToken: string; expiraEm: string; usuario: Usuario };

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5175";

let accessToken = localStorage.getItem("controleFinanceiro.accessToken");
let refreshEmAndamento: Promise<boolean> | null = null;

export function setAccessToken(token: string | null) {
  accessToken = token;
  if (token) localStorage.setItem("controleFinanceiro.accessToken", token);
  else localStorage.removeItem("controleFinanceiro.accessToken");
}

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers);
  if (!headers.has("Content-Type") && options.body) headers.set("Content-Type", "application/json");
  if (accessToken) headers.set("Authorization", `Bearer ${accessToken}`);

  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers,
    credentials: "include"
  });

  if (response.status === 401 && path !== "/api/autenticacao/refresh") {
    const refreshed = await renovarUmaVez();
    if (refreshed) return api<T>(path, options);
  }

  if (!response.ok) {
    const problem = await response.json().catch(() => ({ title: "Falha na requisição." }));
    throw new Error(problem.title ?? "Falha na requisição.");
  }

  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

function renovarUmaVez() {
  if (!refreshEmAndamento) {
    refreshEmAndamento = refresh().finally(() => { refreshEmAndamento = null; });
  }
  return refreshEmAndamento;
}

export async function refresh() {
  try {
    const result = await api<AuthResponse>("/api/autenticacao/refresh", { method: "POST" });
    setAccessToken(result.accessToken);
    return true;
  } catch {
    setAccessToken(null);
    return false;
  }
}
