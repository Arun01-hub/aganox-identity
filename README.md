# Aganox Identity Server

OpenIddict-based authentication server for Aganox products (Rental, future apps).

## Prerequisites

- .NET 10 SDK

## Run locally

```powershell
.\scripts\build.ps1
.\scripts\run.ps1
```

Default URLs (from launch profile):

| URL | Purpose |
|-----|---------|
| `https://localhost:7036` | Identity / OIDC issuer |
| `http://localhost:5110` | HTTP fallback |

## Dev seed user

| | |
|---|---|
| Email | `admin@aganox.local` |
| Password | `ChangeMe123!` |

## Registered clients

Rental SPA client ID: `aganox-rental` (Authorization Code + PKCE).

## Solution layout

```
src/
  Aganox.IdentityServer/              # Host (Razor login/consent UI)
  Aganox.IdentityServer.Application/
  Aganox.IdentityServer.Domain/
  Aganox.IdentityServer.Infrastructure/
scripts/                              # build, run
```

## Related repositories

| Repo | Relationship |
|------|----------------|
| [Aganox-Rental](../Aganox-Rental) | OIDC client — requires this server running |
| [Aganox](../Aganox) (LMS) | Standalone JWT auth — does not use Identity Server |
