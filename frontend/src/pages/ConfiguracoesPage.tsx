import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { CreditCard, FolderCog, Plus, Tags, UserRound } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { api, Perfil, Usuario } from "../api";
import { useAuth } from "../App";
import { useToast } from "../components/Toast";
import { EmptyState, Field, MoneyInput, PageHeader, parseMoney, StatusBadge } from "../components/ui";
import "../styles/pages/ConfiguracoesPage.css";

type Categoria = { id: string; nome: string; ativo: boolean };
type Cartao = { id: string; nome: string; banco: string; bandeira: string; limite: number; diaFechamento: number; diaVencimento: number; ativo: boolean; cor?: string };

export function ConfiguracoesPage() {
  const { perfil, usuario, setPerfil, setUsuario } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const categoriaForm = useForm<{ nome: string }>({ defaultValues: { nome: "" } });
  const usuarioForm = useForm<{ nome: string; email: string }>({ defaultValues: { nome: usuario?.nome ?? "", email: usuario?.email ?? "" } });
  const senhaForm = useForm<{ senhaAtual: string; novaSenha: string }>({ defaultValues: { senhaAtual: "", novaSenha: "" } });
  const mostrarErro = (error: Error) => showToast({ kind: "error", title: "Não foi possível salvar", message: error.message });

  useEffect(() => usuarioForm.reset({ nome: usuario?.nome ?? "", email: usuario?.email ?? "" }), [usuario]);

  const categorias = useQuery({ queryKey: ["categorias", perfil!.id], queryFn: () => api<Categoria[]>(`/api/perfis/${perfil!.id}/categorias-despesa`) });
  const perfis = useQuery({ queryKey: ["perfis"], queryFn: () => api<Perfil[]>("/api/perfis") });
  const cartoes = useQuery({ queryKey: ["cartoes", perfil!.id], queryFn: () => api<Cartao[]>(`/api/perfis/${perfil!.id}/cartoes`) });

  const criarCategoria = useMutation({
    mutationFn: (data: { nome: string }) => api(`/api/perfis/${perfil!.id}/categorias-despesa`, { method: "POST", body: JSON.stringify(data) }),
    onSuccess: () => { categoriaForm.reset(); queryClient.invalidateQueries({ queryKey: ["categorias"] }); showToast({ kind: "success", title: "Categoria criada" }); },
    onError: (error) => showToast({ kind: "error", title: "Erro ao criar categoria", message: error.message })
  });
  const alterarCategoria = useMutation({
    mutationFn: ({ categoria, nome }: { categoria: Categoria; nome: string }) => api<Categoria>(`/api/perfis/${perfil!.id}/categorias-despesa/${categoria.id}`, { method: "PUT", body: JSON.stringify({ nome }) }),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ["categorias"] }); showToast({ kind: "success", title: "Categoria atualizada" }); }, onError: mostrarErro
  });
  const statusCategoria = useMutation({
    mutationFn: (categoria: Categoria) => api(`/api/perfis/${perfil!.id}/categorias-despesa/${categoria.id}/${categoria.ativo ? "desativar" : "ativar"}`, { method: "PATCH" }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["categorias"] }), onError: mostrarErro
  });
  const alterarPerfil = useMutation({
    mutationFn: ({ item, nome }: { item: Perfil; nome: string }) => api<Perfil>(`/api/perfis/${item.id}`, { method: "PUT", body: JSON.stringify({ nome }) }),
    onSuccess: (atualizado) => { if (perfil!.id === atualizado.id) setPerfil(atualizado); queryClient.invalidateQueries({ queryKey: ["perfis"] }); showToast({ kind: "success", title: "Perfil atualizado" }); }, onError: mostrarErro
  });
  const statusPerfil = useMutation({
    mutationFn: (item: Perfil) => api(`/api/perfis/${item.id}/${item.ativo ? "desativar" : "ativar"}`, { method: "PATCH" }),
    onSuccess: (_, item) => { queryClient.invalidateQueries({ queryKey: ["perfis"] }); if (item.id === perfil!.id && item.ativo) setPerfil(null); }, onError: mostrarErro
  });
  const alterarCartao = useMutation({
    mutationFn: (item: Cartao) => api<Cartao>(`/api/perfis/${perfil!.id}/cartoes/${item.id}`, { method: "PUT", body: JSON.stringify(item) }),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ["cartoes"] }); showToast({ kind: "success", title: "Cartão atualizado" }); }, onError: mostrarErro
  });
  const statusCartao = useMutation({
    mutationFn: (item: Cartao) => api(`/api/perfis/${perfil!.id}/cartoes/${item.id}/${item.ativo ? "desativar" : "ativar"}`, { method: "PATCH" }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["cartoes"] }), onError: mostrarErro
  });
  const alterarUsuario = useMutation({
    mutationFn: (data: { nome: string; email: string }) => api<Usuario>("/api/autenticacao/me", { method: "PUT", body: JSON.stringify(data) }),
    onSuccess: (atualizado) => { setUsuario(atualizado); showToast({ kind: "success", title: "Dados pessoais atualizados" }); },
    onError: (error) => showToast({ kind: "error", title: "Erro ao atualizar usuário", message: error.message })
  });
  const alterarSenha = useMutation({
    mutationFn: (data: { senhaAtual: string; novaSenha: string }) => api("/api/autenticacao/me/senha", { method: "PUT", body: JSON.stringify(data) }),
    onSuccess: () => { senhaForm.reset(); showToast({ kind: "success", title: "Senha atualizada" }); },
    onError: (error) => showToast({ kind: "error", title: "Erro ao atualizar senha", message: error.message })
  });

  return <section className="page-stack settings-stack">
    <PageHeader title="Configurações" description="Edite categorias, perfis, cartões e os dados da sua conta." />

    <SettingsSection icon={<Tags size={20} />} title="Categorias de despesa">
      <form className="panel-form compact-form" onSubmit={categoriaForm.handleSubmit(data => criarCategoria.mutate(data))}>
        <Field label="Nova categoria" error={categoriaForm.formState.errors.nome?.message}><input {...categoriaForm.register("nome", { required: "Informe o nome." })} /></Field>
        <button className="primary-action"><Plus size={18} /> Criar</button>
      </form>
      {categorias.data?.length ? categorias.data.map(item => <EditableNameRow key={item.id} name={item.nome} active={item.ativo} label="categoria" onSave={nome => alterarCategoria.mutate({ categoria: item, nome })} onToggle={() => statusCategoria.mutate(item)} />) : <EmptyState title="Nenhuma categoria" description="Crie a primeira categoria acima." />}
    </SettingsSection>

    <SettingsSection icon={<FolderCog size={20} />} title="Perfis financeiros">
      {perfis.data?.map(item => <EditableNameRow key={item.id} name={item.nome} active={item.ativo} label="perfil" onSave={nome => alterarPerfil.mutate({ item, nome })} onToggle={() => statusPerfil.mutate(item)} />)}
    </SettingsSection>

    <SettingsSection icon={<CreditCard size={20} />} title="Cartões do perfil atual">
      {cartoes.data?.length ? cartoes.data.map(item => <EditableCardRow key={item.id} item={item} onSave={alterarCartao.mutate} onToggle={() => statusCartao.mutate(item)} />) : <EmptyState title="Nenhum cartão" description="Cadastre cartões na página de despesas." />}
    </SettingsSection>

    <SettingsSection icon={<UserRound size={20} />} title="Dados do usuário">
      <form className="panel-form settings-form" onSubmit={usuarioForm.handleSubmit(data => alterarUsuario.mutate(data))}>
        <Field label="Nome"><input {...usuarioForm.register("nome", { required: true })} /></Field>
        <Field label="E-mail"><input type="email" {...usuarioForm.register("email", { required: true })} /></Field>
        <button className="primary-action">Salvar dados</button>
      </form>
      <form className="panel-form settings-form" onSubmit={senhaForm.handleSubmit(data => alterarSenha.mutate(data))}>
        <Field label="Senha atual"><input type="password" {...senhaForm.register("senhaAtual", { required: true })} /></Field>
        <Field label="Nova senha" description="Mínimo de 8 caracteres, com maiúscula, minúscula e número."><input type="password" {...senhaForm.register("novaSenha", { required: true })} /></Field>
        <button className="primary-action">Alterar senha</button>
      </form>
    </SettingsSection>
  </section>;
}

