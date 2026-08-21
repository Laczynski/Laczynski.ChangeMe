# Glossary — account and sign-in terms

> Single source of truth for cross-cutting business terms. Functional specifications reference this document instead of duplicating definitions.

The following terms describe observable account state, not implementation details.

## Account lifecycle

| Term                    | Meaning                                                                                                                       |
| ----------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| **Account enabled**     | The user is not deactivated by an administrator and may sign in when other rules allow.                                       |
| **Account deactivated** | An administrator disabled the account; the user cannot sign in and has no effective permissions until reactivated.            |
| **Create user**         | Administrator action that creates an account with email, password, profile, and role assignments. UI label **`Create user`**. |

## Sign-in methods

| Term               | Meaning                                                                                             |
| ------------------ | --------------------------------------------------------------------------------------------------- |
| **Local password** | A password stored in the application for email/password sign-in. Set at user creation (FR-USR-003). |

## Email and profile

| Term              | Meaning                                                                                                                          |
| ----------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| **Profile email** | The **current email** on the user account; used for sign-in and display. Shown as **Email** on **My account** and admin screens. |

## Cross-references

- Login and sessions: FR-AUTH-001
- User administration: `docs/requirements/functional/users/`
- Account model (admin UI): `docs/requirements/_shared/domain/account-model.md`
