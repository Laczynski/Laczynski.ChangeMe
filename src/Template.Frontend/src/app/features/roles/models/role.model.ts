export interface RoleListItemDto {
  id: string;
  name: string;
  description: string | null;
  permissionCount: number;
  userCount: number;
  isSystem: boolean;
}

export interface PermissionCatalogItemDto {
  code: string;
  label: string;
  description: string;
  group: string;
}

export interface RolePermissionItemDto {
  code: string;
  label: string;
  description: string;
  group: string;
}

export interface RoleDetailsDto {
  id: string;
  name: string;
  description: string | null;
  isSystem: boolean;
  permissionCount: number;
  userCount: number;
  version: number;
  permissions: RolePermissionItemDto[];
}

export interface RoleAssignedUserDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  deactivated: boolean;
}

export interface CreateRoleRequest {
  name: string;
  description?: string | null;
  permissionCodes: string[];
}

export interface UpdateRoleRequest {
  id: string;
  version: number;
  name: string;
  description?: string | null;
  permissionCodes: string[];
}
