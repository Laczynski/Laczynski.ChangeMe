# Security policy

## Supported versions

Security fixes are provided for the **latest stable** [`ChangeMe`](https://www.nuget.org/packages/ChangeMe) template release published from this repository. Older template versions and forks generated with `dotnet new changeme` are maintained by their owners.

| Version | Supported |
| --- | --- |
| Latest `ChangeMe` NuGet release | Yes |
| Older template releases | Best effort |
| Generated applications | Maintained by the project that scaffolded them |

## Reporting a vulnerability

**Do not** open a public GitHub issue for security reports.

Preferred channel:

1. Open a **[private security advisory](https://github.com/Laczynski/Laczynski.ChangeMe/security/advisories/new)** on this repository, or
2. Email the maintainer listed in [`LICENSE`](LICENSE) with the subject prefix **`[ChangeMe security]`**.

Include:

- Affected component (template source, sample app, deployment scripts, documentation, etc.)
- Steps to reproduce or proof-of-concept
- Impact assessment (confidentiality, integrity, availability)
- Affected version or commit SHA when known

You should receive an acknowledgment within **7 days**. We will coordinate disclosure timing with you before publishing a fix or advisory.

## Out of scope

- Vulnerabilities in **third-party dependencies** already tracked by Dependabot — report upstream or wait for the weekly dependency PR unless exploitability in this template is demonstrated.
- Deployments you operate outside the documented Ansible/GitLab flow (custom secrets handling, public Swagger/Hangfire exposure, etc.) unless the template defaults are unsafe without extra configuration.
- Issues in **generated applications** that changed template defaults or removed documented hardening.

## Secure defaults reminder

Review before production:

- Keep `SwaggerOptions:Enabled` and `HangfireOptions:DashboardEnabled` disabled outside Development.
- Replace all placeholder secrets in `.env`, GitLab variables, and VPS `secrets.env`.
- Use protected GitLab environments and SSH file variables for deployment keys ([deployment](docs/system/operations/deployment.md)).
