import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Check, CreditCard, Pencil, Plus, Tags, UserRound, X } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { api, Usuario } from "../api";
import { useAuth } from "../App";
import { useToast } from "../components/Toast";
import { EmptyState, Field, MoneyInput, PageHeader, parseMoney, StatusBadge } from "../components/ui";
import "../styles/pages/ConfiguracoesPage.css";

type Categoria = { id: string; nome: string; ativo: boolean };
type Cartao = { id: string; nome: string; banco: string; bandeira: string; limite: number; diaFechamento: number; diaVencimento: number; ativo: boolean; cor?: string };

export function ConfiguracoesPage() {
  const { usuario, setUsuario } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [editandoUsuario, setEditandoUsuario] = useState(false);
  const categoriaForm = useForm<{ nome: string }>({ defaultValues: { nome: "" } });
  const usuarioForm = useForm<{ nome: string; email: string }>({ defaultValues: { nome: usuario?.nome ?? "", email: usuario?.email ?? "" } });
  const senhaForm = useForm<{ senhaAtual: string; novaSenha: string }>({ defaultValues: { senhaAtual: "", novaSenha: "" } });
  const mostrarErro = (error: Error) => showToast({ kind: "error", title: "Não foi possível salvar", message: error.message });

  useEffect(() => usuarioForm.reset({ nome: usuario?.nome ?? "", email: usuario?.email ?? "" }), [usuario]);

  const categorias = useQuery({ queryKey: ["categorias"], queryFn: () => api<Categoria[]>("/api/categorias-despesa") });
  const cartoes = useQuery({ queryKey: ["cartoes"], queryFn: () => api<Cartao[]>("/api/cartoes") });

  const criarCategoria = useMutation({
    mutationFn: (data: { nome: string }) => api("/api/categorias-despesa", { method: "POST", body: JSON.stringify(data) }),
    onSuccess: () => { categoriaForm.reset(); queryClient.invalidateQueries({ queryKey: ["categorias"] }); showToast({ kind: "success", title: "Categoria criada" }); },
    onError: (error) => showToast({ kind: "error", title: "Erro ao criar categoria", message: error.message })
  });
  const alterarCategoria = useMutation({
    mutationFn: ({ categoria, nome }: { categoria: Categoria; nome: string }) => api<Categoria>(`/api/categorias-despesa/${categoria.id}`, { method: "PUT", body: JSON.stringify({ nome }) }),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ["categorias"] }); showToast({ kind: "success", title: "Categoria atualizada" }); }, onError: mostrarErro
  });
  const statusCategoria = useMutation({
    mutationFn: (categoria: Categoria) => api(`/api/categorias-despesa/${categoria.id}/${categoria.ativo ? "desativar" : "ativar"}`, { method: "PATCH" }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["categorias"] }), onError: mostrarErro
  });
  const alterarCartao = useMutation({
    mutationFn: (item: Cartao) => api<Cartao>(`/api/cartoes/${item.id}`, { method: "PUT", body: JSON.stringify(item) }),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ["cartoes"] }); showToast({ kind: "success", title: "Cartão atualizado" }); }, onError: mostrarErro
  });
  const statusCartao = useMutation({
    mutationFn: (item: Cartao) => api(`/api/cartoes/${item.id}/${item.ativo ? "desativar" : "ativar"}`, { method: "PATCH" }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["cartoes"] }), onError: mostrarErro
  });
  const alterarUsuario = useMutation({
    mutationFn: (data: { nome: string; email: string }) => api<Usuario>("/api/autenticacao/me", { method: "PUT", body: JSON.stringify(data) }),
    onSuccess: (atualizado) => { setUsuario(atualizado); setEditandoUsuario(false); showToast({ kind: "success", title: "Dados pessoais atualizados" }); },
    onError: (error) => showToast({ kind: "error", title: "Erro ao atualizar usuário", message: error.message })
  });
  const alterarSenha = useMutation({
    mutationFn: (data: { senhaAtual: string; novaSenha: string }) => api("/api/autenticacao/me/senha", { method: "PUT", body: JSON.stringify(data) }),
    onSuccess: () => { senhaForm.reset(); showToast({ kind: "success", title: "Senha atualizada" }); },
    onError: (error) => showToast({ kind: "error", title: "Erro ao atualizar senha", message: error.message })
  });

  return <section className="page-stack settings-stack">
    <PageHeader title="Configurações" description="Edite categorias, cartões e os dados da sua conta." />

    <SettingsSection icon={<Tags size={20} />} title="Categorias de despesa">
      <form className="panel-form compact-form" onSubmit={categoriaForm.handleSubmit(data => criarCategoria.mutate(data))}>
        <Field label="Nova categoria" error={categoriaForm.formState.errors.nome?.message}><input {...categoriaForm.register("nome", { required: "Informe o nome." })} /></Field>
        <button className="primary-action action-create"><Plus size={18} /> Criar</button>
      </form>
      {categorias.data?.length ? categorias.data.map(item => <EditableNameRow key={item.id} name={item.nome} active={item.ativo} label="categoria" onSave={nome => alterarCategoria.mutate({ categoria: item, nome })} onToggle={() => statusCategoria.mutate(item)} />) : <EmptyState title="Nenhuma categoria" description="Crie a primeira categoria acima." />}
    </SettingsSection>

    <SettingsSection icon={<CreditCard size={20} />} title="Cartões">
      {cartoes.data?.length ? cartoes.data.map(item => <EditableCardRow key={item.id} item={item} onSave={alterarCartao.mutate} onToggle={() => statusCartao.mutate(item)} />) : <EmptyState title="Nenhum cartão" description="Cadastre cartões na página de despesas." />}
    </SettingsSection>

    <SettingsSection icon={<UserRound size={20} />} title="Dados do usuário">
      <form className="panel-form settings-form" onSubmit={usuarioForm.handleSubmit(data => alterarUsuario.mutate(data))}>
        <Field label="Nome"><input disabled={!editandoUsuario} {...usuarioForm.register("nome", { required: true })} /></Field>
        <Field label="E-mail"><input type="email" disabled={!editandoUsuario} {...usuarioForm.register("email", { required: true })} /></Field>
        {editandoUsuario ? (
          <div className="settings-form-actions">
            <button className="primary-action" disabled={alterarUsuario.isPending}><Check size={16} /> Salvar</button>
            <button className="ghost-button action-edit" type="button" onClick={() => { usuarioForm.reset({ nome: usuario?.nome ?? "", email: usuario?.email ?? "" }); setEditandoUsuario(false); }}><X size={16} /> Cancelar</button>
          </div>
        ) : <button className="primary-action action-edit" type="button" onClick={() => setEditandoUsuario(true)}><Pencil size={16} /> Editar</button>}
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
  const [editando, setEditando] = useState(false);
  useEffect(() => setValue(name), [name]);
  const salvar = () => { onSave(value); setEditando(false); };
  const cancelar = () => { setValue(name); setEditando(false); };
  return <div className="settings-row">
    <input aria-label={`Nome do ${label}`} value={value} disabled={!editando} onChange={e => setValue(e.target.value)} />
    <StatusBadge tone={active ? "success" : "neutral"}>{active ? "Ativo" : "Inativo"}</StatusBadge>
    <div className="settings-row-actions">
      {editando ? <><button className="ghost-button action-save" type="button" onClick={salvar} disabled={!value.trim() || value.trim() === name}><Check size={15} /> Salvar</button><button className="ghost-button action-edit" type="button" onClick={cancelar} aria-label={`Cancelar edição da ${label}`}><X size={15} /></button></> : <button className="ghost-button action-edit" type="button" onClick={() => setEditando(true)}><Pencil size={15} /> Editar</button>}
    </div>
    <button className={`ghost-button ${active ? "action-danger" : "action-create"}`} type="button" onClick={onToggle}>{active ? "Desativar" : "Ativar"}</button>
  </div>;
}

