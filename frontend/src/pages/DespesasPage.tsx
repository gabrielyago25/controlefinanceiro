import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { CheckCircle2, CreditCard, Pencil, Plus, Receipt, RotateCcw, ShoppingBag } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { api } from "../api";
import { usePeriodo } from "../App";
import { useToast } from "../components/Toast";
import { EmptyState, Field, formatDate, Modal, money, MoneyInput, parseMoney, StatusBadge } from "../components/ui";
import "../styles/pages/DespesasPage.css";

type Categoria = { id: string; nome: string; ativo: boolean };
type Despesa = { id: string; descricao: string; valor: number; dataVencimento: string; status: string; atrasada: boolean; categoriaDespesaId: string; observacoes?: string };
type Cartao = { id: string; nome: string; banco: string; bandeira: string; limite: number; diaFechamento: number; diaVencimento: number; ativo: boolean; cor?: string };
type DespesaForm = { descricao: string; valor: number | ""; dataVencimento: string; categoriaDespesaId: string; observacoes: string };
type CartaoForm = { nome: string; banco: string; bandeira: string; limite: number | ""; diaFechamento: number; diaVencimento: number; cor: string };
type CompraCartaoForm = { cartaoId: string; descricao: string; valorTotal: number | ""; dataCompra: string; quantidadeParcelas: number };
type CompraCartao = { id: string; descricao: string; valorTotal: number; dataCompra: string; quantidadeParcelas: number };
type FaturaCartao = { id: string; mesReferencia: string; dataFechamento: string; dataVencimento: string; status: string; valor: number };

const hojeIso = () => new Date().toISOString().slice(0, 10);

