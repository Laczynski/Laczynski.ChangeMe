# Documentation rules

> Type: reference
> Scope: repository documentation outside `docs/requirements/`
> Status: implemented
> Canonical for: document ownership, naming, structure, and maintenance rules

## Core rules

1. **One fact has one canonical source.** Link to it instead of copying it.
2. **Organize by owner first.** System-wide knowledge belongs in `docs/system/`; module knowledge belongs in `docs/modules/<module>/` until that module moves to its own repository.
3. **Keep requirements independent.** `docs/requirements/` defines product behaviour. Implementation documents may link to it but must not redefine it.
4. **Prefer a short document with a diagram or table.** Prose should explain decisions, exceptions, and consequences—not repeat visible code or diagram edges.
5. **Do not document one-off PR details.** Add a document only when the knowledge will help later implementation, operation, review, or repository extraction.

## Information types

| Type | Canonical question | Typical content |
| --- | --- | --- |
| `requirements` | What must the product do? | Domain, conventions, quality, `FR-*` |
| `architecture` | How does the implemented solution work? | Boundaries, flows, states, decisions, failure handling |
| `design` | How do we propose to change it? | Options, decision, migration, acceptance criteria |
| `development` | How do we change code safely? | Ownership, conventions, recipes, testing |
| `operations` | How do we run and maintain it? | Configuration, deployment, CI, runbooks, troubleshooting |
| `reference` | Where do I quickly find a stable mapping? | Repository map, catalogs, documentation rules |

Architecture describes current behaviour. A design describes a proposed change. After implementation, update the relevant architecture document and remove obsolete design detail; retain a short ADR only when the decision rationale still matters.

## Type contracts

These are minimum content boundaries, not templates. Omit optional sections instead of creating boilerplate.

| Type | Must make clear | Must not own |
| --- | --- | --- |
| `architecture` | Summary, Mermaid diagram, invariants or decisions, failure handling when relevant, code map, verification | Proposed work or copied product requirements |
| `design` | Goal, decision/target/plan, alternatives when material, migration steps, completion criteria | Current-state claims before implementation |
| `development` | Code ownership, enforceable do/do-not rules, nearest reference implementation, relevant checks | Product behaviour or deployment configuration |
| `operations` | Configuration source, procedure or runtime model, verification, failure/recovery guidance | Detailed coding conventions |
| `reference` | Stable mapping, lookup data, and its authoritative source | Long narrative or project planning |
| `README.md` | Task-to-document routing | Content copied from target documents |

Status is part of the contract:

| Type | Allowed status |
| --- | --- |
| `architecture` | `implemented`, `superseded` |
| `design` | `proposed`, `implemented`, `superseded` |
| `development`, `operations`, `reference` | `implemented`, `superseded` |

`Scope` must match ownership: `docs/system/` uses `system`; `docs/modules/<module>/` uses `<module>`. The validator enforces the scope/status matrix, requires `Summary`, Mermaid, and `Verification` for architecture, and requires `Goal` plus a decision/target/plan/phases section for designs. The remaining content boundaries require review because enforcing exact headings would encourage empty sections.

## When to create a document

Creating a file is the last option. Prefer, in order: keep code or configuration authoritative → link to it → extend the existing canonical document → create a document.

Create a document only when all statements are true:

- the knowledge remains useful after the current PR;
- code or configuration does not explain it quickly and unambiguously;
- it captures a non-trivial mechanism, decision, risk, repeated convention, or operational procedure;
- no existing document owns the concern;
- it has one precise `Canonical for` responsibility;
- omitting it creates a realistic risk of incorrect implementation, operation, or review.

Do not create documents for straightforward CRUD, catalogs visible in source or generated API documentation, copied configuration values, one-off PR notes, resolved incidents, or product behaviour already owned by requirements. Prefer a source comment for a small local exception.

Before adding a file, answer:

1. Why are code, a link, or an existing document insufficient?
2. What exactly will this document be canonical for?
3. Which change will require updating or removing it?

If any answer is unclear, do not create the document.

## When to split or merge

Split a document only when its parts have different owners, lifecycles, uses, or canonical questions. Length alone is not a reason to split.

Merge documents when they answer the same question, change together, have the same owner, or require repeated context when read separately.

> One document may describe several elements, but it must have one responsibility and one reason to change.

## When to remove a document

Remove a document when its mechanism no longer exists, another source becomes canonical, its useful content moves elsewhere, or the source now explains the concern unambiguously. Use `superseded` only when readers still need an explicit pointer to the replacement; otherwise delete the file and update incoming links.

