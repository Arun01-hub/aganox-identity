# Aganox Identity Server

Central OpenID Connect (OIDC) and OAuth 2.0 authority for Aganox products. Built with **ASP.NET Core 10**, **OpenIddict 7.5**, and **ASP.NET Core Identity**, with Razor Pages for login, logout, and consent.

## Features

- Authorization Code flow with **PKCE required** for public clients
- Refresh tokens and **Client Credentials** for service clients
- Signed JWT access tokens (encryption disabled for compatibility with JWT bearer validation)
- EF Core persistence for users, roles, and OpenIddict stores
- Automatic **EF migrations and development seed** on startup
- **Application Registry** (business onboarding) separate from the OpenIddict client store
- Admin UI and REST API for application lifecycle, credentials, and audit
- Dynamic CORS: configured origins plus Active SPA applications from the registry

AI-assisted development: see [AGENTS.md](AGENTS.md) and [Aganox AI standards](https://github.com/Arun01-hub/aganox-standards/blob/main/docs/AI_DRIVEN_DEVELOPMENT.md).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- **Development (default launch profile):** no separate database install — uses SQLite (`aganox-identity.db` under the host project)
- **Non-Development / production-style runs:** SQL Server (default connection string uses LocalDB)

Trust the ASP.NET Core dev HTTPS certificate if browsers or SPAs reject `https://localhost:7036`:

```powershell
dotnet dev-certs https --trust
```

## Run locally

From the repository root:

```powershell
.\scripts\build.ps1
.\scripts\run.ps1
```

`run.ps1` starts the host with the **https** launch profile (`ASPNETCORE_ENVIRONMENT=Development`).

Equivalent manual commands:

```powershell
dotnet build Aganox.Identity.sln
dotnet run --project src\Aganox.IdentityServer\Aganox.IdentityServer.csproj --launch-profile https
```

### URLs

| URL | Purpose |
|-----|---------|
| `https://localhost:7036` | HTTPS issuer (configured in `Identity:Issuer`) |
| `http://localhost:5110` | HTTP (also bound when using the `https` profile) |

OIDC metadata: `https://localhost:7036/.well-known/openid-configuration`

### OIDC endpoints

| Endpoint | Path |
|----------|------|
| Authorization | `/connect/authorize` |
| Token | `/connect/token` |
| UserInfo | `/connect/userinfo` |
| End session (logout) | `/connect/logout` |

Interactive UI: `/Account/Login`, `/Account/Logout`, `/Consent`.

## Configuration

| Setting | Location | Notes |
|---------|----------|--------|
| Database | `ConnectionStrings:IdentityDb` | Development override in `appsettings.Development.json` → SQLite |
| Issuer | `Identity:Issuer` | Default `https://localhost:7036` |
| Post-login redirect hint | `Identity:DefaultClientUrl` | Default `http://rental.aganox.local:4200` |
| SPA CORS | `Cors:AllowedOrigins` | Base list; merged with Active SPA `AllowedOrigins` from the registry |
| Application onboarding | `ApplicationRegistry` | Client ID prefix, production redirect host allowlist, HTTPS rules |

Provider selection is automatic: connection strings containing `Data Source=` (without `Initial Catalog=`) use **SQLite**; otherwise **SQL Server**.

Local secrets: use `appsettings.Development.local.json` (gitignored) or user secrets — do not commit client secrets.

## Development seed data

On startup, the seeder applies migrations, ensures the `User` role, registers scopes and clients, and creates an admin user if missing.

| | |
|---|---|
| Email | `admin@aganox.local` |
| Password | `ChangeMe123!` |
| Roles | `User`, `Administrator` (admin UI / API) |

## Scopes

| Scope | Purpose |
|-------|---------|
| `openid`, `profile`, `email`, `offline_access` | Standard OIDC / refresh |
| `aganox.api` | Access to Aganox API resources (resource name: `aganox.api`) |

Interactive clients use **explicit consent**.

## Registered clients (development seed)

| Client ID | Type | Grants | Notes |
|-----------|------|--------|--------|
| `aganox-rental` | Public | Authorization Code, Refresh Token | SPA — redirect/logout URIs for `localhost:4200` and `http://rental.aganox.local:4200` |
| `aganox-web` | Public | Authorization Code, Refresh Token | Redirect URIs on this host (`/signin-oidc`) |
| `aganox-service` | Confidential | Client Credentials | Secret: `dev-service-secret-change-in-production` |
| `aganox-rental-service` | Confidential | Client Credentials | Secret: `dev-rental-service-secret-change-in-production` |

Existing clients are updated on seed when new redirect or post-logout URIs are added; other metadata is not rewritten.

**Rental SPA integration:** use issuer `https://localhost:7036`, client ID `aganox-rental`, Authorization Code with PKCE, and scopes such as `openid`, `profile`, `email`, `offline_access`, `aganox.api`.

Dev seed clients are written to the **Application Registry** and provisioned into OpenIddict; production onboarding uses the admin tools below (not the dev bootstrap).

## Application Registry and onboarding

The registry stores business metadata (type, environment, status, URIs, scopes, hashed credentials, audit history). OpenIddict remains the protocol/runtime store; provisioning is performed only from Infrastructure.

### Application types

| Type | OAuth profile |
|------|----------------|
| `SpaPublic` | Authorization Code + PKCE (public client) |
| `WebConfidential` | Authorization Code (confidential client, client secret) |
| `ServiceM2m` | Client Credentials (confidential, client secret) |

### Lifecycle statuses

`Pending` → `Active` → (`Disabled` | `Revoked`) → `Archived`

Only **Active** applications may use `/connect/authorize` and `/connect/token`.

### Admin UI (Administrator role)

Sign in as `admin@aganox.local`, then open:

| Page | URL |
|------|-----|
| Application list | `/Admin/Applications` |
| Register | `/Admin/Applications/Create` |
| Details / edit / lifecycle / credentials / audit | `/Admin/Applications/Details/{id}` (+ linked pages) |

Confidential client secrets are shown **once** after create or rotate; the registry stores only hashes.

### Admin REST API

Base path: `/api/admin/applications` (cookie auth, `Administrator` role).

Supports list/filter, register, metadata and URI/origin/scope updates, status changes, secret rotate/revoke, and audit queries.

### Redirect URI policy (production)

Production redirect URIs must use HTTPS unless the host is listed under `ApplicationRegistry:DevelopmentRedirectHostExceptions`. The host must also match `ApplicationRegistry:ProductionAllowedRedirectHosts` (exact host or suffix match; entries like `*.aganox.com` are supported).

### Environment-specific apps

Each registry row has a `DeploymentEnvironment` (`Development`, `Staging`, `Production`). Client IDs are generated as `{prefix}-{slug}-{env}` unless an explicit ID is allowed (Development only by default).

## Tests

```powershell
.\scripts\test.ps1
```

## Solution layout

Clean architecture split across four projects under `src/`:

```
Aganox.Identity.sln
src/
  Aganox.IdentityServer/                 # Web host — OIDC controller, Razor UI, pipeline
  Aganox.IdentityServer.Application/     # Use-case models and abstractions
  Aganox.IdentityServer.Domain/          # Users, scopes, policies, claim constants
  Aganox.IdentityServer.Infrastructure/  # EF Core, Identity, OpenIddict, migrations, seeding
scripts/
  build.ps1                              # dotnet build (Debug by default; -Configuration Release)
  run.ps1                                # dotnet run with https profile
  test.ps1                               # dotnet test
tests/
  Aganox.IdentityServer.Tests/           # unit + integration tests
```

EF migrations live in `Aganox.IdentityServer.Infrastructure/Migrations/`. Design-time tooling uses SQLite via `AuthDbContextFactory`.

### Optional: apply migrations manually

Normally unnecessary because startup seeding runs `MigrateAsync`. If needed:

```powershell
dotnet ef database update `
  --project src\Aganox.IdentityServer.Infrastructure `
  --startup-project src\Aganox.IdentityServer
```

## Production notes

Replace development signing/encryption certificates (OpenIddict `AddDevelopment*` helpers), use strong client secrets, configure a production connection string and issuer URL, and restrict CORS origins. Signing certificates and secrets must not be committed.
