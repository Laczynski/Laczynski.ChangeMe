import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

const AUTH_ENDPOINTS_WITHOUT_REFRESH = ['/auth/login', '/auth/refresh'] as const;

function shouldAttemptTokenRefresh(requestUrl: string): boolean {
  return !AUTH_ENDPOINTS_WITHOUT_REFRESH.some((segment) =>
    requestUrl.includes(segment)
  );
}

export const authTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.token();

  const authorizedRequest = token
    ? req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      })
    : req;

  return next(authorizedRequest).pipe(
    catchError((error: unknown) => {
      const status =
        error && typeof error === 'object' && 'status' in error
          ? (error as { status: number }).status
          : undefined;

      if (status !== 401 || !shouldAttemptTokenRefresh(req.url)) {
        return throwError(() => error);
      }

      return authService.tryRefreshAndRetry(req, next);
    })
  );
};
