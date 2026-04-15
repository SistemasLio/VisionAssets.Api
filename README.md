# VisionAssets.Api

API HTTP central do **VisionAssets** — recebe snapshots de inventário dos agentes Windows, autenticados com **Microsoft Entra ID** (JWT Bearer).

Repositório do **agente** (MSI, coleta, sync cliente): [github.com/SistemasLio/VisionAssets](https://github.com/SistemasLio/VisionAssets). Contrato JSON: [inventory-v1.openapi.yaml](https://github.com/SistemasLio/VisionAssets/blob/main/docs/contracts/inventory-v1.openapi.yaml).

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- App registration no Entra ID para esta API (ver abaixo)

## Configuração rápida (Entra ID)

1. **Registar aplicação (API)** no portal Entra ID → *App registrations* → New registration (contas **My organization only**).
2. Em **Expose an API** → *Add* → Application ID URI (ex.: `api://<api-client-id>`).
3. *Add a scope* (ex.: `access_as_user` ou um scope de aplicação conforme política interna) — o agente usa **application permissions** / scope `api://.../.default` com client credentials.
4. Em **API permissions** da aplicação **do agente** (cliente confidencial), adicionar permissão de aplicação à API exposta e **Grant admin consent**.
5. Copiar **Directory (tenant) ID**, **Application (client) ID** da **API** para `AzureAd:TenantId` e `AzureAd:ClientId` neste projeto.

> O valor de `AzureAd:Audience` deve coincidir com o **aud** do token emitido para a API (geralmente `api://<api-app-id>` ou o App ID URI definido).

## Execução local

```bash
cd src/VisionAssets.Api
dotnet user-secrets set "AzureAd:TenantId" "<tenant-guid>"
dotnet user-secrets set "AzureAd:ClientId" "<api-app-client-id>"
dotnet user-secrets set "AzureAd:Audience" "api://<api-app-client-id>"
dotnet run
```

- Swagger UI (Development): `https://localhost:<porta>/swagger`
- Health (sem auth): `GET /health`
- Inventário (com Bearer): `POST /v1/inventory-snapshots`

## Estado atual (MVP)

- Validação de JWT via **Microsoft.Identity.Web**
- Armazenamento **em memória** (`InventorySnapshotStore`) — substituir por base de dados em produção
- Idempotência pelo cabeçalho `Idempotency-Key`

## Build

```bash
dotnet build -c Release
```

Solução: `VisionAssets.Api.slnx`.
