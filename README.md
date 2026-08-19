# Controle Financeiro

Aplicação de controle financeiro com API ASP.NET Core, frontend React e PostgreSQL.

## Executar com Docker

Pré-requisito: Docker Desktop em execução.

Na raiz do repositório, execute:

```powershell
docker compose up --build
```

Na primeira execução, o Compose baixa as imagens, compila a aplicação, cria o banco e aplica as migrations automaticamente.

- Aplicação: http://localhost:5173
- API: http://localhost:5175
- Saúde da API: http://localhost:5175/health

Para encerrar:

```powershell
docker compose down
```

Os dados do PostgreSQL ficam preservados no volume Docker `controlefinanceiro_pgdata`. O Compose reutiliza esse volume caso ele já tenha sido criado pela execução manual anterior.

Se ainda existir o container manual `controlefinanceiro-postgres`, mantenha-o parado enquanto estiver usando o Compose, pois ele utiliza a mesma porta e o mesmo volume.

Para também apagar o banco local:

```powershell
docker compose down --volumes
```

## Configuração opcional

Os valores padrão são adequados ao desenvolvimento local. Para personalizá-los, copie `.env.example` para `.env` antes de iniciar:

```powershell
Copy-Item .env.example .env
```

Ao alterar `API_PORT`, atualize também `VITE_API_URL`. Ao alterar `FRONTEND_PORT`, atualize também `FRONTEND_ORIGIN`.

## Executar sem Docker

O frontend espera a API em `http://localhost:5175`. Para a execução manual, configure `ConnectionStrings:DefaultConnection` com User Secrets, aplique as migrations do Entity Framework e inicie API e frontend separadamente.
