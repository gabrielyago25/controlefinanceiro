import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Pencil, Plus, Trash2 } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { api } from "../api";
import { usePeriodo } from "../App";
import { useToast } from "../components/Toast";
import { EmptyState, Field, formatDate, Modal, money, MoneyInput, PageHeader, parseMoney } from "../components/ui";

type Receita = { id: string; descricao: string; valor: number; dataRecebimento: string; observacoes?: string };
type ReceitaForm = { descricao: string; valor: number | ""; dataRecebimento: string; observacoes: string };

export function ReceitasPage() {
  const { periodo } = usePeriodo();
  const { showToast } = useToast();
  const [receitaEdicao, setReceitaEdicao] = useState<Receita | null>(null);
  const [receitaParaExcluir, setReceitaParaExcluir] = useState<Receita | null>(null);
  const queryClient = useQueryClient();
  const { register, handleSubmit, reset, formState } = useForm<ReceitaForm>({ defaultValues: { descricao: "", valor: "", dataRecebimento: "", observacoes: "" } });
  const receitas = useQuery({
    queryKey: ["receitas", periodo],
    queryFn: () => api<Receita[]>(`/api/receitas?mes=${periodo.mes}&ano=${periodo.ano}`)
  });
  const salvar = useMutation({
    mutationFn: (data: ReceitaForm) => api(`/api/receitas${receitaEdicao ? `/${receitaEdicao.id}` : ""}`, { method: receitaEdicao ? "PUT" : "POST", body: JSON.stringify({ ...data, valor: Number(data.valor), mes: periodo.mes, ano: periodo.ano }) }),
    onSuccess: () => {
      reset();
      setReceitaEdicao(null);
      queryClient.invalidateQueries({ queryKey: ["receitas"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      showToast({ kind: "success", title: "Receita adicionada", message: "Entrada registrada no período selecionado." });
    },
    onError: (error) => showToast({ kind: "error", title: "Erro ao criar receita", message: error.message })
  });
  const excluir = useMutation({
    mutationFn: (id: string) => api(`/api/receitas/${id}`, { method: "DELETE" }),
    onSuccess: () => { setReceitaParaExcluir(null); queryClient.invalidateQueries({ queryKey: ["receitas"] }); queryClient.invalidateQueries({ queryKey: ["dashboard"] }); showToast({ kind: "success", title: "Receita excluída" }); },
    onError: (error) => showToast({ kind: "error", title: "Erro ao excluir receita", message: error.message })
  });
  const editar = (receita: Receita) => { setReceitaEdicao(receita); reset({ descricao: receita.descricao, valor: receita.valor, dataRecebimento: receita.dataRecebimento, observacoes: receita.observacoes ?? "" }); };

  return (
    <section className="page-stack">
      <PageHeader title="Receitas" description="Registre salários, vendas, aluguéis recebidos e outras entradas." />
      <form className="surface panel-form" onSubmit={handleSubmit((data) => salvar.mutate(data))}>
        <Field label="Descrição da receita" description="Exemplo: salário, venda, serviço prestado." error={formState.errors.descricao?.message}>
          <input {...register("descricao", { required: "Informe a descrição da receita." })} />
        </Field>
        <Field label="Valor recebido" description="Valor em reais." error={formState.errors.valor?.message}>
          <MoneyInput placeholder="Ex.: 2.500,00" {...register("valor", { required: "Informe o valor.", setValueAs: parseMoney, min: { value: 0.01, message: "O valor deve ser maior que zero." } })} />
        </Field>
        <Field label="Data de recebimento" description="Quando o dinheiro entrou ou entrará." error={formState.errors.dataRecebimento?.message}>
          <input type="date" {...register("dataRecebimento", { required: "Informe a data de recebimento." })} />
        </Field>
        <Field label="Observações" description="Campo opcional para detalhes internos.">
          <input {...register("observacoes")} />
        </Field>
        <button className={`primary-action form-submit ${receitaEdicao ? "" : "action-create"}`} disabled={salvar.isPending}><Plus size={18} /> {salvar.isPending ? "Salvando..." : receitaEdicao ? "Salvar alterações" : "Adicionar receita"}</button>
        {receitaEdicao && <button className="ghost-button" type="button" onClick={() => { setReceitaEdicao(null); reset(); }}>Cancelar edição</button>}
      </form>

      <div className="surface table-wrap">
        {receitas.data?.length ? (
          <table>
            <thead><tr><th>Receita</th><th>Recebimento</th><th>Valor</th><th>Ações</th></tr></thead>
            <tbody>{receitas.data.map(r => <tr key={r.id}><td><strong>{r.descricao}</strong><span>{r.observacoes}</span></td><td>{formatDate(r.dataRecebimento)}</td><td>{money(r.valor)}</td><td className="row-actions"><button className="ghost-button action-edit" onClick={() => editar(r)}><Pencil size={15} /> Editar</button> <button className="ghost-button action-danger" onClick={() => setReceitaParaExcluir(r)}><Trash2 size={15} /> Excluir</button></td></tr>)}</tbody>
          </table>
        ) : (
          <EmptyState title="Nenhuma receita lançada" description="Adicione entradas para compor o saldo mensal do dashboard." />
        )}
      </div>
      <Modal title="Excluir receita" description="Esta ação é permanente e não poderá ser desfeita." open={!!receitaParaExcluir} onClose={() => setReceitaParaExcluir(null)}>
        <p>Confirma a exclusão de <strong>{receitaParaExcluir?.descricao}</strong>, no valor de <strong>{money(receitaParaExcluir?.valor)}</strong>?</p>
        <div className="modal-actions">
          <button className="ghost-button" type="button" onClick={() => setReceitaParaExcluir(null)}>Cancelar</button>
          <button className="primary-action danger-action" type="button" disabled={excluir.isPending} onClick={() => receitaParaExcluir && excluir.mutate(receitaParaExcluir.id)}>{excluir.isPending ? "Excluindo..." : "Confirmar exclusão"}</button>
        </div>
      </Modal>
    </section>
  );
}
