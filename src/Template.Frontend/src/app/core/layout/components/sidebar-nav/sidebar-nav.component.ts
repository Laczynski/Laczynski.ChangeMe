import { Component, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LayoutNavItem } from '@core/layout/models/layout-nav-item.model';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideList,
  lucidePlus,
  lucideShield,
  lucideUser,
  lucideUsers
} from '@ng-icons/lucide';
import { HlmTooltipImports } from '@spartan/ui/tooltip';

@Component({
  selector: 'app-sidebar-nav',
  imports: [RouterLink, RouterLinkActive, ...HlmTooltipImports, NgIcon],
  providers: [
    provideIcons({ lucideList, lucidePlus, lucideShield, lucideUser, lucideUsers })
  ],
  template: `
    <nav class="flex flex-col gap-1 p-2" aria-label="Main navigation">
      @for (item of items(); track item.routerLink) {
        <a
          [routerLink]="item.routerLink"
          routerLinkActive="bg-primary text-primary-foreground"
          [routerLinkActiveOptions]="{ exact: item.exact ?? false }"
          class="text-foreground hover:bg-muted flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors"
          [class.justify-center]="collapsed()"
          [class.px-2]="collapsed()"
          [hlmTooltip]="collapsed() ? item.label : null"
          position="right"
          [attr.aria-label]="item.label"
          (click)="navigate.emit()"
        >
          <span class="relative inline-flex shrink-0">
            <ng-icon [name]="item.icon" class="text-lg" aria-hidden="true" />
            @if (collapsed() && resolveBadge()?.(item); as count) {
              <span
                class="absolute -top-1 -right-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-0.5 text-[10px] font-semibold text-white dark:bg-red-400"
              >
                {{ count > 9 ? '9+' : count }}
              </span>
            }
          </span>
          @if (!collapsed()) {
            <span class="min-w-0 flex-1 truncate">{{ item.label }}</span>
            @if (resolveBadge()?.(item); as count) {
              <span
                class="ml-auto flex h-5 min-w-5 items-center justify-center rounded-full bg-red-500 px-1 text-xs font-semibold text-white dark:bg-red-400"
              >
                {{ count > 99 ? '99+' : count }}
              </span>
            }
          }
        </a>
      }
    </nav>
  `
})
export class SidebarNavComponent {
  readonly items = input.required<LayoutNavItem[]>();
  readonly collapsed = input(false);
  readonly resolveBadge = input<(item: LayoutNavItem) => number | undefined>();

  readonly navigate = output<void>();
}
