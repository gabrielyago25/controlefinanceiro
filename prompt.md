Atue como um engenheiro de software sênior, arquiteto de sistemas e desenvolvedor full stack especializado em C#, ASP.NET Core, Entity Framework Core, PostgreSQL, React e TypeScript.

Sua tarefa é continuar e concluir autonomamente o desenvolvimento de um SaaS de controle financeiro chamado ControleFinanceiro.

IMPORTANTE:

- Trabalhe diretamente no repositório aberto no VS Code.
- Antes de alterar qualquer coisa, inspecione completamente a estrutura e os arquivos existentes.
- Preserve todo o código válido já existente.
- Não recrie o projeto do zero.
- Não renomeie arbitrariamente projetos, classes, propriedades, namespaces, tabelas, pastas, endpoints ou contratos existentes.
- Não remova migrations já aplicadas.
- Não gere longas explicações.
- Não forneça tutoriais.
- Não pare a cada pequena etapa para pedir confirmação.
- Implemente diretamente.
- Execute builds e testes.
- Corrija automaticamente erros encontrados.
- Continue até concluir o escopo solicitado ou até encontrar um bloqueio real que não possa ser resolvido sem informações externas.
- Ao final, forneça apenas um resumo objetivo do que foi criado, arquivos principais alterados, migrations adicionadas, testes executados e eventuais pendências reais.

==================================================
1. ESTADO ATUAL DO PROJETO
==================================================

O projeto já existe e NÃO deve ser recriado.

A estrutura atual é aproximadamente:

financas/
│
├── backend/
│   │
│   ├── ControleFinanceiro/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── ControleFinanceiro.csproj
│   │
│   ├── ControleFinanceiro.Application/
│   │   └── ControleFinanceiro.Application.csproj
│   │
│   ├── ControleFinanceiro.Domain/
│   │   ├── Usuarios/
│   │   │   └── Usuario.cs
│   │   ├── Perfis/
│   │   │   └── Perfis.cs
│   │   └── ControleFinanceiro.Domain.csproj
│   │
│   ├── ControleFinanceiro.Infrastructure/
│   │   ├── Persistencia/
│   │   │   ├── ControleFinanceiroDbContext.cs
│   │   │   ├── Configuracoes/
│   │   │   │   ├── UsuarioConfiguracao.cs
│   │   │   │   └── PerfisConfiguracao.cs
│   │   │   └── Migrations/
│   │   │       └── InitialCreate e arquivos relacionados
│   │   ├── InjecaoDependencia/
│   │   │   └── ConfiguracaoDependencias.cs
│   │   └── ControleFinanceiro.Infrastructure.csproj
│   │
│   └── ControleFinanceiro.sln
│
└── frontend/

Antes de trabalhar, confirme a estrutura real lendo os arquivos existentes.

==================================================
2. TECNOLOGIAS EXISTENTES
==================================================

Backend:

- C#
- .NET 10
- ASP.NET Core Web API
- Controllers
- Entity Framework Core 10
- PostgreSQL
- Npgsql
- OpenAPI/Swagger

Projetos:

- ControleFinanceiro
- ControleFinanceiro.Application
- ControleFinanceiro.Domain
- ControleFinanceiro.Infrastructure

Arquitetura:

- Monólito modular.
- Não transformar em microserviços.
- Separação entre API, Application, Domain e Infrastructure.

Banco:

- PostgreSQL.
- Banco atual: controle_financeiro.

Migration inicial já aplicada:

InitialCreate

Ela criou:

Usuarios
Perfis
__EFMigrationsHistory

Não modificar nem remover a migration InitialCreate aplicada.

Qualquer alteração futura de banco deverá gerar uma NOVA migration.

==================================================
3. CONVENÇÕES DE NOMES
==================================================

Os identificadores de domínio devem permanecer em português.

Exemplos:

Usuario
Perfis
Despesa
CategoriaDespesa
Receita
CartaoCredito
CompraCartao
ParcelaCartao
FaturaCartao

Propriedades:

Nome
Email
SenhaHash
Ativo
CriadoEm
Descricao
Valor
DataVencimento
DataPagamento
UsuarioId
PerfilId
CodigoMoeda

