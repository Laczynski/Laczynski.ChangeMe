import type { BadgeVariants } from '@spartan/ui/badge';

export type BadgeSeverity = 'secondary' | 'success' | 'info' | 'warn' | 'danger';

export function mapBadgeSeverity(
  severity: BadgeSeverity
): NonNullable<BadgeVariants['variant']> {
  switch (severity) {
    case 'danger':
      return 'destructive';
    case 'secondary':
      return 'secondary';
    case 'warn':
      return 'outline';
    default:
      return 'default';
  }
}
