import { UserMembershipStatus } from '../utils/users.utils';

export interface UserListItemDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  deactivated: boolean;
  status: UserMembershipStatus;
  roleNames: string[];
  lastSignInAt: string | null;
  createdAt: string;
}

export interface UserRoleSummaryDto {
  id: string;
  name: string;
  isSystem: boolean;
}

export interface EffectivePermissionDto {
  code: string;
  label: string;
  description: string;
  group: string;
  fromRoleNames: string[];
}

export interface UserDetailsDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  deactivated: boolean;
  deactivatedAt: string | null;
  status: UserMembershipStatus;
  memberSince: string;
  lastSignInAt: string | null;
  version: number;
  roles: UserRoleSummaryDto[];
  effectivePermissions: EffectivePermissionDto[];
}

export interface RoleAssignmentOptionDto {
  id: string;
  name: string;
  isSystem: boolean;
}

export interface AdminUserSessionDto {
  id: string;
  deviceBrowserLabel: string;
  signInMethod: string;
  ipAddress: string | null;
  signedInAt: string;
  lastActivityAt: string;
}

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  roleIds: string[];
}

export interface UpdateUserRequest {
  id: string;
  version: number;
  firstName: string;
  lastName: string;
  email: string;
  roleIds?: string[];
  deactivated?: boolean;
}

export interface PreviewEffectivePermissionsRequest {
  roleIds: string[];
}