Não converter os nomes de domínio para inglês.

Os projetos arquiteturais permanecem:

ControleFinanceiro.Application
ControleFinanceiro.Domain
ControleFinanceiro.Infrastructure

Preserve a classe existente chamada:

Perfis

Mesmo estando no plural.

Não renomeá-la para Perfil ou PerfilFinanceiro.

==================================================
4. ENTIDADES JÁ EXISTENTES
==================================================

A entidade Usuario já existe aproximadamente com:

- Id
- Nome
- Email
- SenhaHash
- Ativo
- CriadoEm

Ela possui:

- construtor controlado;
- validações essenciais;
- normalização de nome;
- normalização de e-mail;
- AlterarNome;
- AlterarEmail;
- AlterarSenhaHash;
- Ativar;
- Desativar.

A entidade Perfis já existe aproximadamente com:

- Id
- Nome
- UsuarioId
- CodigoMoeda
- Ativo
- CriadoEm

Ela possui:

- construtor controlado;
- validações;
- normalização;
- AlterarNome;
- Ativar;
- Desativar.

Preserve essas implementações e evolua somente quando necessário.

==================================================
5. BANCO E MAPEAMENTOS EXISTENTES
==================================================

Tabela Usuarios:

- Id uuid PK
- Nome varchar(150) NOT NULL
- Email varchar(254) NOT NULL
- SenhaHash varchar(500) NOT NULL
- Ativo boolean NOT NULL
- CriadoEm timestamp with time zone NOT NULL

Existe índice UNIQUE para Email.

Tabela Perfis:

- Id uuid PK
- Nome varchar(100) NOT NULL
- UsuarioId uuid NOT NULL
- CodigoMoeda varchar(3) NOT NULL
- Ativo boolean NOT NULL
- CriadoEm timestamp with time zone NOT NULL

Relacionamento:

Usuario 1:N Perfis

Perfis.UsuarioId → Usuarios.Id

DeleteBehavior:

Restrict

Existe índice em Perfis.UsuarioId.

==================================================
6. OBJETIVO DO PRODUTO
==================================================

O sistema é um SaaS de controle financeiro pessoal, residencial e empresarial.

Um usuário deve poder:

1. Criar uma conta.
2. Fazer login.
3. Possuir um ou vários perfis financeiros.
4. Selecionar um perfil após o login.
5. Alternar entre perfis sem logout.
6. Gerenciar separadamente os dados financeiros de cada perfil.

Exemplo:

Usuario: Gabriel

Perfis:
- Casa
- Loja
- Empresa XYZ

Cada perfil possui dados completamente independentes:

- despesas;
- receitas;
- categorias;
- cartões;
- compras;
- parcelas;
- faturas;
- configurações.

Dados de um perfil nunca podem ser misturados aos de outro perfil.

==================================================
7. REGRA CENTRAL DE SEGURANÇA
==================================================

Toda informação financeira pertence a um Perfis.

A regra mais importante do sistema é:

Um usuário autenticado nunca pode acessar um perfil pertencente a outro usuário.

Também nunca pode acessar:

- despesa;
- receita;
- categoria;
- cartão;
- compra;
- parcela;
- fatura;

pertencente a um perfil sobre o qual ele não possui autorização.

Toda consulta de recurso financeiro deve validar:

Usuario autenticado
+
Perfil solicitado
+
Propriedade do recurso

Não confiar no frontend.

Não considerar GUID difícil de adivinhar como mecanismo de segurança.

Preferencialmente retornar 404 para recursos inexistentes ou não pertencentes ao usuário, evitando enumeração de recursos.

==================================================
8. AUTENTICAÇÃO
==================================================

Implementar:

- cadastro;
- login;
- JWT;
- refresh token;
- logout/revogação do refresh token;
- endpoint para obter usuário autenticado;
- proteção dos endpoints.

JWT deve possuir pelo menos:

sub = Id do Usuario

Não colocar PerfilId dentro do JWT.

O perfil ativo é escolhido pelo frontend e enviado nas rotas ou requisições apropriadas.

O backend sempre valida se o perfil pertence ao usuário autenticado.

