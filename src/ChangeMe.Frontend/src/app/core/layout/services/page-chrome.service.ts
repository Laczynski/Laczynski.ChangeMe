import { computed, DestroyRef, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { PageBreadcrumb } from '@core/layout/models/page-breadcrumb.model';
import {
  readRouteBreadcrumbData,
  resolveRouteBreadcrumbs
} from '@core/layout/utils/route-breadcrumb.utils';
import { filter } from 'rxjs/operators';

const PAGE_TITLE_SELECTOR = '.app-shell-page h1';

@Injectable({ providedIn: 'root' })
export class PageChromeService {
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly breadcrumbs = signal<PageBreadcrumb[]>([]);

  readonly currentTitle = computed(() => {
    const crumbs = this.breadcrumbs();
    return crumbs.at(-1)?.label ?? 'ChangeMe';
  });

  private titleObserver: MutationObserver | null = null;
  private dynamicCrumbIndex: number | null = null;

  constructor() {
    this.syncFromActiveRoute();

    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.syncFromActiveRoute());

    this.destroyRef.onDestroy(() => this.disconnectTitleObserver());
  }

  updateCrumb(index: number, partial: Partial<PageBreadcrumb>): void {
    this.breadcrumbs.update((crumbs) => {
      if (index < 0 || index >= crumbs.length) {
        return crumbs;
      }

      const next = [...crumbs];
      next[index] = { ...next[index], ...partial };
      return next;
    });
  }

  private syncFromActiveRoute(): void {
    this.disconnectTitleObserver();

    const leafSnapshot = this.getLeafRouteSnapshot();
    const { breadcrumb, breadcrumbDynamicCrumbIndex } =
      readRouteBreadcrumbData(leafSnapshot);
    const params = this.collectRouteParams(leafSnapshot);

    this.breadcrumbs.set(resolveRouteBreadcrumbs(breadcrumb ?? [], params));
    this.dynamicCrumbIndex = breadcrumbDynamicCrumbIndex ?? null;

    if (this.dynamicCrumbIndex != null) {
      this.observePageTitle();
    }
  }

  private getLeafRouteSnapshot() {
    let route = this.router.routerState.snapshot.root;

    while (route.firstChild) {
      route = route.firstChild;
    }

    return route;
  }

  private collectRouteParams(snapshot: {
    pathFromRoot: { params: Record<string, string> }[];
  }): Record<string, string> {
    const params: Record<string, string> = {};

    for (const route of snapshot.pathFromRoot) {
      Object.assign(params, route.params);
    }

    return params;
  }

  private observePageTitle(): void {
    const syncTitle = () => {
      if (this.dynamicCrumbIndex == null) {
        return;
      }

      const title = document.querySelector(PAGE_TITLE_SELECTOR)?.textContent?.trim();
      if (!title) {
        return;
      }

      this.updateCrumb(this.dynamicCrumbIndex, { label: title });
    };

    const attachObserver = () => {
      const pageRoot = document.querySelector('.app-shell-page');
      if (!pageRoot) {
        requestAnimationFrame(attachObserver);
        return;
      }

      syncTitle();
      this.titleObserver?.disconnect();
      this.titleObserver = new MutationObserver(() => syncTitle());
      this.titleObserver.observe(pageRoot, {
        childList: true,
        subtree: true,
        characterData: true
      });
    };

    requestAnimationFrame(attachObserver);
  }

  private disconnectTitleObserver(): void {
    this.titleObserver?.disconnect();
    this.titleObserver = null;
    this.dynamicCrumbIndex = null;
  }
}
