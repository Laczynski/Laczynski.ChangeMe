import {
  HttpClient,
  HttpErrorResponse,
  HttpHandlerFn,
  HttpRequest
} from '@angular/common/http';
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
  readonly isAuthenticated = computed(() => {
    const current = this.session();
    return current !== null && this.getAccessTokenLifetimeMs(current) > 0;
  });
  readonly token = computed(() => {
    const current = this.session();
    if (!current || this.getAccessTokenLifetimeMs(current) <= 0) {
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

    if (this.getAccessTokenLifetimeMs(storedSession) <= 0) {
      return this.refreshStoredSession().then((session) => {
        if (!session) {
          this.clearLocalSession();
        }
      });
    }

    this.scheduleRenewal(storedSession);
    return Promise.resolve();
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
          return throwError(() => new Error('Session expired.'));
        }

        return next(
          req.clone({
            setHeaders: { Authorization: `Bearer ${token}` }
          })
        );
      }),
      catchError((error: unknown) => {
        this.clearLocalSession();
        void this.router.navigateByUrl('/login');
        return throwError(() => error);
      })
    );
  }

  private postAuth<T>(endpoint: string, body: unknown): Observable<T> {
    return this.http
      .post<Result<T>>(`${this.baseUrl}${endpoint}`, body, {
        headers: { 'X-Skip-Auth-Refresh': 'true' }
      })
      .pipe(
        map((response) => {
          if (response.isSuccess) {
            return response.value as T;
          }

          throw new Error(this.getErrorMessage(response));
        }),
        catchError((error: unknown) => throwError(() => this.toError(error)))
      );
  }

  private refreshSessionOnce(): Promise<AuthResponse> {
    const current = this.session();
    if (!current?.refreshToken) {
      return Promise.reject(new Error('Session expired.'));
    }

    if (this.refreshInFlight) {
      return this.refreshInFlight;
    }

    this.refreshInFlight = firstValueFrom(
      this.postAuth<AuthResponse>('auth/refresh', {
        refreshToken: current.refreshToken
      }).pipe(
        tap((session) => this.setSession(session)),
        catchError((error: unknown) => {
          this.clearLocalSession();
          return throwError(() => error);
        })
      )
    )
      .catch((error: unknown) => {
        if (error instanceof Error) {
          throw error;
        }

        throw new Error('Session expired.');
      })
      .finally(() => {
        this.refreshInFlight = null;
      });

    return this.refreshInFlight;
  }

  private async refreshStoredSession(): Promise<AuthResponse | null> {
    try {
      return await this.refreshSessionOnce();
    } catch {
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
      void this.refreshSessionOnce().catch(() => {
        this.clearLocalSession();
        void this.router.navigateByUrl('/login');
      });
    }, delay);
  }

  private getRenewalDelayMs(session: AuthResponse): number | null {
    const lifetimeMs = this.getAccessTokenLifetimeMs(session);

    if (lifetimeMs <= AuthConstraints.MIN_RENEWAL_SCHEDULE_MS) {
      return null;
    }

    const leadTimeMs = Math.min(
      AuthConstraints.RENEWAL_LEAD_TIME_MS,
      Math.floor(lifetimeMs / 2)
    );

    return Math.max(AuthConstraints.MIN_RENEWAL_SCHEDULE_MS, lifetimeMs - leadTimeMs);
  }

  private getAccessTokenLifetimeMs(session: AuthResponse | null): number {
    if (!session) {
      return 0;
    }

    return new Date(session.expiresAtUtc).getTime() - Date.now();
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

  private toError(error: unknown): Error {
    if (error instanceof HttpErrorResponse) {
      const body = error.error as Partial<Result<unknown>> | null;
      if (body && typeof body.isSuccess === 'boolean' && !body.isSuccess) {
        return new Error(this.getErrorMessage(body as Result<unknown>));
      }
    }

    if (error instanceof Error) {
      return error;
    }

    return new Error('Request failed.');
  }
}