Utilizar:

- Access Token JWT de curta duração.
- Refresh Token com expiração e rotação.
- Armazenamento seguro do refresh token no banco.
- Nunca armazenar senha em texto puro.

Utilizar mecanismo seguro de hashing de senha.

Não criar algoritmo criptográfico próprio.

Pode utilizar PasswordHasher<Usuario> do ecossistema ASP.NET Core ou outra solução madura e segura, desde que haja justificativa técnica clara e poucas dependências adicionais.

No frontend:

- evitar armazenar refresh tokens longos em localStorage;
- preferir cookie HttpOnly, Secure quando apropriado e SameSite adequadamente configurado;
- implementar proteção contra os riscos associados ao método escolhido.

Não expor segredos em:

- código-fonte;
- appsettings versionado;
- logs;
- respostas HTTP.

Preservar o uso atual de User Secrets no ambiente local.

==================================================
9. CASOS DE USO DE USUARIO E AUTENTICACAO
==================================================

Implementar pelo menos:

POST /api/autenticacao/cadastro
POST /api/autenticacao/login
POST /api/autenticacao/refresh
POST /api/autenticacao/logout
GET  /api/autenticacao/me

Cadastro:

Entrada:

- nome
- email
- senha

Regras:

- nome obrigatório;
- email obrigatório;
- formato de email válido;
- senha com regras mínimas de segurança;
- email normalizado;
- email único;
- senha convertida para hash seguro;
- novo usuário nasce ativo.

Não retornar SenhaHash em nenhuma resposta.

Login:

Entrada:

- email
- senha

Regras:

- normalizar email;
- mensagem genérica para credenciais inválidas;
- não revelar se o email existe;
- negar login de usuário desativado;
- emitir access token;
- emitir refresh token seguro.

==================================================
10. PERFIS
==================================================

Implementar:

GET    /api/perfis
POST   /api/perfis
PUT    /api/perfis/{id}
PATCH  /api/perfis/{id}/ativar
PATCH  /api/perfis/{id}/desativar

Regras:

- somente retornar perfis do usuário autenticado;
- não permitir editar perfil de outro usuário;
- novo perfil usa BRL por padrão;
- CodigoMoeda deve ter três caracteres;
- perfil desativado mantém histórico;
- não excluir fisicamente perfis no MVP.

Caso o usuário possua somente um perfil:

- o frontend pode selecioná-lo automaticamente.

Caso tenha vários:

- mostrar tela de seleção.

Permitir alternar entre perfis sem logout.

==================================================
11. DESPESAS
==================================================

Criar domínio de despesas.

Criar entidade:

Despesa

Campos recomendados para o MVP:

- Id
- Descricao
- Valor decimal
- DataVencimento DateOnly
- DataPagamento DateOnly?
- Competencia DateOnly
- Status
- Observacoes?
- CategoriaDespesaId
- PerfilId
- CriadoEm

Nunca usar float ou double para dinheiro.

Usar decimal em C#.

Configurar numeric apropriado no PostgreSQL, por exemplo numeric(18,2).

Status persistido:

- Pendente
- Paga

Atrasada deve preferencialmente ser condição calculada:

Status == Pendente
e
DataVencimento < data atual

Evitar armazenar "Atrasada" se isso puder gerar inconsistência.

Competencia deve representar o mês financeiro.

Usar uma única data normalizada para o primeiro dia do mês, em vez de propriedades separadas Mes e Ano.

Exemplo:

Julho de 2026:

2026-07-01

Implementar:

GET    /api/perfis/{perfilId}/despesas?mes=7&ano=2026
POST   /api/perfis/{perfilId}/despesas
PUT    /api/perfis/{perfilId}/despesas/{id}
PATCH  /api/perfis/{perfilId}/despesas/{id}/pagar
PATCH  /api/perfis/{perfilId}/despesas/{id}/reabrir

Para exclusão, avaliar se deve existir no MVP. Se existir, não permitir violação de histórico importante.

==================================================
12. CATEGORIAS DE DESPESA
==================================================

Criar:

CategoriaDespesa

Campos:

- Id
- Nome
- PerfilId
- Ativo
- CriadoEm

