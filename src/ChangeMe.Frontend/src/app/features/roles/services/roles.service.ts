import { Injectable, inject } from '@angular/core';
import { GridQuery, GridResult } from '@laczynski/datagrid';
import { ApiService } from '@shared/api/services/api.service';
import { Observable } from 'rxjs';
import {
  CreateRoleRequest,
  PermissionCatalogItemDto,
  RoleAssignedUserDto,
  RoleDetailsDto,
  RoleListItemDto,
  UpdateRoleRequest
} from '../models/role.model';

@Injectable({
  providedIn: 'root'
})
export class RolesService {
  private readonly apiService = inject(ApiService);
  private readonly baseEndpoint = 'roles';

  getRoles(grid: GridQuery): Observable<GridResult<RoleListItemDto>> {
    return this.apiService.get<GridResult<RoleListItemDto>>(this.baseEndpoint, {
      grid
    });
  }

  getRoleById(id: string): Observable<RoleDetailsDto> {
    return this.apiService.get<RoleDetailsDto>(`${this.baseEndpoint}/${id}`);
  }

  getPermissionCatalog(): Observable<PermissionCatalogItemDto[]> {
    return this.apiService.get<PermissionCatalogItemDto[]>(
      `${this.baseEndpoint}/permission-catalog`
    );
  }

  createRole(request: CreateRoleRequest): Observable<RoleDetailsDto> {
    return this.apiService.post<RoleDetailsDto>(this.baseEndpoint, request);
  }

  updateRole(request: UpdateRoleRequest): Observable<RoleDetailsDto> {
    return this.apiService.put<RoleDetailsDto>(
      `${this.baseEndpoint}/${request.id}`,
      request
    );
  }

  deleteRole(id: string): Observable<boolean> {
    return this.apiService.delete<boolean>(`${this.baseEndpoint}/${id}`);
  }

  getRoleAssignedUsers(
    roleId: string,
    grid: GridQuery
  ): Observable<GridResult<RoleAssignedUserDto>> {
    return this.apiService.get<GridResult<RoleAssignedUserDto>>(
      `${this.baseEndpoint}/${roleId}/users`,
      { grid }
    );
  }

  removeUserFromRole(roleId: string, userId: string): Observable<boolean> {
    return this.apiService.delete<boolean>(
      `${this.baseEndpoint}/${roleId}/users/${userId}`
    );
  }
}
