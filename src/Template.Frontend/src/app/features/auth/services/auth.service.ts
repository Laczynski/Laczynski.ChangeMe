import { HttpClient, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { getApiUrl } from '@environments/runtime-config';
import { Result } from '@shared/api/models/api-response.model';
import { ApiService } from '@shared/api/services/api.service';
import { Observable, firstValueFrom, from, of, throwError } from 'rxjs';
import { catchError, finalize, map, switchMap, tap } from 'rxjs/operators';
import {
  AuthResponse,
  LoginRequest,
  LoginResponse,
  MyAccountDto,
  UpdateMyAccountRequest
} from '../models/auth.model';
import {
  RefreshSessionError,
  getAccessTokenLifetimeMs,
  getRefreshTokenLifetimeMs,
  hasPersistedSession,
  toRefreshSessionError
} from '../utils/auth-session.utils';
import { AuthConstraints } from '../utils/auth.utils';
import { AuthStorageService } from './auth-storage.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiService = inject(ApiService);
  private readonly http = inject(HttpClient);
  private readonly authStorageService = inject(AuthStorageService);
  private readonly router = inject(Router);

  private readonly baseUrl = getApiUrl() + '/';
  private readonly session = signal<AuthResponse | null>(
    this.authStorageService.getSession()
  );
  private renewalTimerId: ReturnType<typeof setTimeout> | null = null;
  private refreshInFlight: Promise<AuthResponse> | null = null;

  readonly currentSession = this.session.asReadonly();
  readonly isAuthenticated = computed(() => hasPersistedSession(this.session()));
  readonly token = computed(() => {
    const current = this.session();
    if (!current || getAccessTokenLifetimeMs(current) <= 0) {
      return null;
    }

    return current.token;
  });
  readonly permissions = computed(() => this.session()?.permissions ?? []);
  readonly currentUser = computed(() => {
    const session = this.session();
    if (!session) {
      return null;
    }

    return {
      id: session.userId,
      firstName: session.firstName,
      lastName: session.lastName,
      email: session.email
    };
  });

  initializeSession(): Promise<void> {
    const storedSession = this.session();
    if (!storedSession) {
      return Promise.resolve();
    }

    if (getRefreshTokenLifetimeMs(storedSession) <= 0) {
      this.clearLocalSession();
      return Promise.resolve();
    }

    if (getAccessTokenLifetimeMs(storedSession) > 0) {
      this.scheduleRenewal(storedSession);
      return Promise.resolve();
    }

    return this.refreshStoredSession().then((session) => {
      if (!session) {
        this.clearLocalSession();
      }
    });
  }

  hasPermission(permissionCode: string): boolean {
    return this.permissions().includes(permissionCode);
  }

  login(request: LoginRequest) {
    return this.apiService
      .post<LoginResponse>('auth/login', request)
      .pipe(tap((response) => this.setSession(response.authSession)));
  }

  refreshSession() {
    return from(this.refreshSessionOnce());
  }

  logout() {
    return this.apiService.post<boolean>('auth/logout', {}).pipe(
      catchError(() => of(true)),
      finalize(() => this.clearLocalSession())
    );
  }

  getMyAccount() {
    return this.apiService.get<MyAccountDto>('auth/account');
  }

  updateMyAccount(request: UpdateMyAccountRequest) {
    return this.apiService.put<MyAccountDto>('auth/account', request);
  }

  syncProfileToSession(firstName: string, lastName: string): void {
    const current = this.session();
    if (!current) {
      return;
    }

    this.setSession({
      ...current,
      firstName,
      lastName
    });
  }

  continueAfterLogin(returnUrl = '/issues'): void {
    void this.router.navigateByUrl(returnUrl);
  }

  tryRefreshAndRetry(req: HttpRequest<unknown>, next: HttpHandlerFn) {
    if (req.headers.has('X-Skip-Auth-Refresh')) {
      return next(req);
    }

    return this.refreshSession().pipe(
      switchMap(() => {
        const token = this.token();
        if (!token) {
          return throwError(() => new RefreshSessionError('Session expired.', 'auth'));
        }

        return next(
          req.clone({
            setHeaders: { Authorization: `Bearer ${token}` }
          })
        );
      }),
      catchError((error: unknown) => {
        const refreshError = toRefreshSessionError(error);
        if (refreshError.kind === 'auth') {
          this.clearLocalSession();
          void this.router.navigateByUrl('/login');
        }

        return throwError(() => refreshError);
      })
    );
  }

  private postRefresh(body: { refreshToken: string }): Observable<AuthResponse> {
    return this.http
      .post<Result<AuthResponse>>(`${this.baseUrl}auth/refresh`, body, {
        headers: { 'X-Skip-Auth-Refresh': 'true' }
      })
      .pipe(
        map((response) => {
          if (response.isSuccess) {
            return response.value as AuthResponse;
          }

          throw new RefreshSessionError(this.getErrorMessage(response), 'auth');
        }),
        catchError((error: unknown) => {
          const refreshError = toRefreshSessionError(error);
          if (refreshError.kind === 'auth') {
            this.clearLocalSession();
          }

          return throwError(() => refreshError);
        })
      );
  }

  private refreshSessionOnce(): Promise<AuthResponse> {
    const current = this.session();
    if (!current?.refreshToken) {
      return Promise.reject(new RefreshSessionError('Session expired.', 'auth'));
    }

    if (getRefreshTokenLifetimeMs(current) <= 0) {
      return Promise.reject(new RefreshSessionError('Session expired.', 'auth'));
    }

    if (this.refreshInFlight) {
      return this.refreshInFlight;
    }

    this.refreshInFlight = firstValueFrom(
      this.postRefresh({
        refreshToken: current.refreshToken
      }).pipe(tap((session) => this.setSession(session)))
    )
      .catch((error: unknown) => {
        if (error instanceof RefreshSessionError) {
          throw error;
        }

        throw toRefreshSessionError(error);
      })
      .finally(() => {
        this.refreshInFlight = null;
      });

    return this.refreshInFlight;
  }

  private async refreshStoredSession(): Promise<AuthResponse | null> {
    try {
      return await this.refreshSessionOnce();
    } catch (error: unknown) {
      const refreshError = toRefreshSessionError(error);
      if (refreshError.kind === 'transient') {
        this.scheduleTransientRefreshRetry();
        return this.session();
      }

      return null;
    }
  }

  private setSession(session: AuthResponse): void {
    this.session.set(session);
    this.authStorageService.setSession(session);
    this.scheduleRenewal(session);
  }

  clearLocalSession(): void {
    this.clearRenewalTimer();
    this.refreshInFlight = null;
    this.session.set(null);
    this.authStorageService.clearSession();
  }

  private scheduleRenewal(session: AuthResponse): void {
    this.clearRenewalTimer();

    const delay = this.getRenewalDelayMs(session);
    if (delay === null) {
      return;
    }

    this.renewalTimerId = setTimeout(() => {
      void this.refreshSessionOnce().catch((error: unknown) => {
        this.handleBackgroundRefreshFailure(error);
      });
    }, delay);
  }

  private scheduleTransientRefreshRetry(): void {
    this.clearRenewalTimer();

    const session = this.session();
    if (!hasPersistedSession(session)) {
      this.clearLocalSession();
      void this.router.navigateByUrl('/login');
      return;
    }

    this.renewalTimerId = setTimeout(() => {
      void this.refreshSessionOnce().catch((error: unknown) => {
        this.handleBackgroundRefreshFailure(error);
      });
    }, AuthConstraints.TRANSIENT_REFRESH_RETRY_MS);
  }

  private handleBackgroundRefreshFailure(error: unknown): void {
    const refreshError = toRefreshSessionError(error);
    if (refreshError.kind === 'auth') {
      this.clearLocalSession();
      void this.router.navigateByUrl('/login');
      return;
    }

    this.scheduleTransientRefreshRetry();
  }

  private getRenewalDelayMs(session: AuthResponse): number | null {
    const lifetimeMs = getAccessTokenLifetimeMs(session);

    if (lifetimeMs <= AuthConstraints.MIN_RENEWAL_SCHEDULE_MS) {
      return null;
    }

    const leadTimeMs = Math.min(
      AuthConstraints.RENEWAL_LEAD_TIME_MS,
      Math.floor(lifetimeMs / 2)
    );

    return Math.max(AuthConstraints.MIN_RENEWAL_SCHEDULE_MS, lifetimeMs - leadTimeMs);
  }

  private clearRenewalTimer(): void {
    if (this.renewalTimerId !== null) {
      clearTimeout(this.renewalTimerId);
      this.renewalTimerId = null;
    }
  }

  private getErrorMessage(result: Result<unknown>): string {
    if (result.errors?.length) {
      return result.errors.join(' ');
    }

    return 'Request failed.';
  }
}
