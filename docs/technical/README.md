# Technical documentation

> **Scope:** run, configure, and troubleshoot the application and its toolchain — not how to write feature code.
>
> For implementation conventions, see [`docs/guides/`](../guides/). For product behaviour (`FR-*`), see [`docs/requirements/`](../requirements/).
>
> **Adding or changing technical docs:** [technical-documentation-guide.md](technical-documentation-guide.md).

## Documents

| Document                                                             | Read when you need to…                                                           |
| -------------------------------------------------------------------- | -------------------------------------------------------------------------------- |
| [technical-documentation-guide.md](technical-documentation-guide.md) | Rules for when to add or extend docs in this folder                              |
| [database-and-docker.md](database-and-docker.md)                     | EF Core migrations, Docker Compose, Hangfire jobs, file storage                  |
| [deployment.md](deployment.md)                                       | Runtime API URL, production checklist, TLS, CORS, split-host deployment          |
| [data-generator.md](data-generator.md)                               | Fill the database with optional demo users and issues after migrations           |
| [ci.md](ci.md)                                                       | Understand GitHub Actions, reproduce CI locally, or debug a failing pipeline job |
| [publishing.md](publishing.md)                                       | Release the `Template` NuGet template on tag push                              |

## Start here by task

| Task                                                     | Document                                                                                                              |
| -------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| First clone — run full stack locally                     | [database-and-docker.md](database-and-docker.md) → `npm run docker:up`                                                |
| First clone — run API/tests without Compose              | [database-and-docker.md](database-and-docker.md) → `docs/guides/testing-guidelines.md`                                |
| Populate UI with sample data                             | [data-generator.md](data-generator.md)                                                                                |
| Deploy to production or split API / SPA hosts            | [deployment.md](deployment.md)                                                                                        |
| PR failed on GitHub                                      | [ci.md](ci.md)                                                                                                        |
| Publish a new template version                           | [publishing.md](publishing.md)                                                                                        |
| Configure JWT, password policy, or initial administrator | [database-and-docker.md](database-and-docker.md) (`AuthOptions`, `InitialAdministratorOptions` in `appsettings.json`) |
| Email delivery in local or deployed environments         | [database-and-docker.md](database-and-docker.md) (MailHog) + `EmailOptions` in `appsettings.json`                     |
| Background jobs failing or not running                   | [database-and-docker.md](database-and-docker.md) (Hangfire dashboard, cron)                                           |

## Relationship to other docs

| Folder               | Use for                                            |
| -------------------- | -------------------------------------------------- |
| `docs/guides/`       | How to _implement_ (code, tests, recipes)          |
| `docs/requirements/` | What the product _must do_ (`FR-*`, `_shared/`)    |
| `docs/technical/`    | How to _run and configure_ the stack (this folder) |

Auth behaviour: `docs/requirements/functional/identity/` and `docs/requirements/functional/access/`. Runtime settings: `AuthOptions` and `InitialAdministratorOptions` in `appsettings.json` (see [database-and-docker.md](database-and-docker.md)).

Commands shared across areas: [`AGENTS.md`](../../AGENTS.md).
