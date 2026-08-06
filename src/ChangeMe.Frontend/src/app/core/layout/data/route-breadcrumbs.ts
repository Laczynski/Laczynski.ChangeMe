import { RouteBreadcrumb } from '@core/layout/models/page-breadcrumb.model';

export const issuesListBreadcrumb: RouteBreadcrumb[] = [{ label: 'Issues list' }];

export const issuesCreateBreadcrumb: RouteBreadcrumb[] = [
  { label: 'Issues list', routerLink: '/issues' },
  { label: 'Create issue' }
];

export const issueDetailsBreadcrumb: RouteBreadcrumb[] = [
  { label: 'Issues list', routerLink: '/issues' },
  { label: 'Issue details' }
];

export const issueEditBreadcrumb: RouteBreadcrumb[] = [
  { label: 'Issues list', routerLink: '/issues' },
  { label: 'Issue details', routerLink: '/issues/:id' },
  { label: 'Edit issue' }
];

export const usersListBreadcrumb: RouteBreadcrumb[] = [{ label: 'Users list' }];

export const usersCreateBreadcrumb: RouteBreadcrumb[] = [
  { label: 'Users list', routerLink: '/users' },
  { label: 'Create user' }
];

export const userDetailsBreadcrumb: RouteBreadcrumb[] = [
  { label: 'Users list', routerLink: '/users' },
  { label: 'User details' }
];

export const userEditBreadcrumb: RouteBreadcrumb[] = [
  { label: 'Users list', routerLink: '/users' },
  { label: 'User details', routerLink: '/users/:id' },
  { label: 'Edit user' }
];

export const rolesListBreadcrumb: RouteBreadcrumb[] = [{ label: 'Roles list' }];

export const rolesCreateBreadcrumb: RouteBreadcrumb[] = [
  { label: 'Roles list', routerLink: '/roles' },
  { label: 'Create role' }
];

export const roleDetailsBreadcrumb: RouteBreadcrumb[] = [
  { label: 'Roles list', routerLink: '/roles' },
  { label: 'Role details' }
];

export const roleEditBreadcrumb: RouteBreadcrumb[] = [
  { label: 'Roles list', routerLink: '/roles' },
  { label: 'Role details', routerLink: '/roles/:id' },
  { label: 'Edit role' }
];

export const myAccountBreadcrumb: RouteBreadcrumb[] = [{ label: 'My account' }];

export const editMyAccountBreadcrumb: RouteBreadcrumb[] = [
  { label: 'My account', routerLink: '/account' },
  { label: 'Edit account' }
];
