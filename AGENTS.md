# Aganox Identity — agent guide

**Catalog**: OIDC/OAuth authority for Aganox products · **Standards**: [AI_DRIVEN_DEVELOPMENT](https://github.com/Arun01-hub/aganox-standards/blob/main/docs/AI_DRIVEN_DEVELOPMENT.md)

## Purpose

OpenIddict + ASP.NET Core Identity host: login/consent UI, token issuance, **Application Registry** (SPA onboarding, redirect URIs, audit).

## Stack

- .NET 10, OpenIddict 7.5, EF Core (SQLite dev / SQL Server prod-style), Razor Pages, admin API/UI.

## Layout

```
src/Aganox.IdentityServer/          # Host
src/Aganox.IdentityServer.Application/
src/Aganox.IdentityServer.Domain/
src/Aganox.IdentityServer.Infrastructure/
tests/Aganox.IdentityServer.Tests/
scripts/build.ps1, run.ps1, test.ps1
```

## Commands

| Task | Command |
|------|---------|
| Build | `.\scripts\build.ps1` |
| Run | `.\scripts\run.ps1` → `https://localhost:7036` |
| Test | `.\scripts\test.ps1` or `dotnet test Aganox.Identity.sln` |

## Configuration

| File | Notes |
|------|-------|
| `src/Aganox.IdentityServer/appsettings.json` | Non-secret defaults |
| `appsettings.Development.local.json` | Gitignored — copy from `appsettings.Development.local.json.example` |

## Dependent apps

Rental (`aganox-rental`), LMS (OIDC rollout) — register clients via Application Registry or seed; PKCE required for public clients.

## Agent notes

- Do not disable PKCE or weaken token validation without explicit security review.
- Registry changes need migrations + admin UI/API parity.
- Dev seed users: see README (`admin@aganox.local`).