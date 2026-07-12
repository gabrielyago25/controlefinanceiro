import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowRight, BriefcaseBusiness, Home, Plus } from "lucide-react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { api, Perfil } from "../api";
import { useAuth } from "../App";
import { useToast } from "../components/Toast";
import { EmptyState, Field, PageHeader } from "../components/ui";
import "../styles/pages/PerfilPage.css";

export function PerfilPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { setPerfil } = useAuth();
  const { showToast } = useToast();
  const { register, handleSubmit, reset, formState } = useForm<{ nome: string }>({ defaultValues: { nome: "" } });
  const perfis = useQuery({ queryKey: ["perfis"], queryFn: () => api<Perfil[]>("/api/perfis") });
  const criar = useMutation({
    mutationFn: (data: { nome: string }) => api<Perfil>("/api/perfis", { method: "POST", body: JSON.stringify({ nome: data.nome, codigoMoeda: "BRL" }) }),
    onSuccess: (perfil) => {
      reset();
      queryClient.invalidateQueries({ queryKey: ["perfis"] });
      setPerfil(perfil);
      showToast({ kind: "success", title: "Perfil criado", message: `${perfil.nome} está pronto para uso.` });
      navigate("/");
    },
    onError: (error) => showToast({ kind: "error", title: "Erro ao criar perfil", message: error.message })
  });

  return (
    <main className="selection-screen">
      <div className="selection-shell">
        <PageHeader
          title="Escolha um perfil financeiro"
          description="Cada perfil mantém despesas, receitas, cartões e configurações separados."
        />

        {perfis.data?.length ? (
          <div className="profile-grid">
            {perfis.data.map((perfil, index) => (
              <button className="profile-card" key={perfil.id} disabled={!perfil.ativo} onClick={() => { setPerfil(perfil); navigate("/"); }}>
                {index % 2 === 0 ? <Home size={22} /> : <BriefcaseBusiness size={22} />}
                <strong>{perfil.nome}</strong>
                <span>Moeda padrão: {perfil.codigoMoeda}</span>
                {!perfil.ativo && <span>Perfil inativo</span>}
                <ArrowRight size={18} />
              </button>
            ))}
          </div>
        ) : (
          <EmptyState title="Nenhum perfil cadastrado" description="Crie o primeiro perfil para separar sua vida financeira por casa, empresa ou projeto." />
        )}

        <form className="panel-form compact-form" onSubmit={handleSubmit((data) => criar.mutate(data))}>
          <Field label="Nome do novo perfil" description="Exemplos: Casa, Loja, Empresa XYZ." error={formState.errors.nome?.message}>
            <input {...register("nome", { required: "Informe um nome para o perfil." })} />
          </Field>
          <button className="primary-action" disabled={criar.isPending}>
            <Plus size={18} /> {criar.isPending ? "Criando..." : "Criar perfil"}
          </button>
        </form>
      </div>
    </main>
  );
}
