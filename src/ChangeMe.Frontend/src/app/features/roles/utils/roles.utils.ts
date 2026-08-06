import type { ConfirmMessagePart } from '@core/confirm/models/confirm-message.model';
import { confirmMessage, confirmStrong } from '@core/confirm/utils/confirm-message.utils';

export { groupEffectivePermissions } from '@features/users/utils/users.utils';

export const RoleConstraints = {
  NAME_MIN_LENGTH: 2,
  NAME_MAX_LENGTH: 100,
  DESCRIPTION_MAX_LENGTH: 500
};

export const RoleMessages = {
  duplicateName: 'A role with this name already exists.',
  descriptionTooLong: 'Description cannot exceed 500 characters.',
  atLeastOnePermission: 'At least one permission is required.',
  roleCreated: 'Role created.',
  roleSaved: 'Role saved.',
  roleDeleted: 'Role deleted.',
  systemRoleCannotBeModified: 'System roles cannot be modified.',
  roleAssignedToUsers:
    'Role is assigned to one or more users. Remove all user assignments before deleting this role.',
  userRemovedFromRole: 'User removed from role.',
  userMustHaveRole:
    'Each user must have at least one role. Assign another role before removing this one.',
  noAssignedUsers: 'No users are assigned to this role.',
  emptyDescription: '—',
  permissionsCount: (count: number) => `${count} permissions`,
  usersCount: (count: number) => `${count} users`
};

export const RoleFieldErrors = {
  name: {
    required: 'Name is required.',
    minlength: `Name must be at least ${RoleConstraints.NAME_MIN_LENGTH} characters.`,
    maxlength: `Name must be at most ${RoleConstraints.NAME_MAX_LENGTH} characters.`
  },
  description: {
    maxlength: RoleMessages.descriptionTooLong
  },
  permissionCodes: {
    required: RoleMessages.atLeastOnePermission
  }
} as const;

export function formatDescription(description: string | null | undefined): string {
  return description?.trim() ? description : RoleMessages.emptyDescription;
}

export function getDeleteRoleConfirmMessage(roleName: string): ConfirmMessagePart[] {
  return confirmMessage(
    'Delete role ',
    confirmStrong(roleName),
    '? Users will lose permissions granted only through this role.'
  );
}

export function getRemoveUserFromRoleConfirmMessage(
  userReference: string,
  roleName: string
): ConfirmMessagePart[] {
  return confirmMessage(
    'Remove ',
    confirmStrong(userReference),
    ' from role ',
    confirmStrong(roleName),
    '? The user will lose permissions granted only through this role.'
  );
}
