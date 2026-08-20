import { HttpErrorResponse } from '@angular/common/http';
import { AuthResponse } from '../models/auth.model';

const TRANSIENT_HTTP_STATUSES = new Set([0, 408, 429, 500, 502, 503, 504]);

export type RefreshFailureKind = 'auth' | 'transient';

export class RefreshSessionError extends Error {
  readonly kind: RefreshFailureKind;

  constructor(message: string, kind: RefreshFailureKind) {
    super(message);
    this.name = 'RefreshSessionError';
    this.kind = kind;
  }
}

export function getAccessTokenLifetimeMs(session: AuthResponse | null): number {
  if (!session) {
    return 0;
  }

  return new Date(session.expiresAtUtc).getTime() - Date.now();
}

export function getRefreshTokenLifetimeMs(session: AuthResponse | null): number {
  if (!session?.refreshTokenExpiresAtUtc) {
    return 0;
  }

  return new Date(session.refreshTokenExpiresAtUtc).getTime() - Date.now();
}

export function hasPersistedSession(session: AuthResponse | null): boolean {
  return session !== null && getRefreshTokenLifetimeMs(session) > 0;
}

export function toRefreshSessionError(error: unknown): RefreshSessionError {
  if (error instanceof RefreshSessionError) {
    return error;
  }

  if (error instanceof HttpErrorResponse) {
    if (error.status === 401 || error.status === 403) {
      return new RefreshSessionError(getHttpErrorMessage(error), 'auth');
    }

    if (TRANSIENT_HTTP_STATUSES.has(error.status)) {
      return new RefreshSessionError(getHttpErrorMessage(error), 'transient');
    }

    if (error.status >= 400 && error.status < 500) {
      return new RefreshSessionError(getHttpErrorMessage(error), 'auth');
    }

    return new RefreshSessionError(getHttpErrorMessage(error), 'transient');
  }

  if (error instanceof Error) {
    return new RefreshSessionError(error.message, 'auth');
  }

  return new RefreshSessionError('Session expired.', 'auth');
}

function getHttpErrorMessage(error: HttpErrorResponse): string {
  const body = error.error as { errors?: string[]; isSuccess?: boolean } | null;
  if (body?.errors?.length) {
    return body.errors.join(' ');
  }

  if (error.status === 0) {
    return "We couldn't connect to the server. Check your internet connection and try again.";
  }

  return error.message || 'Request failed.';
}
