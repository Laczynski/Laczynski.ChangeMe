import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { getApiUrl } from '@environments/runtime-config';
import { ApiService } from '@shared/api/services/api.service';
import { AuthResponse } from '../models/auth.model';
import { AuthStorageService } from './auth-storage.service';
import { AuthService } from './auth.service';

function createSession(overrides: Partial<AuthResponse> = {}): AuthResponse {
  return {
    userId: 'user-1',
    firstName: 'Jane',
    lastName: 'Doe',
    email: 'jane@example.com',
    sessionId: 'session-1',
    token: 'access-token',
    expiresAtUtc: new Date(Date.now() - 60_000).toISOString(),
    refreshToken: 'refresh-token',
    refreshTokenExpiresAtUtc: new Date(
      Date.now() + 14 * 24 * 60 * 60_000
    ).toISOString(),
    permissions: [],
    ...overrides
  };
}

describe('AuthService session resilience', () => {
  let storage: AuthStorageService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        AuthStorageService,
        ApiService,
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    storage = TestBed.inject(AuthStorageService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
    TestBed.resetTestingModule();
  });

  it('keeps the stored session when startup refresh fails because the API is unavailable', async () => {
    const session = createSession();
    storage.setSession(session);

    const service = TestBed.inject(AuthService);
    const initPromise = service.initializeSession();

    const refreshRequest = httpMock.expectOne(`${getApiUrl()}/auth/refresh`);
    refreshRequest.flush(null, { status: 0, statusText: 'Unknown Error' });

    await initPromise;

    expect(storage.getSession()).toEqual(session);
    expect(service.isAuthenticated()).toBe(true);
  });

  it('clears the stored session when startup refresh is rejected by the API', async () => {
    const session = createSession();
    storage.setSession(session);

    const service = TestBed.inject(AuthService);
    const initPromise = service.initializeSession();

    const refreshRequest = httpMock.expectOne(`${getApiUrl()}/auth/refresh`);
    refreshRequest.flush(
      { isSuccess: false, status: 3, errors: ['Please sign in to continue.'] },
      { status: 401, statusText: 'Unauthorized' }
    );

    await initPromise;

    expect(storage.getSession()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
  });
});