export function DespesasPage() {
  const { periodo } = usePeriodo();
  const { showToast } = useToast();
  const [modalContaAberto, setModalContaAberto] = useState(false);
  const [despesaEdicao, setDespesaEdicao] = useState<Despesa | null>(null);
  const [modalCartaoAberto, setModalCartaoAberto] = useState(false);
  const [modalCompraAberto, setModalCompraAberto] = useState(false);
  const [cartaoCompraVinculado, setCartaoCompraVinculado] = useState<Cartao | null>(null);
  const [cartaoDetalhe, setCartaoDetalhe] = useState<Cartao | null>(null);
  const [abaCartao, setAbaCartao] = useState<"compras" | "faturas">("compras");
  const queryClient = useQueryClient();

  const contaForm = useForm<DespesaForm>({
    defaultValues: { descricao: "", valor: "", dataVencimento: "", categoriaDespesaId: "", observacoes: "" }
  });
  const cartaoForm = useForm<CartaoForm>({
    defaultValues: { nome: "", banco: "", bandeira: "", limite: "", diaFechamento: 10, diaVencimento: 17, cor: "#1239c5" }
  });
  const compraCartaoForm = useForm<CompraCartaoForm>({
    defaultValues: { cartaoId: "", descricao: "", valorTotal: "", dataCompra: hojeIso(), quantidadeParcelas: 1 }
  });

  const categorias = useQuery({
    queryKey: ["categorias"],
    queryFn: () => api<Categoria[]>("/api/categorias-despesa")
  });

  const despesas = useQuery({
    queryKey: ["despesas", periodo],
    queryFn: () => api<Despesa[]>(`/api/despesas?mes=${periodo.mes}&ano=${periodo.ano}`)
  });

  const cartoes = useQuery({
    queryKey: ["cartoes"],
    queryFn: () => api<Cartao[]>("/api/cartoes")
  });

  const comprasCartao = useQuery({
    queryKey: ["compras-cartao", cartaoDetalhe?.id],
    queryFn: () => api<CompraCartao[]>(`/api/cartoes/${cartaoDetalhe!.id}/compras`),
    enabled: !!cartaoDetalhe
  });
  const faturasCartao = useQuery({
    queryKey: ["faturas-cartao", cartaoDetalhe?.id],
    queryFn: () => api<FaturaCartao[]>(`/api/cartoes/${cartaoDetalhe!.id}/faturas`),
    enabled: !!cartaoDetalhe
  });

  const salvarConta = useMutation({
    mutationFn: (data: DespesaForm) => api(`/api/despesas${despesaEdicao ? `/${despesaEdicao.id}` : ""}`, {
      method: despesaEdicao ? "PUT" : "POST",
      body: JSON.stringify({ ...data, valor: Number(data.valor), mes: periodo.mes, ano: periodo.ano })
    }),
    onSuccess: () => {
      contaForm.reset();
      setModalContaAberto(false);
      setDespesaEdicao(null);
      queryClient.invalidateQueries({ queryKey: ["despesas"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      showToast({ kind: "success", title: "Conta cadastrada", message: "A conta foi adicionada ao período selecionado." });
    },
    onError: (error) => showToast({ kind: "error", title: "Erro ao cadastrar conta", message: error.message })
  });

  const salvarCartao = useMutation({
    mutationFn: (data: CartaoForm) => api("/api/cartoes", {
      method: "POST",
      body: JSON.stringify({ ...data, limite: Number(data.limite), diaFechamento: Number(data.diaFechamento), diaVencimento: Number(data.diaVencimento) })
    }),
    onSuccess: () => {
      cartaoForm.reset();
      setModalCartaoAberto(false);
      queryClient.invalidateQueries({ queryKey: ["cartoes"] });
      showToast({ kind: "success", title: "Cartão cadastrado", message: "Ele já aparece na lista de cartões." });
    },
    onError: (error) => showToast({ kind: "error", title: "Erro ao cadastrar cartão", message: error.message })
  });

  const salvarCompraCartao = useMutation({
    mutationFn: (data: CompraCartaoForm) => api(`/api/cartoes/${data.cartaoId}/compras`, {
      method: "POST",
      body: JSON.stringify({
        descricao: data.descricao,
        valorTotal: Number(data.valorTotal),
        dataCompra: data.dataCompra,
        quantidadeParcelas: Number(data.quantidadeParcelas)
      })
    }),
    onSuccess: () => {
      compraCartaoForm.reset({ cartaoId: "", descricao: "", valorTotal: "", dataCompra: hojeIso(), quantidadeParcelas: 1 });
      setModalCompraAberto(false);
      setCartaoCompraVinculado(null);
      queryClient.invalidateQueries({ queryKey: ["cartoes"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      showToast({ kind: "success", title: "Compra lançada", message: "As parcelas e faturas foram geradas para o cartão selecionado." });
    },
    onError: (error) => showToast({ kind: "error", title: "Erro ao lançar compra", message: error.message })
  });

  const pagar = useMutation({
    mutationFn: (id: string) => api(`/api/despesas/${id}/pagar`, { method: "PATCH", body: JSON.stringify({}) }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["despesas"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      showToast({ kind: "success", title: "Conta marcada como paga" });
    },
    onError: (error) => showToast({ kind: "error", title: "Não foi possível pagar", message: error.message })
  });

  const reabrir = useMutation({
    mutationFn: (id: string) => api(`/api/despesas/${id}/reabrir`, { method: "PATCH" }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["despesas"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      showToast({ kind: "info", title: "Conta reaberta" });
    },
    onError: (error) => showToast({ kind: "error", title: "Não foi possível reabrir", message: error.message })
  });
  const pagarFatura = useMutation({
    mutationFn: (faturaId: string) => api(`/api/cartoes/${cartaoDetalhe!.id}/faturas/${faturaId}/pagar`, { method: "PATCH" }),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ["faturas-cartao"] }); queryClient.invalidateQueries({ queryKey: ["dashboard"] }); showToast({ kind: "success", title: "Fatura marcada como paga" }); },
    onError: (error) => showToast({ kind: "error", title: "Erro ao pagar fatura", message: error.message })
  });

  const abrirModalCompra = (cartao?: Cartao) => {
    setCartaoCompraVinculado(cartao ?? null);
    compraCartaoForm.reset({
      cartaoId: cartao?.id ?? "",
      descricao: "",
      valorTotal: "",
      dataCompra: hojeIso(),
      quantidadeParcelas: 1
    });
    setModalCompraAberto(true);
  };
  const abrirEdicaoDespesa = (despesa: Despesa) => {
    setDespesaEdicao(despesa);
    contaForm.reset({ descricao: despesa.descricao, valor: despesa.valor, dataVencimento: despesa.dataVencimento, categoriaDespesaId: despesa.categoriaDespesaId, observacoes: despesa.observacoes ?? "" });
    setModalContaAberto(true);
  };

  const existeCartaoAtivo = !!cartoes.data?.some((cartao) => cartao.ativo);

  return (
    <section className="accounts-page">
      <header className="accounts-toolbar">
        <div className="accounts-period">
          <span>Organização financeira</span>
          <h1>Controle de contas</h1>
          <p>Cadastre contas, acompanhe vencimentos e gerencie seus cartões.</p>
        </div>
        <div className="accounts-actions">
          <button className="primary-action action-create" type="button" onClick={() => setModalContaAberto(true)}><Plus size={18} /> Cadastrar conta</button>
          <button className="primary-action action-create" type="button" onClick={() => setModalCartaoAberto(true)}><CreditCard size={18} /> Cadastrar cartão de crédito</button>    
        </div>
      </header>

      <div className="accounts-board">
        <section className="accounts-panel">
          <div className="accounts-panel-header">
            <div>
              <span>Contas</span>
            </div>
            <Receipt size={22} />
          </div>

          {despesas.data?.length ? (
            <div className="bill-list">
              {despesas.data.map((despesa) => (
                <article className="bill-row" key={despesa.id}>
                  <div className="bill-main">
                    <strong>{despesa.descricao}</strong>
                    <span>Vence em {formatDate(despesa.dataVencimento)}</span>
                  </div>
                  <StatusBadge tone={despesa.atrasada ? "danger" : despesa.status === "Paga" ? "success" : "warning"}>{despesa.atrasada ? "Atrasada" : despesa.status}</StatusBadge>
                  <strong className="bill-value">{money(despesa.valor)}</strong>
                  <button className="ghost-button action-edit" type="button" onClick={() => abrirEdicaoDespesa(despesa)}><Pencil size={16} /> Editar</button>
                  {despesa.status === "Paga"
                    ? <button className="ghost-button" onClick={() => reabrir.mutate(despesa.id)}><RotateCcw size={16} /> Reabrir</button>
                    : <button className="ghost-button" onClick={() => pagar.mutate(despesa.id)}><CheckCircle2 size={16} /> Pagar</button>}
                </article>
              ))}
            </div>
          ) : (
            <EmptyState title="Nenhuma conta cadastrada" description="Clique em Cadastrar conta para lançar o primeiro pagamento deste mês." />
          )}
        </section>

        <section className="accounts-panel">
          <div className="accounts-panel-header">
            <div>
              <span>Cartões de Crédito</span>
            </div>
            <CreditCard size={22} />
          </div>

          {cartoes.data?.length ? (
            <div className="card-line-list">
              {cartoes.data.map((cartao) => (
                <article className="card-line" key={cartao.id}>
                  <span className="card-color" style={{ backgroundColor: cartao.cor ?? "#1239c5" }} />
                  <div>
                    <button className="card-name-button" type="button" onClick={() => { setCartaoDetalhe(cartao); setAbaCartao("compras"); }}>{cartao.nome}</button>
                    <span>{cartao.bandeira}</span>
                  </div>
                  <strong>{money(cartao.limite)}</strong>
                  <button className="ghost-button" type="button" onClick={() => abrirModalCompra(cartao)} disabled={!cartao.ativo}><ShoppingBag size={16} /> Compra</button>
                </article>
              ))}
            </div>
          ) : (
            <EmptyState title="Nenhum cartão cadastrado" description="Clique em Cadastrar cartão de crédito para organizar seus cartões." />
          )}
        </section>
      </div>

      <Modal title={cartaoDetalhe?.nome ?? "Detalhes do cartão"} description="Consulte o histórico de compras e as faturas geradas mês a mês." open={!!cartaoDetalhe} onClose={() => setCartaoDetalhe(null)}>
        <div className="card-detail-tabs">
          <button className={abaCartao === "compras" ? "active" : ""} type="button" onClick={() => setAbaCartao("compras")}>Compras</button>
          <button className={abaCartao === "faturas" ? "active" : ""} type="button" onClick={() => setAbaCartao("faturas")}>Faturas mês a mês</button>
        </div>
        <div className="table-wrap card-detail-content">
          {abaCartao === "compras" ? (comprasCartao.data?.length ? <table><thead><tr><th>Compra</th><th>Data</th><th>Parcelas</th><th>Valor total</th></tr></thead><tbody>{comprasCartao.data.map(compra => <tr key={compra.id}><td><strong>{compra.descricao}</strong></td><td>{formatDate(compra.dataCompra)}</td><td>{compra.quantidadeParcelas}x</td><td>{money(compra.valorTotal)}</td></tr>)}</tbody></table> : <EmptyState title="Nenhuma compra" description="Ainda não existem compras lançadas neste cartão." />)
            : (faturasCartao.data?.length ? <table><thead><tr><th>Mês</th><th>Vencimento</th><th>Status</th><th>Valor</th><th>Ações</th></tr></thead><tbody>{faturasCartao.data.map(fatura => <tr key={fatura.id}><td>{formatDate(fatura.mesReferencia)}</td><td>{formatDate(fatura.dataVencimento)}</td><td><StatusBadge tone={fatura.status === "Paga" ? "success" : "warning"}>{fatura.status}</StatusBadge></td><td>{money(fatura.valor)}</td><td>{fatura.status !== "Paga" && <button className="ghost-button" onClick={() => pagarFatura.mutate(fatura.id)}><CheckCircle2 size={15} /> Pagar</button>}</td></tr>)}</tbody></table> : <EmptyState title="Nenhuma fatura" description="As faturas aparecerão após o lançamento de uma compra." />)}
        </div>
      </Modal>

      <Modal title={despesaEdicao ? "Editar conta" : "Cadastrar conta"} description="Informe os dados da conta." open={modalContaAberto} onClose={() => { setModalContaAberto(false); setDespesaEdicao(null); contaForm.reset(); }}>
        <form className="modal-form" onSubmit={contaForm.handleSubmit((data) => salvarConta.mutate(data))}>
          <Field label="Descrição da conta" description="Exemplo: aluguel, internet, mercado." error={contaForm.formState.errors.descricao?.message}>
            <input {...contaForm.register("descricao", { required: "Informe a descrição da conta." })} />
          </Field>
          <Field label="Valor" description="Use o valor total em reais." error={contaForm.formState.errors.valor?.message}>
            <MoneyInput placeholder="Ex.: 1.500,00" {...contaForm.register("valor", { required: "Informe o valor.", setValueAs: parseMoney, min: { value: 0.01, message: "O valor deve ser maior que zero." } })} />
          </Field>
          <Field label="Data de vencimento" description="Data limite para pagamento." error={contaForm.formState.errors.dataVencimento?.message}>
            <input type="date" {...contaForm.register("dataVencimento", { required: "Informe o vencimento." })} />
          </Field>
          <Field label="Categoria" description="Classificação usada no dashboard." error={contaForm.formState.errors.categoriaDespesaId?.message}>
            <select {...contaForm.register("categoriaDespesaId", { required: "Selecione uma categoria." })}>
              <option value="">Selecione</option>
              {categorias.data?.filter(c => c.ativo).map(c => <option key={c.id} value={c.id}>{c.nome}</option>)}
            </select>
          </Field>
          <Field label="Observações" description="Campo opcional para detalhes internos.">
            <input {...contaForm.register("observacoes")} />
          </Field>
          <div className="modal-actions">
            <button className="ghost-button" type="button" onClick={() => { setModalContaAberto(false); setDespesaEdicao(null); contaForm.reset(); }}>Cancelar</button>
            <button className={`primary-action ${despesaEdicao ? "" : "action-create"}`} disabled={salvarConta.isPending}>{salvarConta.isPending ? "Salvando..." : despesaEdicao ? "Salvar alterações" : "Salvar conta"}</button>
          </div>
        </form>
      </Modal>

      <Modal title="Cadastrar cartão de crédito" description="Informe o cartão que será usado no controle de contas e faturas." open={modalCartaoAberto} onClose={() => setModalCartaoAberto(false)}>
        <form className="modal-form" onSubmit={cartaoForm.handleSubmit((data) => salvarCartao.mutate(data))}>
          <Field label="Nome do cartão" description="Apelido exibido no sistema. Exemplo: Nubank roxo." error={cartaoForm.formState.errors.nome?.message}>
            <input {...cartaoForm.register("nome", { required: "Informe o nome do cartão." })} />
          </Field>
          <Field label="Banco ou instituição" description="Nome do banco emissor." error={cartaoForm.formState.errors.banco?.message}>
            <input {...cartaoForm.register("banco", { required: "Informe o banco." })} />
          </Field>
          <Field label="Bandeira" description="Selecione a bandeira do cartão." error={cartaoForm.formState.errors.bandeira?.message}>
            <select {...cartaoForm.register("bandeira", { required: "Selecione a bandeira." })}>
              <option value="">Selecione</option>
              <option value="Visa">Visa</option>
              <option value="Mastercard">Mastercard</option>
            </select>
          </Field>
          <Field label="Limite" description="Limite total disponível no cartão." error={cartaoForm.formState.errors.limite?.message}>
            <MoneyInput placeholder="Ex.: 5.000,00" {...cartaoForm.register("limite", { required: "Informe o limite.", setValueAs: parseMoney, min: { value: 0, message: "O limite não pode ser negativo." } })} />
          </Field>
          <Field label="Dia de fechamento" description="Dia do mês em que a fatura fecha." error={cartaoForm.formState.errors.diaFechamento?.message}>
            <input type="number" min="1" max="31" {...cartaoForm.register("diaFechamento", { min: 1, max: 31 })} />
          </Field>
          <Field label="Dia de vencimento" description="Dia do mês em que a fatura vence." error={cartaoForm.formState.errors.diaVencimento?.message}>
            <input type="number" min="1" max="31" {...cartaoForm.register("diaVencimento", { min: 1, max: 31 })} />
          </Field>
          <Field label="Cor de identificação" description="Ajuda a diferenciar cartões na listagem.">
            <input type="color" {...cartaoForm.register("cor")} />
          </Field>
          <div className="modal-actions">
            <button className="ghost-button" type="button" onClick={() => setModalCartaoAberto(false)}>Cancelar</button>
            <button className="primary-action action-create" disabled={salvarCartao.isPending}>{salvarCartao.isPending ? "Salvando..." : "Salvar cartão"}</button>
          </div>
        </form>
      </Modal>

      <Modal title={`Lançar compra no ${cartaoCompraVinculado?.nome ?? "cartão"}`} description="Informe a compra e a quantidade de parcelas. O sistema criará as parcelas e faturas automaticamente." open={modalCompraAberto} onClose={() => { setModalCompraAberto(false); setCartaoCompraVinculado(null); }}>
        <form className="modal-form" onSubmit={compraCartaoForm.handleSubmit((data) => salvarCompraCartao.mutate(data))}>
          <Field label="Cartão de crédito" description="Esta compra será vinculada somente a este cartão." error={compraCartaoForm.formState.errors.cartaoId?.message}>
            <input type="hidden" {...compraCartaoForm.register("cartaoId", { required: "Selecione o cartão." })} />
            <div className="readonly-card-field"><CreditCard size={18} /><strong>{cartaoCompraVinculado?.nome}</strong><span>{cartaoCompraVinculado?.bandeira}</span></div>
          </Field>
          <Field label="Descrição da compra" description="Exemplo: notebook, mercado, combustível." error={compraCartaoForm.formState.errors.descricao?.message}>
            <input {...compraCartaoForm.register("descricao", { required: "Informe a descrição da compra." })} />
          </Field>
          <Field label="Valor total" description="Valor total da compra, antes da divisão em parcelas." error={compraCartaoForm.formState.errors.valorTotal?.message}>
            <MoneyInput placeholder="Ex.: 350,00" {...compraCartaoForm.register("valorTotal", { required: "Informe o valor total.", setValueAs: parseMoney, min: { value: 0.01, message: "O valor deve ser maior que zero." } })} />
          </Field>
          <Field label="Data da compra" description="Data em que a compra foi realizada." error={compraCartaoForm.formState.errors.dataCompra?.message}>
            <input type="date" {...compraCartaoForm.register("dataCompra", { required: "Informe a data da compra." })} />
          </Field>
          <Field label="Quantidade de parcelas" description="Use 1 para compra à vista no cartão." error={compraCartaoForm.formState.errors.quantidadeParcelas?.message}>
            <select {...compraCartaoForm.register("quantidadeParcelas", { required: "Selecione a quantidade de parcelas.", valueAsNumber: true })}>
              {Array.from({ length: 24 }, (_, indice) => indice + 1).map(quantidade => (
                <option key={quantidade} value={quantidade}>{quantidade}x</option>
              ))}
            </select>
          </Field>
          <div className="modal-actions">
            <button className="ghost-button" type="button" onClick={() => { setModalCompraAberto(false); setCartaoCompraVinculado(null); }}>Cancelar</button>
            <button className="primary-action action-create" disabled={salvarCompraCartao.isPending}>{salvarCompraCartao.isPending ? "Lançando..." : "Lançar compra"}</button>
          </div>
        </form>
      </Modal>
    </section>
  );
}
