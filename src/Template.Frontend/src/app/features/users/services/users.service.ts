import { Injectable, inject } from '@angular/core';
import { GridQuery, GridResult } from '@laczynski/datagrid';
import { ApiService } from '@shared/api/services/api.service';
import { Observable } from 'rxjs';
import {
  AdminUserSessionDto,
  CreateUserRequest,
  EffectivePermissionDto,
  PreviewEffectivePermissionsRequest,
  RoleAssignmentOptionDto,
  UpdateUserRequest,
  UserDetailsDto,
  UserListItemDto
} from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  private readonly apiService = inject(ApiService);
  private readonly baseEndpoint = 'users';

  getUsers(grid: GridQuery): Observable<GridResult<UserListItemDto>> {
    return this.apiService.get<GridResult<UserListItemDto>>(this.baseEndpoint, {
      grid
    });
  }

  getUserById(id: string): Observable<UserDetailsDto> {
    return this.apiService.get<UserDetailsDto>(`${this.baseEndpoint}/${id}`);
  }

  getRolesForAssignment(): Observable<RoleAssignmentOptionDto[]> {
    return this.apiService.get<RoleAssignmentOptionDto[]>(`${this.baseEndpoint}/roles`);
  }

  previewEffectivePermissions(
    request: PreviewEffectivePermissionsRequest
  ): Observable<EffectivePermissionDto[]> {
    return this.apiService.get<EffectivePermissionDto[]>(
      `${this.baseEndpoint}/effective-permissions/preview`,
      request
    );
  }

  createUser(request: CreateUserRequest): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(this.baseEndpoint, request);
  }

  updateUser(request: UpdateUserRequest): Observable<UserDetailsDto> {
    return this.apiService.put<UserDetailsDto>(
      `${this.baseEndpoint}/${request.id}`,
      request
    );
  }

  deactivateUser(id: string): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(
      `${this.baseEndpoint}/${id}/deactivate`,
      {}
    );
  }

  activateUser(id: string): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(
      `${this.baseEndpoint}/${id}/activate`,
      {}
    );
  }

  getUserSessions(
    id: string,
    grid: GridQuery
  ): Observable<GridResult<AdminUserSessionDto>> {
    return this.apiService.get<GridResult<AdminUserSessionDto>>(
      `${this.baseEndpoint}/${id}/sessions`,
      { grid }
    );
  }

  revokeUserSession(userId: string, sessionId: string): Observable<boolean> {
    return this.apiService.delete<boolean>(
      `${this.baseEndpoint}/${userId}/sessions/${sessionId}`
    );
  }

  revokeAllUserSessions(userId: string): Observable<boolean> {
    return this.apiService.post<boolean>(
      `${this.baseEndpoint}/${userId}/sessions/revoke-all`,
      {}
    );
  }
}
