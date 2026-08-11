import { highlightDialogValue } from '@shared/ui/utils/dialog-message.utils';

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

export function formatDescription(description: string | null | undefined): string {
  return description?.trim() ? description : RoleMessages.emptyDescription;
}

export function getDeleteRoleConfirmMessage(roleName: string): string {
  return `Delete role ${highlightDialogValue(roleName)}? Users will lose permissions granted only through this role.`;
}

export function getRemoveUserFromRoleConfirmMessage(
  userReference: string,
  roleName: string
): string {
  return `Remove ${highlightDialogValue(userReference)} from role ${highlightDialogValue(roleName)}? The user will lose permissions granted only through this role.`;
}