function EditableCardRow({ item, onSave, onToggle }: { item: Cartao; onSave: (item: Cartao) => void; onToggle: () => void }) {
  const [value, setValue] = useState(item);
  const [editando, setEditando] = useState(false);
  useEffect(() => setValue(item), [item]);
  const alterado = value.nome !== item.nome || value.banco !== item.banco || value.bandeira !== item.bandeira || value.limite !== item.limite || value.diaFechamento !== item.diaFechamento || value.diaVencimento !== item.diaVencimento;
  const salvar = () => { onSave(value); setEditando(false); };
  const cancelar = () => { setValue(item); setEditando(false); };
  return <div className="settings-card-row">
    <input aria-label="Nome do cartão" value={value.nome} disabled={!editando} onChange={e => setValue({ ...value, nome: e.target.value })} />
    <input aria-label="Banco" value={value.banco} disabled={!editando} onChange={e => setValue({ ...value, banco: e.target.value })} />
    <input aria-label="Bandeira" value={value.bandeira} disabled={!editando} onChange={e => setValue({ ...value, bandeira: e.target.value })} />
    <MoneyInput key={`${item.id}-${item.limite}`} aria-label="Limite" defaultValue={item.limite.toFixed(2).replace(".", ",")} disabled={!editando} onChange={e => setValue({ ...value, limite: parseMoney(e.target.value) })} />
    <input aria-label="Fechamento" type="number" min="1" max="31" value={value.diaFechamento} disabled={!editando} onChange={e => setValue({ ...value, diaFechamento: Number(e.target.value) })} />
    <input aria-label="Vencimento" type="number" min="1" max="31" value={value.diaVencimento} disabled={!editando} onChange={e => setValue({ ...value, diaVencimento: Number(e.target.value) })} />
    <StatusBadge tone={item.ativo ? "success" : "neutral"}>{item.ativo ? "Ativo" : "Inativo"}</StatusBadge>
    <div className="settings-row-actions">
      {editando ? <><button className="ghost-button action-save" type="button" onClick={salvar} disabled={!alterado}><Check size={15} /> Salvar</button><button className="ghost-button action-edit" type="button" onClick={cancelar} aria-label="Cancelar edição do cartão"><X size={15} /></button></> : <button className="ghost-button action-edit" type="button" onClick={() => setEditando(true)}><Pencil size={15} /> Editar</button>}
    </div>
    <button className={`ghost-button ${item.ativo ? "action-danger" : "action-create"}`} type="button" onClick={onToggle}>{item.ativo ? "Desativar" : "Ativar"}</button>
  </div>;
}
