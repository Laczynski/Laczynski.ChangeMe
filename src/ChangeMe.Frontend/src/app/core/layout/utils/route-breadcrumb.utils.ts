import { ActivatedRouteSnapshot, Params } from '@angular/router';
import {
  PageBreadcrumb,
  RouteBreadcrumb,
  RouteBreadcrumbData
} from '@core/layout/models/page-breadcrumb.model';

function substituteRouteParams(value: string, params: Params): string {
  return value.replace(/:([A-Za-z0-9_]+)/g, (_, key: string) => params[key] ?? '');
}

export function resolveRouteBreadcrumbs(
  crumbs: RouteBreadcrumb[],
  params: Params
): PageBreadcrumb[] {
  return crumbs.map((crumb) => ({
    label: crumb.label,
    routerLink: crumb.routerLink
      ? substituteRouteParams(crumb.routerLink, params)
      : undefined
  }));
}

export function readRouteBreadcrumbData(
  snapshot: ActivatedRouteSnapshot
): RouteBreadcrumbData {
  const breadcrumbs: RouteBreadcrumb[] = [];
  let dynamicCrumbIndex: number | undefined;

  for (const route of snapshot.pathFromRoot) {
    const data = route.data as RouteBreadcrumbData;
    const start = breadcrumbs.length;

    if (data.breadcrumb?.length) {
      breadcrumbs.push(...data.breadcrumb);
    }

    if (data.breadcrumbDynamicCrumbIndex != null) {
      dynamicCrumbIndex = start + data.breadcrumbDynamicCrumbIndex;
    }
  }

  return {
    breadcrumb: breadcrumbs,
    breadcrumbDynamicCrumbIndex: dynamicCrumbIndex
  };
}
