export type LayoutNavSection = 'Issues' | 'Administration' | 'Account';

export type LayoutNavItem = {
  label: string;
  icon: import('@laczynski/ui').IconName;
  routerLink: string;
  section: LayoutNavSection;
  exact?: boolean;
};