Categorias são dados configuráveis, não enum fixo.

Exemplos:

- Moradia
- Alimentação
- Transporte
- Saúde
- Educação
- Lazer

Implementar CRUD adequado, preferindo desativação a exclusão quando já houver histórico relacionado.

Não tratar:

- Conta Fixa
- Conta Dividida

como categorias.

"Fixa" está relacionada à recorrência.

"Dividida" está relacionada ao compartilhamento da despesa.

Esses conceitos não fazem parte do primeiro MVP.

==================================================
13. RECEITAS
==================================================

Criar entidade:

Receita

Campos recomendados:

- Id
- Descricao
- Valor decimal
- DataRecebimento DateOnly
- Competencia DateOnly
- Observacoes?
- PerfilId
- CriadoEm

Implementar:

GET    /api/perfis/{perfilId}/receitas?mes=7&ano=2026
POST   /api/perfis/{perfilId}/receitas
PUT    /api/perfis/{perfilId}/receitas/{id}
DELETE /api/perfis/{perfilId}/receitas/{id}

Toda receita pertence obrigatoriamente a um perfil autorizado.

==================================================
14. CARTOES DE CREDITO
==================================================

Criar:

CartaoCredito

Campos:

- Id
- Nome
- Banco
- Bandeira
- Limite decimal
- DiaFechamento
- DiaVencimento
- Cor?
- Ativo
- PerfilId
- CriadoEm

Validações:

- limite não negativo;
- dia de fechamento válido;
- dia de vencimento válido.

Quando um dia configurado não existir em um determinado mês, utilizar o último dia válido daquele mês.

Exemplo:

Dia configurado: 31
Fevereiro de 2027:
usar 28/02/2027.

Implementar:

GET    /api/perfis/{perfilId}/cartoes
POST   /api/perfis/{perfilId}/cartoes
PUT    /api/perfis/{perfilId}/cartoes/{id}
PATCH  /api/perfis/{perfilId}/cartoes/{id}/ativar
PATCH  /api/perfis/{perfilId}/cartoes/{id}/desativar

==================================================
15. COMPRAS NO CARTAO
==================================================

Criar:

CompraCartao

Campos:

- Id
- Descricao
- ValorTotal decimal
- DataCompra DateOnly
- QuantidadeParcelas
- CartaoCreditoId
- PerfilId
- CriadoEm

Criar:

ParcelaCartao

Campos:

- Id
- CompraCartaoId
- FaturaCartaoId
- NumeroParcela
- QuantidadeParcelas
- Valor decimal

Exemplo:

Notebook
R$ 6.000,00
12 parcelas

Gerar:

1/12 = 500,00
2/12 = 500,00
...
12/12 = 500,00

Para valores inexatos:

R$ 100,00 / 3

Gerar exatamente:

33,33
33,33
33,34

A soma final das parcelas deve ser exatamente igual ao valor total.

Preferir fazer a distribuição internamente por centavos inteiros para evitar inconsistências.

Criar testes automatizados para esta regra.

==================================================
16. FATURAS
==================================================

Criar:

FaturaCartao

Campos recomendados:

- Id
- CartaoCreditoId
- MesReferencia
- DataFechamento
- DataVencimento
- Status
- CriadoEm

Uma compra pertence à primeira fatura cujo fechamento ainda não ocorreu na data da compra.

Exemplo:

Cartão:
Fechamento dia 10.
Vencimento dia 17.

Compra em 09/07/2026:
pertence à fatura que fecha em 10/07/2026.

Compra em 11/07/2026:
pertence à próxima fatura, fechando em 10/08/2026.

Parcelas futuras devem cair nas respectivas faturas mensais.

A geração de:

- compra;
- parcelas;
- faturas necessárias;
- associações;

deve ser transacional.

Tudo é salvo ou nada é salvo.

Não permitir estado parcial.

==================================================
17. REGRA CONTRA DUPLA CONTAGEM DE CARTAO
==================================================

Não contar uma compra e depois contar novamente a fatura como outra despesa.

Exemplo incorreto:

Compra no cartão:
R$ 500

Fatura:
R$ 500

Total:
R$ 1.000

