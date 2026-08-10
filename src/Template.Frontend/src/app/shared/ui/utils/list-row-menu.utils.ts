export type ListRowMenuItem = {
  label: string;
  icon?: string;
  routerLink?: (string | number)[];
  variant?: 'default' | 'destructive';
  disabled?: boolean;
  command?: () => void;
  separator?: boolean;
};
