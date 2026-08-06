import { Injectable, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

const SIDEBAR_COLLAPSED_KEY = 'sidebarCollapsed';
const REDUCED_MOTION_CLASS = 'app-reduced-motion';

@Injectable({
  providedIn: 'root'
})
export class LayoutService {
  private readonly _sidebarCollapsed = signal(this.getInitialSidebarCollapsed());
  private readonly _mobileNavOpen = signal(false);
  private readonly _themeMode = signal<ThemeMode>(this.getInitialThemeMode());
  private readonly _prefersReducedMotion = signal(this.getInitialReducedMotion());

  readonly $sidebarCollapsed = this._sidebarCollapsed.asReadonly();
  readonly $mobileNavOpen = this._mobileNavOpen.asReadonly();
  readonly $themeMode = this._themeMode.asReadonly();
  readonly $prefersReducedMotion = this._prefersReducedMotion.asReadonly();

  constructor() {
    this.applyTheme(this._themeMode());
    this.applyReducedMotion(this._prefersReducedMotion());
    this.watchReducedMotionPreference();
  }

  toggleSidebarCollapsed(): void {
    this._sidebarCollapsed.update((collapsed) => !collapsed);
    localStorage.setItem(SIDEBAR_COLLAPSED_KEY, String(this._sidebarCollapsed()));
  }

  openMobileNav(): void {
    this._mobileNavOpen.set(true);
  }

  closeMobileNav(): void {
    this._mobileNavOpen.set(false);
  }

  toggleMobileNav(): void {
    this._mobileNavOpen.update((open) => !open);
  }

  toggleTheme(): void {
    const newTheme = this._themeMode() === 'light' ? 'dark' : 'light';
    this._themeMode.set(newTheme);
    localStorage.setItem('theme', newTheme);
    this.applyTheme(newTheme);
  }

  private getInitialSidebarCollapsed(): boolean {
    return localStorage.getItem(SIDEBAR_COLLAPSED_KEY) === 'true';
  }

  private getInitialThemeMode(): ThemeMode {
    const savedTheme = localStorage.getItem('theme');
    return savedTheme === 'dark' ? 'dark' : 'light';
  }

  private applyTheme(mode: ThemeMode): void {
    document.documentElement.setAttribute('data-theme', mode);

    if (mode === 'dark') {
      document.documentElement.classList.add('dark');
      return;
    }

    document.documentElement.classList.remove('dark');
  }

  private getInitialReducedMotion(): boolean {
    if (typeof window === 'undefined') {
      return false;
    }

    return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  }

  private watchReducedMotionPreference(): void {
    if (typeof window === 'undefined') {
      return;
    }

    const mediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)');
    const listener = (event: MediaQueryListEvent) => {
      this._prefersReducedMotion.set(event.matches);
      this.applyReducedMotion(event.matches);
    };

    mediaQuery.addEventListener('change', listener);
  }

  private applyReducedMotion(enabled: boolean): void {
    document.documentElement.classList.toggle(REDUCED_MOTION_CLASS, enabled);
  }
}