Isso está errado.

A fatura é agrupamento das parcelas.

No dashboard, considerar as parcelas/despesas do cartão no período sem duplicar o valor da fatura como uma segunda despesa independente.

==================================================
18. ALTERACAO E EXCLUSAO DE COMPRAS PARCELADAS
==================================================

Para o primeiro MVP:

Enquanto nenhuma parcela estiver em fatura paga ou bloqueada, permitir:

- editar toda a compra;
- excluir toda a compra.

Não implementar inicialmente:

- excluir somente uma parcela intermediária;
- alterar somente uma parcela;
- alterar esta e futuras;
- recálculos complexos de parcelas já consolidadas.

Criar regras que impeçam inconsistência no histórico.

==================================================
19. DASHBOARD
==================================================

Implementar:

GET /api/perfis/{perfilId}/dashboard?mes=7&ano=2026

Retornar pelo menos:

- total de receitas do mês;
- total de despesas do mês;
- saldo mensal;
- total de contas pagas;
- total de contas pendentes;
- total de contas atrasadas;
- valor das faturas dos cartões;
- próximos vencimentos;
- comparação receitas x despesas;
- distribuição de despesas por categoria;
- evolução financeira mensal.

Garantir que todos os dados pertençam exclusivamente ao perfil autorizado.

Não persistir uma entidade Dashboard.

Dashboard deve ser composto por consultas e agregações.

==================================================
20. ARQUITETURA DO BACKEND
==================================================

Manter:

ControleFinanceiro
ControleFinanceiro.Application
ControleFinanceiro.Domain
ControleFinanceiro.Infrastructure

Responsabilidades:

ControleFinanceiro:

- Controllers;
- HTTP;
- configuração da aplicação;
- autenticação;
- middlewares;
- composição de dependências.

Application:

- casos de uso;
- DTOs;
- contratos/interfaces necessários;
- validação de entrada;
- orquestração.

Domain:

- entidades;
- enums;
- regras essenciais;
- invariantes;
- comportamentos do domínio.

Infrastructure:

- EF Core;
- PostgreSQL;
- DbContext;
- configurações;
- migrations;
- implementações de persistência;
- JWT;
- hashing;
- serviços externos.

Não adicionar repositories genéricos desnecessários.

Não criar:

IRepository<T>
GenericRepository<T>

apenas por padrão arquitetural.

Criar abstrações somente quando houver necessidade real.

Não implementar CQRS, MediatR, event bus ou mensageria sem benefício técnico concreto.

Manter simplicidade.

==================================================
21. PADRAO DE ERROS
==================================================

Implementar tratamento global de exceções.

Retornar ProblemDetails ou padrão equivalente consistente.

Usar códigos HTTP adequados:

200
201
204
400
401
404
409
422 quando apropriado

Não retornar stack traces para o cliente.

Para email duplicado:

409 Conflict

Para credenciais inválidas:

401 Unauthorized

Para recurso inexistente ou não autorizado dentro de outro perfil:

preferir 404.

==================================================
22. VALIDACOES
==================================================

Aplicar validação em camadas apropriadas.

Domain:

- invariantes essenciais.

Application:

- formato dos dados;
- tamanho;
- regras do caso de uso.

Banco:

- unicidade;
- integridade referencial;
- campos obrigatórios;
- precisão;
- índices.

Evitar duplicação excessiva e validações contraditórias.

Não adicionar biblioteca de validação sem necessidade clara.

Se usar FluentValidation, use de forma consistente e justificada.

==================================================
23. DATAS
==================================================

Timestamps técnicos:

- usar UTC.

Exemplos:

CriadoEm
AtualizadoEm, caso exista.

Datas civis financeiras:

- usar DateOnly quando apropriado.

Exemplos:

DataVencimento
DataPagamento
DataCompra
DataRecebimento
Competencia

Não converter indiscriminadamente datas civis para UTC.

==================================================
24. FRONTEND
==================================================

Criar o frontend em:

frontend/

Stack:

- React
- TypeScript
- Vite
- React Router
- TanStack Query
- React Hook Form
- Zod

Adicionar bibliotecas somente quando houver necessidade real.

