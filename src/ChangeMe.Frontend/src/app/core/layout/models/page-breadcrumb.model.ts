export type PageBreadcrumb = {
  label: string;
  routerLink?: string;
};

export type RouteBreadcrumb = {
  label: string;
  routerLink?: string;
};

export type RouteBreadcrumbData = {
  breadcrumb?: RouteBreadcrumb[];
  breadcrumbDynamicCrumbIndex?: number;
};
