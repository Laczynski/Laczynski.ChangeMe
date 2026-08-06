import type { ConfirmMessagePart } from '@core/confirm/models/confirm-message.model';
import { confirmMessage, confirmStrong } from '@core/confirm/utils/confirm-message.utils';

export const UserConstraints = {
  NAME_MAX_LENGTH: 100,
  EMAIL_MAX_LENGTH: 320,
  PASSWORD_MIN_LENGTH: 8,
  PASSWORD_MAX_LENGTH: 128
};

export type UserMembershipStatus = 'Active' | 'Deactivated';

export const UserMessages = {
  duplicateEmail: 'A user with this email already exists.',
  userCreated: 'User created.',
  userSaved: 'User saved.',
  userDeactivated: 'User deactivated.',
  userActivated: 'User activated.',
  profileUpdated: 'Profile updated.',
  noRolesAssigned: 'No roles assigned.',
  noPermissions: 'No permissions.',
  selectRoleToPreview: 'Select at least one role to preview permissions.',
  neverSignedIn: 'Never',
  noActiveSessions: 'No active sessions.',
  revokeSessionTitle: 'Revoke this session?',
  revokeSessionMessage: 'Revoke this session? That device will be signed out.',
  revokeAllSessionsTitle: 'Revoke all active sessions for this user?',
  revokeAllSessionsMessage:
    'Revoke all active sessions for this user? They will be signed out on every device.'
};

export const UserFieldErrors = {
  firstName: {
    required: 'First name is required.',
    maxlength: `First name must be at most ${UserConstraints.NAME_MAX_LENGTH} characters.`
  },
  lastName: {
    required: 'Last name is required.',
    maxlength: `Last name must be at most ${UserConstraints.NAME_MAX_LENGTH} characters.`
  },
  email: {
    required: 'Email is required.',
    email: 'Enter a valid email address.',
    maxlength: `Email must be less than ${UserConstraints.EMAIL_MAX_LENGTH} characters long.`
  },
  password: {
    required: 'Password is required.',
    minlength: `Password must be at least ${UserConstraints.PASSWORD_MIN_LENGTH} characters long.`,
    maxlength: `Password must be less than ${UserConstraints.PASSWORD_MAX_LENGTH} characters long.`,
    passwordPolicy: (error: unknown) =>
      typeof error === 'string' ? error : 'Password does not meet policy requirements.'
  },
  confirmPassword: {
    required: 'Confirm password is required.',
    passwordMismatch: 'Passwords do not match.'
  },
  roleIds: {
    required: 'Select at least one role.'
  }
} as const;

export const statusFilters: { label: string; value: UserMembershipStatus }[] = [
  { label: 'Active', value: 'Active' },
  { label: 'Deactivated', value: 'Deactivated' }
];

export function getUserStatusLabel(status: UserMembershipStatus): string {
  return status === 'Deactivated' ? 'Deactivated' : 'Active';
}

export function toUserMembershipStatus(deactivated: boolean): UserMembershipStatus {
  return deactivated ? 'Deactivated' : 'Active';
}

export function getUserStatusSeverity(
  status: UserMembershipStatus
): 'success' | 'danger' {
  return status === 'Deactivated' ? 'danger' : 'success';
}

export function formatFromRoles(roleNames: string[]): string {
  if (roleNames.length === 0) {
    return '';
  }

  if (roleNames.length === 1) {
    return `From role: ${roleNames[0]}`;
  }

  return `From roles: ${roleNames.join(', ')}`;
}

export function getDeactivateConfirmMessage(userReference: string): ConfirmMessagePart[] {
  return confirmMessage(
    'Deactivate ',
    confirmStrong(userReference),
    '? The user will be signed out and cannot sign in until reactivated.'
  );
}

export function getActivateConfirmMessage(userReference: string): ConfirmMessagePart[] {
  return confirmMessage(
    'Activate ',
    confirmStrong(userReference),
    '? The user will be able to sign in again.'
  );
}

export function groupEffectivePermissions<T extends { group: string }>(
  permissions: T[]
): { group: string; items: T[] }[] {
  const groups = new Map<string, T[]>();

  for (const permission of permissions) {
    const existing = groups.get(permission.group) ?? [];
    existing.push(permission);
    groups.set(permission.group, existing);
  }

  return [...groups.entries()].map(([group, items]) => ({ group, items }));
}
