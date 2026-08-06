import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, computed, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { SidebarNavComponent } from '@core/layout/components/sidebar-nav/sidebar-nav.component';
import { LayoutNavItem } from '@core/layout/models/layout-nav-item.model';
import { LayoutService } from '@core/layout/services/layout.service';
import { PageChromeService } from '@core/layout/services/page-chrome.service';
import { formatUserReference } from '@core/user/utils/user-display.utils';
import { AuthService } from '@features/auth/services/auth.service';
import { NotificationsBellComponent } from '@features/notifications/components/notifications-bell/notifications-bell.component';
import {
  BreadcrumbComponent,
  ButtonComponent,
  DrawerComponent,
  IconComponent
} from '@laczynski/ui';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { filter, map } from 'rxjs/operators';

const MOBILE_BREAKPOINT = '(max-width: 767.98px)';

@Component({
  selector: 'app-shell',
  imports: [
    RouterOutlet,
    SidebarNavComponent,
    BreadcrumbComponent,
    NotificationsBellComponent,
    ButtonComponent,
    DrawerComponent,
    IconComponent
  ],
  templateUrl: './app-shell.component.html'
})
export class AppShellComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);

  readonly layoutService = inject(LayoutService);
  readonly pageChrome = inject(PageChromeService);

  readonly pageBreadcrumbs = this.pageChrome.breadcrumbs;
  readonly currentPageTitle = this.pageChrome.currentTitle;

  readonly breadcrumbItems = computed(() =>
    this.pageBreadcrumbs().map((crumb, index, crumbs) => ({
      id: String(index),
      label: crumb.label,
      selected: index === crumbs.length - 1
    }))
  );

  readonly currentUser = this.authService.currentUser;
  readonly formatUserReference = formatUserReference;
  readonly showAuthenticatedChrome = computed(() => this.authService.isAuthenticated());

  readonly isMobile = toSignal(
    this.breakpointObserver
      .observe(MOBILE_BREAKPOINT)
      .pipe(map((state) => state.matches)),
    {
      initialValue:
        typeof window !== 'undefined'
          ? window.matchMedia(MOBILE_BREAKPOINT).matches
          : false
    }
  );

  readonly sidebarOpen = computed(() =>
    this.isMobile()
      ? this.layoutService.$mobileNavOpen()
      : !this.layoutService.$sidebarCollapsed()
  );

  readonly themeLabel = computed(() =>
    this.layoutService.$themeMode() === 'dark' ? 'Light mode' : 'Dark mode'
  );

  readonly themeIcon = computed(() =>
    this.layoutService.$themeMode() === 'dark' ? 'weather_sunny' : 'weather_moon'
  );

  readonly authenticatedNavItems = computed<LayoutNavItem[]>(() => {
    const items: LayoutNavItem[] = [
      {
        label: 'Issues list',
        icon: 'clipboard_task_list',
        routerLink: '/issues',
        section: 'Issues'
      },
      {
        label: 'Create issue',
        icon: 'add',
        routerLink: '/issues/create',
        section: 'Issues'
      }
    ];

    if (this.authService.hasPermission(PermissionCodes.usersView)) {
      items.push({
        label: 'Users list',
        icon: 'people',
        routerLink: '/users',
        section: 'Administration'
      });
    }

    if (this.authService.hasPermission(PermissionCodes.rolesView)) {
      items.push({
        label: 'Roles list',
        icon: 'shield',
        routerLink: '/roles',
        section: 'Administration'
      });
    }

    items.push({
      label: 'My account',
      icon: 'person',
      routerLink: '/account',
      section: 'Account'
    });
    return items;
  });

  constructor() {
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        if (this.isMobile()) {
          this.layoutService.closeMobileNav();
        }
      });
  }

  onBreadcrumbItemClick(item: { id: string | number }): void {
    const index = Number(item.id);
    const routerLink = this.pageBreadcrumbs()[index]?.routerLink;
    if (routerLink) {
      void this.router.navigateByUrl(routerLink);
    }
  }

  onMenuToggle(): void {
    if (this.isMobile()) {
      this.layoutService.toggleMobileNav();
      return;
    }

    this.layoutService.toggleSidebarCollapsed();
  }

  onSidebarNavigate(): void {
    if (this.isMobile()) {
      this.layoutService.closeMobileNav();
    }
  }

  closeSidebar(): void {
    if (this.isMobile()) {
      this.layoutService.closeMobileNav();
    }
  }

  navigateToLogin(): void {
    void this.router.navigateByUrl('/login');
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