After implementing a design, update the current-state architecture or operations documentation and remove obsolete planning detail. Keep historical rationale only as an ADR when it remains necessary to understand constraints or evaluate the decision later.

## Ownership

| Location | Owner and lifecycle |
| --- | --- |
| `docs/requirements/` | Product requirements; remains in the main repository |
| `docs/system/` | Cross-module architecture, development, designs, and operations; remains in the main repository |
| `docs/modules/frontend/` | Frontend-only knowledge; moves with the frontend repository |
| `docs/modules/backend/` | Backend-only knowledge; moves with the backend repository |
| `docs/modules/<service>/` | Service-only knowledge; moves with that service repository |

Put a document in `docs/system/` when changing it requires understanding more than one module or the main repository owns the workflow. Do not copy module documentation into the system folder; link to it.

## File names

- Use lowercase `kebab-case`.
- Name the subject, not the technology category: `auth-session-lifecycle.md`, not `technical-auth.md`.
- Avoid `technical`, `misc`, `notes`, `new`, `final`, dates, and version suffixes.
- Avoid `and` when it joins independently owned concerns; split the document instead.
- Do not repeat the module name inside its module folder: use `modules/frontend/development.md`, not `frontend-guidelines.md`.
- Keep a domain prefix when it improves repository-wide search: `auth-session-lifecycle.md`, not `lifecycle.md`.

Use these names consistently:

| Name | Meaning |
| --- | --- |
| `README.md` | Short folder index only; create one only when navigation benefits |
| `architecture.md` / `*-architecture.md` | Static structure and boundaries |
| `*-flow.md` | Request or message sequence |
| `*-lifecycle.md` | States and transitions |
| `*-design.md` | Proposed solution |
| `development.md` | Module implementation rules |
| `testing.md` / `testing-strategy.md` | Module tests / cross-module test ownership |
| `operations.md` | Module-wide runtime concerns |
| `*-runbook.md` | Repeatable operational procedure |
| `*-troubleshooting.md` | Diagnosis of a bounded concern |
| `*-reference.md` | Stable lookup material |
| `adr-NNNN-*.md` | Durable architecture decision |

## Document shape

Start non-requirement documents with only the metadata that helps readers route and trust the content:

```markdown
# Precise title

> Type: architecture | design | development | operations | reference
> Scope: system | frontend | backend | <service>
> Status: proposed | implemented | superseded
> Canonical for: one sentence
```

Add links to product requirements and code when relevant. Then use only the sections the subject needs:

1. **Summary** — three to five facts for fast orientation.
2. **Diagram** — the smallest useful visual.
3. **Invariants or decisions** — rules and rationale that code alone does not reveal.
4. **Failure modes** — condition, expected response, and verification.
5. **Code map** — only key entry points and owners.
6. **Verification** — tests, commands, or observable evidence.
7. **Related documents** — links without duplicated summaries.

Do not add boilerplate sections with no useful content. Keep `README.md` files as routers: task → canonical document.

## Diagrams

Use Mermaid so diagrams remain reviewable in source control:

| Relationship | Diagram |
| --- | --- |
| Components and dependencies | `flowchart` |
| Calls over time | `sequenceDiagram` |
| State transitions | `stateDiagram-v2` |
| Data relationships | `erDiagram` |

Use names that match code or public contracts. A diagram and its prose must not narrate the same details; prose records implications, exceptions, and rationale.

## Canonical-source boundaries

| Fact | Canonical source | Documentation role |
| --- | --- | --- |
| Product rule or default | `docs/requirements/` | Link and explain implementation impact |
| Machine-readable value | Config, workflow, Compose, or source code | Explain purpose and operational consequence |
| Module implementation pattern | `docs/modules/<module>/` | State the rule and nearest reference implementation |
| Cross-module interaction | `docs/system/` | Show boundaries, contracts, and failure handling |
| Shared command catalog and fast-start routing | `AGENTS.md` | Link to deeper documents; focused operations docs may repeat only commands required by their procedure |

## Update checklist

When adding, moving, or removing a document:

1. Apply the creation test; prefer updating the canonical source.
2. Confirm the owner, information type, and one reason to change.
3. Update `docs/README.md`; update `AGENTS.md` only for task-routing or command changes.
4. Update links in the root README, generated template README, package README, and source comments when affected.
5. Remove obsolete documents or mark them `superseded` only when a replacement pointer is still useful.
6. Run `npm run docs:validate`; it includes specialized requirements validation and fails when generated requirement indexes were stale. Review generated changes and run it again.
7. Search for old paths before removing the previous location.
