import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { ArrowRight, LockKeyhole, Mail, UserRound } from "lucide-react";
import { useForm } from "react-hook-form";
import { Link, useNavigate } from "react-router-dom";
import { z } from "zod";
import { api, AuthResponse } from "../api";
import { useAuth } from "../App";
import { useToast } from "../components/Toast";
import { Field } from "../components/ui";
import "../styles/pages/AuthPage.css";

const schema = z.object({
  nome: z.string().optional(),
  email: z.string().email("Informe um e-mail válido."),
  senha: z.string().min(8, "A senha precisa ter pelo menos 8 caracteres.")
});

type FormData = z.infer<typeof schema>;

export function AuthPage({ modo }: { modo: "login" | "cadastro" }) {
  const navigate = useNavigate();
  const { onAuth } = useAuth();
  const { showToast } = useToast();
  const form = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { email: "", senha: "", nome: "" }
  });

  const mutation = useMutation({
    mutationFn: (data: FormData) => api<AuthResponse>(`/api/autenticacao/${modo}`, { method: "POST", body: JSON.stringify(data) }),
    onSuccess: (response) => {
      onAuth(response);
      showToast({ kind: "success", title: modo === "login" ? "Login realizado" : "Conta criada", message: "Você já pode organizar suas finanças." });
      navigate("/");
    },
    onError: (error) => {
      showToast({ kind: "error", title: "Não foi possível continuar", message: error.message });
    }
  });

  return (
    <main className="auth-screen">
      <section className="auth-hero">
        <div className="auth-mark"><LockKeyhole size={26} /></div>
        <h1>{modo === "login" ? "Bem-vindo de volta" : "Comece seu controle financeiro"}</h1>
        <p>Organize contas, receitas e cartões em um só lugar, com acesso simples e seguro.</p>
      </section>

      <section className="auth-panel">
        <div className="auth-title">
          <span>{modo === "login" ? "Acessar conta" : "Novo usuário"}</span>
          <h2>{modo === "login" ? "Entrar" : "Criar conta"}</h2>
        </div>

        <form onSubmit={form.handleSubmit((data) => mutation.mutate(data))}>
          {modo === "cadastro" && (
            <Field label="Nome completo" description="Como você quer ser identificado dentro do sistema." error={form.formState.errors.nome?.message}>
              <div className="input-with-icon"><UserRound size={18} /><input autoComplete="name" {...form.register("nome")} /></div>
            </Field>
          )}

          <Field label="E-mail" description="Usado para login e identificação da conta." error={form.formState.errors.email?.message}>
            <div className="input-with-icon"><Mail size={18} /><input type="email" autoComplete="email" {...form.register("email")} /></div>
          </Field>

          <Field label="Senha" description="Mínimo de 8 caracteres. No cadastro, use letras maiúsculas, minúsculas e números." error={form.formState.errors.senha?.message}>
            <div className="input-with-icon"><LockKeyhole size={18} /><input type="password" autoComplete={modo === "login" ? "current-password" : "new-password"} {...form.register("senha")} /></div>
          </Field>

          <button className={`primary-action ${modo === "cadastro" ? "action-create" : ""}`} disabled={mutation.isPending}>
            {mutation.isPending ? "Processando..." : modo === "login" ? "Entrar" : "Cadastrar"} <ArrowRight size={18} />
          </button>
        </form>

        {modo === "login" && (
          <aside className="test-credentials" aria-label="Credenciais de acesso para teste">
            <strong>Credenciais de acesso</strong>
            <span>E-mail: <code>gabriel@teste2.com</code></span>
            <span>Senha: <code>Teste@123</code></span>
          </aside>
        )}

        <Link className="auth-link" to={modo === "login" ? "/cadastro" : "/login"}>
          {modo === "login" ? "Criar uma nova conta" : "Já tenho uma conta"}
        </Link>
      </section>
    </main>
  );
}