Para gráficos, pode utilizar Recharts.

Não adicionar Redux sem necessidade.

Usar TanStack Query para estado vindo da API.

Usar estado React/context somente para estado verdadeiramente global do cliente, como:

- usuário autenticado;
- perfil selecionado;
- preferências necessárias.

Criar interface:

- moderna;
- minimalista;
- responsiva;
- desktop e mobile.

Desktop:

- sidebar.

Mobile:

- navegação adaptada.

==================================================
25. TELAS DO FRONTEND
==================================================

Criar pelo menos:

- Login
- Cadastro
- Seleção de perfil
- Dashboard
- Controle de despesas
- Receitas
- Cartões
- Configurações

Sidebar:

- Resumo
- Controle de Contas
- Receitas
- Configurações

Cartões podem aparecer dentro de Controle de Contas ou possuir seção própria se isso melhorar a experiência.

==================================================
26. SELECAO DE PERFIL
==================================================

Após login:

Se o usuário tiver zero perfis:

- direcionar para criação do primeiro perfil.

Se tiver um perfil:

- selecioná-lo automaticamente.

Se tiver mais de um:

- mostrar tela de seleção.

Permitir trocar de perfil sem logout.

Persistir a seleção no cliente de maneira segura e apropriada.

Nunca usar a seleção do frontend como autorização.

Backend sempre valida propriedade.

==================================================
27. PAGINA CONTROLE DE CONTAS
==================================================

Ter seletor mensal:

< Junho de 2026 >

Permitir:

- mês anterior;
- próximo mês;
- seleção de período.

Ao trocar período:

- atualizar os dados usando TanStack Query;
- tratar loading;
- tratar erros;
- evitar estados inconsistentes.

Mostrar despesas:

- pagas;
- pendentes;
- atrasadas.

Permitir:

- criar;
- editar;
- marcar como paga;
- reabrir.

Usar modal para cadastros rápidos quando fizer sentido.

==================================================
28. PAGINA RECEITAS
==================================================

Também organizada por mês e ano.

Permitir:

- criar;
- editar;
- excluir;
- listar.

Usar modal para cadastro e edição quando apropriado.

==================================================
29. CARTOES NO FRONTEND
==================================================

Permitir:

- cadastrar cartão;
- editar;
- desativar;
- consultar limite;
- visualizar compras;
- visualizar fatura;
- adicionar compra;
- compra à vista;
- compra parcelada.

Modais devem ter:

- validação;
- loading;
- feedback;
- prevenção de envio duplicado.

==================================================
30. DESIGN
==================================================

Criar design moderno e limpo.

Não usar uma interface exageradamente carregada.

Usar:

- espaçamento consistente;
- boa hierarquia visual;
- cards apenas quando agregarem valor;
- tabelas responsivas;
- feedback visual;
- empty states;
- skeleton/loading;
- mensagens de erro claras;
- confirmação para ações destrutivas.

Evitar dependência de uma biblioteca visual pesada inicialmente.

CSS moderno, organizado e reutilizável.

Pode usar CSS Modules ou abordagem equivalente simples.

Ícones podem usar Lucide React se houver benefício.

==================================================
31. TESTES
==================================================

Criar projetos de testes se ainda não existirem.

Utilizar xUnit.

Cobrir prioritariamente:

- criação inválida de Usuario;
- normalização de email;
- Perfis com UsuarioId vazio;
- isolamento entre usuários e perfis;
- divisão de parcelas;
- soma exata de centavos;
- cálculo de fatura considerando fechamento;
- meses com menos dias;
- compra parcelada transacional;
- endpoints protegidos;
- email duplicado.

Criar testes unitários para regras puras.

Criar testes de integração para fluxos importantes.

Executar todos os testes antes de considerar o trabalho concluído.

==================================================
32. MIGRATIONS
==================================================

A InitialCreate já existe e já foi aplicada.

NÃO:

- editar;
- remover;
- recriar;
- renomear.

Para novas entidades:

- criar novas migrations incrementais.

Usar nomes descritivos.

Exemplos:

AdicionarAutenticacao
AdicionarCategoriasEDespesas
AdicionarReceitas
AdicionarCartoesCredito

