FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ControleFinanceiro.Domain/ControleFinanceiro.Domain.csproj ControleFinanceiro.Domain/
COPY ControleFinanceiro.Application/ControleFinanceiro.Application.csproj ControleFinanceiro.Application/
COPY ControleFinanceiro.Infrastructure/ControleFinanceiro.Infrastructure.csproj ControleFinanceiro.Infrastructure/
COPY ControleFinanceiro/ControleFinanceiro.csproj ControleFinanceiro/
RUN dotnet restore ControleFinanceiro/ControleFinanceiro.csproj

COPY . .
RUN dotnet publish ControleFinanceiro/ControleFinanceiro.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install --yes --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ControleFinanceiro.dll"]