function SettingsSection({ icon, title, children }: { icon: React.ReactNode; title: string; children: React.ReactNode }) {
  return <section className="surface settings-section"><header>{icon}<h2>{title}</h2></header>{children}</section>;
}

function EditableNameRow({ name, active, label, onSave, onToggle }: { name: string; active: boolean; label: string; onSave: (name: string) => void; onToggle: () => void }) {
  const [value, setValue] = useState(name);
  useEffect(() => setValue(name), [name]);
  return <div className="settings-row"><input aria-label={`Nome do ${label}`} value={value} onChange={e => setValue(e.target.value)} /><StatusBadge tone={active ? "success" : "neutral"}>{active ? "Ativo" : "Inativo"}</StatusBadge><button className="ghost-button" onClick={() => onSave(value)} disabled={!value.trim() || value.trim() === name}>Salvar</button><button className="ghost-button" onClick={onToggle}>{active ? "Desativar" : "Ativar"}</button></div>;
}

function EditableCardRow({ item, onSave, onToggle }: { item: Cartao; onSave: (item: Cartao) => void; onToggle: () => void }) {
  const [value, setValue] = useState(item);
  useEffect(() => setValue(item), [item]);
  return <div className="settings-card-row">
    <input aria-label="Nome do cartão" value={value.nome} onChange={e => setValue({ ...value, nome: e.target.value })} />
    <input aria-label="Banco" value={value.banco} onChange={e => setValue({ ...value, banco: e.target.value })} />
    <input aria-label="Bandeira" value={value.bandeira} onChange={e => setValue({ ...value, bandeira: e.target.value })} />
    <MoneyInput aria-label="Limite" defaultValue={item.limite.toFixed(2).replace(".", ",")} onChange={e => setValue({ ...value, limite: parseMoney(e.target.value) })} />
    <input aria-label="Fechamento" type="number" min="1" max="31" value={value.diaFechamento} onChange={e => setValue({ ...value, diaFechamento: Number(e.target.value) })} />
    <input aria-label="Vencimento" type="number" min="1" max="31" value={value.diaVencimento} onChange={e => setValue({ ...value, diaVencimento: Number(e.target.value) })} />
    <StatusBadge tone={item.ativo ? "success" : "neutral"}>{item.ativo ? "Ativo" : "Inativo"}</StatusBadge>
    <button className="ghost-button" onClick={() => onSave(value)}>Salvar</button><button className="ghost-button" onClick={onToggle}>{item.ativo ? "Desativar" : "Ativar"}</button>
  </div>;
}