Antes de aplicar uma migration:

- inspecionar o que foi gerado;
- garantir ausência de exclusões ou alterações destrutivas não planejadas.

==================================================
33. SEGURANCA
==================================================

Garantir:

- JWT validado corretamente;
- assinatura;
- emissor;
- audiência;
- expiração;
- refresh token seguro;
- proteção dos endpoints;
- isolamento de perfis;
- consultas filtradas pelo usuário autenticado;
- ausência de SenhaHash em respostas;
- ausência de segredos em logs;
- ausência de segredos no Git;
- CORS restrito corretamente;
- validação dos dados;
- rate limiting em autenticação, se adequado;
- proteção contra enumeração excessiva;
- parâmetros SQL sempre por EF Core ou mecanismo seguro;
- não construir SQL inseguro.

==================================================
34. LOGGING
==================================================

Utilizar logging estruturado do ASP.NET Core.

Registrar:

- falhas inesperadas;
- eventos importantes de autenticação sem dados sensíveis;
- falhas de persistência.

Não logar:

- senha;
- hash de senha;
- access token completo;
- refresh token completo;
- connection string com senha.

==================================================
35. FRONTEND E TOKENS
==================================================

Implementar estratégia segura.

Preferência:

- Access Token de curta duração.
- Refresh Token em cookie HttpOnly.
- Rotação de refresh token.
- Endpoint de refresh.
- Revogação no logout.

Configurar corretamente:

- HttpOnly;
- Secure em ambientes apropriados;
- SameSite;
- CORS;
- credentials.

Evitar colocar refresh token no localStorage.

==================================================
36. NAO IMPLEMENTAR NO PRIMEIRO MVP
==================================================

Não implementar agora, salvo se for necessário para uma dependência crítica:

- microserviços;
- Kubernetes;
- Kafka;
- RabbitMQ;
- Open Finance;
- integração bancária;
- IA;
- previsões financeiras;
- aplicativo mobile nativo;
- despesas divididas entre pessoas;
- perfis compartilhados por múltiplos usuários;
- múltiplas moedas com conversão;
- recorrência automática complexa;
- exportação PDF;
- exportação Excel;
- orçamento;
- metas financeiras;
- notificações externas.

==================================================
37. ORDEM DE IMPLEMENTACAO
==================================================

Antes de alterar qualquer coisa:

1. Inspecione o repositório.
2. Execute dotnet build.
3. Execute testes existentes.
4. Confira migrations existentes.
5. Confira pacotes e versões.
6. Não atualize versões arbitrariamente.

Depois implemente nesta ordem:

FASE A — Autenticação e usuários

- contratos;
- hashing;
- refresh tokens;
- JWT;
- cadastro;
- login;
- refresh;
- logout;
- me;
- testes.

FASE B — Perfis

- CRUD necessário;
- autorização;
- isolamento;
- testes.

FASE C — Categorias e despesas

- domínio;
- mapeamentos;
- migration;
- Application;
- endpoints;
- testes.

FASE D — Receitas

- domínio;
- migration;
- Application;
- endpoints;
- testes.

FASE E — Dashboard

- consultas;
- agregações;
- endpoint;
- testes.

FASE F — Cartões

- CartaoCredito;
- CompraCartao;
- ParcelaCartao;
- FaturaCartao;
- regras de fechamento;
- parcelamento;
- transações;
- testes críticos.

FASE G — Frontend

Pode começar após autenticação e perfis estarem funcionais e evoluir em paralelo com os demais módulos.

FASE H — Revisão

- segurança;
- performance;
- índices;
- N+1;
- tracking desnecessário;
- paginação onde necessário;
- tratamento de erros;
- logs;
- build;
- testes.

==================================================
38. REGRAS PARA ENTITY FRAMEWORK
==================================================

Para consultas somente leitura:

- considerar AsNoTracking.

Evitar Include excessivo.

Projetar DTOs adequados.

Não retornar entidades EF diretamente pela API.

Usar CancellationToken em operações assíncronas relevantes.

Utilizar métodos assíncronos:

- ToListAsync;
- FirstOrDefaultAsync;
- SingleOrDefaultAsync;
- SaveChangesAsync.

