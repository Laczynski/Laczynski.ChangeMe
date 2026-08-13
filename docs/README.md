# Documentation index

> Start with [`AGENTS.md`](../AGENTS.md) for commands and task-based reading order. Documentation ownership, naming, and content rules: [documentation rules](documentation-rules.md).

## Start by task

| Task | Read |
| --- | --- |
| Understand repository ownership | [Repository map](system/development/repository-map.md) |
| Implement a cross-module feature | Target `FR-*` → [cross-module feature workflow](system/development/feature-workflow.md) → relevant module development doc |
| Change frontend code | [Frontend development](modules/frontend/development.md) |
| Understand frontend session renewal | [Auth session lifecycle](modules/frontend/architecture/auth-session-lifecycle.md) |
| Change backend code or add an endpoint | [Backend development](modules/backend/development.md) |
| Select test layers | [Testing strategy](system/development/testing-strategy.md) |
| Add or debug browser journeys | [E2E testing](system/development/e2e-testing.md) |
| Run the full stack locally | [Local full-stack environment](system/operations/local-stack.md) |
| Change EF Core or PostgreSQL | [Backend persistence](modules/backend/operations/persistence.md) |
| Operate Hangfire jobs | [Background jobs](modules/backend/operations/background-jobs.md) |
| Operate attachment storage | [File storage](modules/backend/operations/file-storage.md) |
| Generate sample data | [Demo data](modules/backend/demo-data.md) |
| Deploy the system | [Deployment](system/operations/deployment.md) |
| Reproduce CI | [Continuous integration](system/operations/ci.md) |
| Publish the template | [Template publishing](system/operations/publishing.md) |
| Understand runtime configuration hardening | [Runtime configuration hardening](system/designs/runtime-configuration-hardening-design.md) |
| Design GitLab multi-environment delivery | [Multi-environment application delivery](system/designs/multi-environment-application-delivery-design.md) |
| Continue repository modularization | [Modular documentation migration](system/designs/modular-documentation-migration.md) |

## Ownership

| Location | Canonical responsibility |
| --- | --- |
| [`requirements/`](requirements/README.md) | Product domain, conventions, quality, and functional behaviour |
| [`system/`](system/) | Knowledge owned by the main repository or crossing module boundaries |
| [`modules/frontend/`](modules/frontend/) | Frontend implementation and internal architecture; moves with the frontend repository |
| [`modules/backend/`](modules/backend/) | Backend implementation, development tools, and operations; moves with the backend repository |

## Requirements workflow

Requirements remain an independent, validated structure:

- [Change process](requirements/requirements-change-process.md)
- [Authoring guide](requirements/requirements-authoring-guide.md)
- [Five-layer model](requirements/_shared/README.md)
- [Generated requirements index](requirements/README.md)

Run `npm run docs:validate` after any documentation change. It includes the specialized requirements validation and regenerates the requirements index.
