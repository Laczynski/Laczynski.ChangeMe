# Laczynski ChangeMe Full-Stack Template

`Laczynski.ChangeMe` is a full-stack **starter template** for `dotnet new` — architecture, tooling, docs workflow, and tests you reuse; a small issue-tracking **sample app** shows the patterns in context. Generated solutions use the `ChangeMe` token (`dotnet new changeme -n YourApp`).

It generates:

- an Angular frontend
- a layered ASP.NET backend
- **PostgreSQL** for EF Core, Hangfire, integration tests, and Docker Compose
- MailHog for local email capture
- backend unit and integration test projects
- Playwright E2E smoke tests (Chromium via `npm run install:frontend`)
- `docs/` with system/module ownership, architecture diagrams, requirements workflow (`FR-*` specs, validation script), and `AGENTS.md`
- layered backend (Web → UseCases → Domain → Infrastructure), JWT + RBAC reference, API `/api/v1`, Mediator + FastEndpoints

Production Docker stack uses **same-origin** `/api/v1` through nginx; see `docs/system/operations/deployment.md`.

## Install

```powershell
dotnet new install Laczynski.ChangeMe
```

## Create a project

```powershell
dotnet new changeme -n IssuesDemo -o IssuesDemo
```

## After generation

- Start the API once in Development (migrations apply on startup), then see `docs/system/operations/local-stack.md` and `docs/modules/backend/operations/persistence.md`.

## Verify the install

```powershell
dotnet new list changeme
```

## Update

```powershell
dotnet new install Laczynski.ChangeMe --force
```

## Uninstall

```powershell
dotnet new uninstall Laczynski.ChangeMe
```

## Source repository

https://github.com/Laczynski/Template
