import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, computed, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { SidebarNavComponent } from '@core/layout/components/sidebar-nav/sidebar-nav.component';
import { LayoutNavItem } from '@core/layout/models/layout-nav-item.model';
import { LayoutService } from '@core/layout/services/layout.service';
import { formatUserReference } from '@core/user/utils/user-display.utils';
import { AuthService } from '@features/auth/services/auth.service';
import { NotificationsBellComponent } from '@features/notifications/components/notifications-bell/notifications-bell.component';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideChevronRight,
  lucideLogOut,
  lucideMenu,
  lucideMoon,
  lucideSun,
  lucideZap
} from '@ng-icons/lucide';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { HlmButtonImports } from '@spartan/ui/button';
import { HlmSheetImports } from '@spartan/ui/sheet';
import { filter, map } from 'rxjs/operators';

@Component({
  selector: 'app-shell',
  imports: [
    RouterOutlet,
    RouterLink,
    SidebarNavComponent,
    NotificationsBellComponent,
    ...HlmButtonImports,
    ...HlmSheetImports,
    NgIcon
  ],
  providers: [
    provideIcons({
      lucideChevronRight,
      lucideLogOut,
      lucideMenu,
      lucideMoon,
      lucideSun,
      lucideZap
    })
  ],
  templateUrl: './app-shell.component.html'
})
export class AppShellComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);

  readonly layoutService = inject(LayoutService);

  readonly currentUser = this.authService.currentUser;
  readonly formatUserReference = formatUserReference;
  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly showAuthenticatedChrome = computed(() => this.isAuthenticated());
  readonly isDesktop = toSignal(
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .pipe(map((state) => state.matches)),
    { initialValue: false }
  );
  readonly mobileNavState = computed(() =>
    this.layoutService.$mobileNavOpen() ? 'open' : 'closed'
  );

  readonly authenticatedNavItems = computed<LayoutNavItem[]>(() => {
    const items: LayoutNavItem[] = [
      { label: 'Issues list', icon: 'lucideList', routerLink: '/issues', exact: true },
      { label: 'Create issue', icon: 'lucidePlus', routerLink: '/issues/create' }
    ];

    if (this.authService.hasPermission(PermissionCodes.usersView)) {
      items.push({
        label: 'Users list',
        icon: 'lucideUsers',
        routerLink: '/users',
        exact: true
      });
    }

    if (this.authService.hasPermission(PermissionCodes.rolesView)) {
      items.push({
        label: 'Roles list',
        icon: 'lucideShield',
        routerLink: '/roles',
        exact: true
      });
    }

    items.push({ label: 'My account', icon: 'lucideUser', routerLink: '/account' });
    return items;
  });

  constructor() {
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.layoutService.closeMobileNav());

    this.breakpointObserver
      .observe('(min-width: 768px)')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((state) => {
        if (state.matches) {
          this.layoutService.closeMobileNav();
        }
      });
  }

  onMenuToggle(): void {
    if (this.isDesktop()) {
      this.layoutService.toggleSidebarCollapsed();
      return;
    }

    this.layoutService.toggleMobileNav();
  }

  onMobileNavStateChanged(state: 'open' | 'closed'): void {
    if (state === 'open') {
      this.layoutService.openMobileNav();
      return;
    }

    this.layoutService.closeMobileNav();
  }

  logout(): void {
    this.authService
      .logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          void this.router.navigateByUrl('/login');
        },
        error: () => {
          void this.router.navigateByUrl('/login');
        }
      });
  }
}
