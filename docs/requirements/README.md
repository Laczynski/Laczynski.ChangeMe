# Requirements index

> Five layers: **Domain** · **Conventions** · **Quality** · **Capabilities (FR-*)** · **Implementation (module and system docs)**.
> Layer index: `_shared/README.md`. Start: `requirements-change-process.md`. Authoring: `requirements-authoring-guide.md`. Validate: `npm run docs:validate`.

## L1 — Domain (`_shared/domain/`)

| Document | File |
| -------- | ---- |
| account-model.md | [account-model.md](_shared/domain/account-model.md) |
| glossary.md | [glossary.md](_shared/domain/glossary.md) |
| permissions.md | [permissions.md](_shared/domain/permissions.md) |
| Layer index | [README.md](_shared/domain/README.md) |

## L2 — Conventions (`_shared/conventions/`)

| ID | Title | File |
| -- | ----- | ---- |
| CONV-001 | Product Standards | [product-standards.md](_shared/conventions/product-standards.md) |

| — | [STD index & checklist](_shared/conventions/README.md) | [README.md](_shared/conventions/README.md) |

## L3 — Quality (`_shared/quality/`)

| ID | Title | File |
| -- | ----- | ---- |
| NFR-A11Y-001 | Accessibility | [accessibility.md](_shared/quality/accessibility.md) |
| NFR-I18N-001 | Internationalization and Copy | [internationalization.md](_shared/quality/internationalization.md) |
| NFR-PERF-001 | Performance and Scale | [performance-and-scale.md](_shared/quality/performance-and-scale.md) |
| NFR-QUAL-001 | Product Quality (Index) | [product-quality.md](_shared/quality/product-quality.md) |
| NFR-RSP-001 | Responsiveness and Layout | [responsiveness.md](_shared/quality/responsiveness.md) |
| — | Quality index | [README.md](_shared/quality/README.md) |

## Pending changes

See [changes/](changes/) for open requirement deltas.


## L4 — Access (`functional/access/`)

| ID | Title | File |
| -- | ----- | ---- |
| FR-ROL-001 | Permission Catalog and Effective Permissions | [fr-rol-001-permission-catalog-and-effective-permissions.md](functional/access/fr-rol-001-permission-catalog-and-effective-permissions.md) |
| FR-ROL-002 | Roles List | [fr-rol-002-roles-list.md](functional/access/fr-rol-002-roles-list.md) |
| FR-ROL-003 | Create and Edit Role | [fr-rol-003-create-and-edit-role.md](functional/access/fr-rol-003-create-and-edit-role.md) |
| FR-ROL-004 | Role Details | [fr-rol-004-role-details.md](functional/access/fr-rol-004-role-details.md) |
| FR-ROL-005 | Role and User Assignments | [fr-rol-005-role-and-user-assignments.md](functional/access/fr-rol-005-role-and-user-assignments.md) |
| FR-ROL-006 | Initial Administrator and System Roles | [fr-rol-006-initial-administrator-and-system-roles.md](functional/access/fr-rol-006-initial-administrator-and-system-roles.md) |

## L4 — Identity (`functional/identity/`)

| ID | Title | File |
| -- | ----- | ---- |
| FR-AUTH-001 | Login with Sessions | [fr-auth-001-login-with-sessions.md](functional/identity/fr-auth-001-login-with-sessions.md) |
| FR-AUTH-002 | Staying Signed In | [fr-auth-002-staying-signed-in.md](functional/identity/fr-auth-002-staying-signed-in.md) |
| FR-AUTH-003 | Logout | [fr-auth-003-logout.md](functional/identity/fr-auth-003-logout.md) |
| FR-AUTH-004 | My Sessions | [fr-auth-004-my-sessions.md](functional/identity/fr-auth-004-my-sessions.md) |
| FR-AUTH-008 | Password Policy | [fr-auth-008-password-policy.md](functional/identity/fr-auth-008-password-policy.md) |

## L4 — Issues (`functional/issues/`)

| ID | Title | File |
| -- | ----- | ---- |
| FR-ISS-001 | Issue List | [fr-iss-001-issue-list.md](functional/issues/fr-iss-001-issue-list.md) |
| FR-ISS-002 | Issue Create and Edit Flow | [fr-iss-002-issue-create-and-edit-flow.md](functional/issues/fr-iss-002-issue-create-and-edit-flow.md) |
| FR-ISS-003 | Issue Details, Comments, and Change History | [fr-iss-003-issue-details-comments-and-change-history.md](functional/issues/fr-iss-003-issue-details-comments-and-change-history.md) |
| FR-ISS-004 | Watching Issues and Push / Email Notifications | [fr-iss-004-watching-issues-and-push-email-notifications.md](functional/issues/fr-iss-004-watching-issues-and-push-email-notifications.md) |
| FR-ISS-005 | Notification Bell and Dropdown | [fr-iss-005-notification-bell-and-dropdown.md](functional/issues/fr-iss-005-notification-bell-and-dropdown.md) |
| FR-ISS-006 | Issue Attachments | [fr-iss-006-issue-attachments.md](functional/issues/fr-iss-006-issue-attachments.md) |

## L4 — Users (`functional/users/`)

| ID | Title | File |
| -- | ----- | ---- |
| FR-USR-001 | My Account Profile | [fr-usr-001-my-account-profile.md](functional/users/fr-usr-001-my-account-profile.md) |
| FR-USR-002 | User List | [fr-usr-002-user-list.md](functional/users/fr-usr-002-user-list.md) |
| FR-USR-003 | Create and Edit User (Admin) | [fr-usr-003-edit-user-admin.md](functional/users/fr-usr-003-edit-user-admin.md) |
| FR-USR-004 | User Details and Session Administration | [fr-usr-004-user-details-and-session-administration.md](functional/users/fr-usr-004-user-details-and-session-administration.md) |
| FR-USR-005 | Deactivate and Activate Accounts | [fr-usr-005-deactivate-and-activate-accounts.md](functional/users/fr-usr-005-deactivate-and-activate-accounts.md) |

---

_Auto-generated from `.requirements-manifest.json` via the internal requirements validator (refreshed by `npm run docs:validate`)._
