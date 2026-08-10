import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  GuardResult,
  Router,
  RouterStateSnapshot,
  UrlTree,
  provideRouter
} from '@angular/router';
import { AuthService } from '../services/auth.service';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  let router: Router;
  let authService: {
    isAuthenticated: ReturnType<typeof vi.fn>;
  };

  const route = {} as ActivatedRouteSnapshot;

  function runGuard(url: string): GuardResult {
    const state = { url } as RouterStateSnapshot;
    return TestBed.runInInjectionContext(() => authGuard(route, state)) as GuardResult;
  }

  function expectUrl(result: GuardResult, expectedPath: string): void {
    expect(result).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(result as UrlTree)).toBe(expectedPath);
  }

  beforeEach(() => {
    authService = {
      isAuthenticated: vi.fn(() => false)
    };

    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: authService }]
    });

    router = TestBed.inject(Router);
  });

  it('redirects unauthenticated users to login with returnUrl', () => {
    expectUrl(runGuard('/issues'), '/login?returnUrl=%2Fissues');
  });

  it('allows access when authenticated', () => {
    authService.isAuthenticated.mockReturnValue(true);

    expect(runGuard('/issues')).toBe(true);
  });
});
