import { HttpErrorResponse } from '@angular/common/http';
import { AuthResponse } from '../models/auth.model';
import {
  getAccessTokenLifetimeMs,
  getRefreshTokenLifetimeMs,
  hasPersistedSession,
  toRefreshSessionError
} from './auth-session.utils';

function createSession(overrides: Partial<AuthResponse> = {}): AuthResponse {
  const accessExpiresAt = new Date(Date.now() + 30 * 60_000).toISOString();
  const refreshExpiresAt = new Date(Date.now() + 14 * 24 * 60 * 60_000).toISOString();

  return {
    userId: 'user-1',
    firstName: 'Jane',
    lastName: 'Doe',
    email: 'jane@example.com',
    sessionId: 'session-1',
    token: 'access-token',
    expiresAtUtc: accessExpiresAt,
    refreshToken: 'refresh-token',
    refreshTokenExpiresAtUtc: refreshExpiresAt,
    permissions: [],
    ...overrides
  };
}

describe('auth-session.utils', () => {
  it('treats a valid refresh token as a persisted session', () => {
    const session = createSession({
      expiresAtUtc: new Date(Date.now() - 60_000).toISOString()
    });

    expect(getAccessTokenLifetimeMs(session)).toBeLessThanOrEqual(0);
    expect(getRefreshTokenLifetimeMs(session)).toBeGreaterThan(0);
    expect(hasPersistedSession(session)).toBe(true);
  });

  it('classifies network failures as transient refresh errors', () => {
    const error = toRefreshSessionError(
      new HttpErrorResponse({ status: 0, statusText: 'Unknown Error' })
    );

    expect(error.kind).toBe('transient');
  });

  it('classifies unauthorized refresh responses as auth failures', () => {
    const error = toRefreshSessionError(
      new HttpErrorResponse({ status: 401, statusText: 'Unauthorized' })
    );

    expect(error.kind).toBe('auth');
  });

  it('classifies service unavailable responses as transient refresh errors', () => {
    const error = toRefreshSessionError(
      new HttpErrorResponse({ status: 503, statusText: 'Service Unavailable' })
    );

    expect(error.kind).toBe('transient');
  });
});
