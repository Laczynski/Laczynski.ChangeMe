import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export function permissionGuard(
  requiredPermission: string,
  fallbackUrl = '/account'
): CanActivateFn {
  return permissionsGuard([requiredPermission], fallbackUrl);
}

export function permissionsGuard(
  requiredPermissions: string[],
  fallbackUrl = '/account'
): CanActivateFn {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (
      requiredPermissions.every((permission) => authService.hasPermission(permission))
    ) {
      return true;
    }

    return router.createUrlTree([fallbackUrl]);
  };
}