Configurar índices de acordo com consultas reais.

Configurar decimal explicitamente.

Configurar relacionamentos explicitamente quando necessário.

==================================================
39. REGRAS PARA API
==================================================

Controllers devem ser finos.

Não colocar regras complexas dentro de Controllers.

Fluxo esperado:

HTTP
→ Controller
→ Application
→ Domain
→ Infrastructure

Não expor:

- entidades diretamente;
- SenhaHash;
- detalhes internos;
- stack traces.

Usar DTOs de request e response.

==================================================
40. DOCUMENTACAO DA API
==================================================

Manter Swagger/OpenAPI funcionando.

Documentar autenticação Bearer.

Permitir autenticação pelo Swagger.

Não introduzir pacotes vulneráveis.

Antes de concluir:

dotnet list package --vulnerable --include-transitive

Corrigir vulnerabilidades reais encontradas sem atualizar versões de forma imprudente.

==================================================
41. BUILD E QUALIDADE
==================================================

Após cada mudança relevante:

- executar dotnet build;
- corrigir warnings relevantes;
- executar testes relacionados.

Antes de finalizar:

dotnet build

dotnet test

dotnet list package --vulnerable --include-transitive

Frontend:

npm run build

Executar lint, caso configurado.

Não considerar concluído enquanto houver:

- build quebrada;
- testes falhando;
- migrations incoerentes;
- vulnerabilidades altas conhecidas sem tratamento;
- segredos expostos.

==================================================
42. GIT
==================================================

Não apagar histórico.

Não executar comandos destrutivos como:

git reset --hard

sem necessidade explícita.

Não sobrescrever trabalho válido do usuário.

Não commitar segredos.

Garantir .gitignore adequado para:

- bin/
- obj/
- node_modules/
- .env com segredos;
- arquivos locais sensíveis.

Não realizar commits automaticamente, salvo se explicitamente solicitado.

==================================================
43. COMPORTAMENTO AUTONOMO
==================================================

Não pare para explicar cada arquivo.

Não me ensine os conceitos.

Não pergunte autorização a cada etapa.

Analise, implemente, teste e corrija.

Somente faça uma pergunta quando existir um bloqueio real, como:

- credencial externa indispensável;
- decisão de produto impossível de inferir;
- operação destrutiva irreversível;
- conflito real entre requisitos.

Caso uma decisão possa ser tomada com segurança a partir deste prompt e do código existente, tome a decisão e continue.

==================================================
44. CRITERIO DE CONCLUSAO
==================================================

O MVP deve possuir funcionalmente:

Backend:

- cadastro;
- login;
- JWT;
- refresh token;
- logout;
- usuário autenticado;
- perfis;
- seleção de perfil;
- isolamento entre usuários;
- categorias;
- despesas;
- receitas;
- dashboard;
- cartões;
- compras à vista;
- compras parceladas;
- parcelas;
- faturas;
- regras de fechamento;
- regras de centavos;
- tratamento global de erros;
- Swagger;
- testes.

Frontend:

- login;
- cadastro;
- fluxo de perfis;
- dashboard;
- despesas;
- receitas;
- cartões;
- configurações básicas;
- layout responsivo;
- proteção de rotas;
- loading;
- erros;
- formulários validados;
- modais;
- integração completa com API.

==================================================
45. INSTRUCAO FINAL
==================================================

Comece agora.

Primeiro:

- inspecione todo o repositório;
- identifique o estado real atual;
- preserve tudo o que estiver correto;
- execute a build atual;
- execute os testes existentes;
- confira migrations;
- continue da implementação atual.

Não recrie o projeto.

Não modifique a InitialCreate já aplicada.

Implemente o sistema incrementalmente, mas de forma autônoma e contínua.

Corrija seus próprios erros durante a execução.

Não entregue somente sugestões ou exemplos.

Edite efetivamente os arquivos do workspace.

Ao final, responda apenas com:

1. resumo objetivo do que foi implementado;
2. principais arquivos criados ou modificados;
3. migrations adicionadas;
4. testes executados e resultado;
5. builds executadas e resultado;
6. pendências reais, caso existam.