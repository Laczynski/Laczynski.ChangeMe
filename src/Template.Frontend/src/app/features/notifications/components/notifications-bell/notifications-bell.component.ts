import { Component, computed, inject, signal } from '@angular/core';
import { NotificationsPanelComponent } from '@features/notifications/components/notifications-panel/notifications-panel.component';
import { NotificationsService } from '@features/notifications/services/notifications.service';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideBell } from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan/ui/button';
import { HlmPopoverImports } from '@spartan/ui/popover';

@Component({
  selector: 'app-notifications-bell',
  imports: [
    ...HlmButtonImports,
    ...HlmPopoverImports,
    NotificationsPanelComponent,
    NgIcon
  ],
  providers: [provideIcons({ lucideBell })],
  template: `
    <hlm-popover
      [state]="panelState()"
      [attachTo]="bellButton"
      [closeOnOutsidePointerEvents]="true"
      (stateChanged)="onPopoverStateChanged($event)"
    >
      <span class="relative inline-flex">
        <button
          #bellButton
          type="button"
          hlmBtn
          variant="ghost"
          size="icon"
          aria-label="Notifications"
          (click)="togglePanel()"
        >
          <ng-icon name="lucideBell" aria-hidden="true" />
        </button>
        @if (unreadCount() > 0) {
          <span
            class="bg-destructive text-destructive-foreground pointer-events-none absolute end-0 top-0 flex h-4 min-w-4 -translate-y-1/4 translate-x-1/4 items-center justify-center rounded-full px-0.5 text-[10px] font-semibold leading-none"
          >
            {{ unreadCount() > 9 ? '9+' : unreadCount() }}
          </span>
        }
      </span>
      <ng-template hlmPopoverPortal>
        <div hlmPopoverContent class="overflow-hidden p-0">
          <app-notifications-panel (closed)="hidePanel()" />
        </div>
      </ng-template>
    </hlm-popover>
  `
})
export class NotificationsBellComponent {
  private readonly notificationsService = inject(NotificationsService);

  readonly unreadCount = this.notificationsService.unreadCount;
  private readonly panelOpen = signal(false);
  readonly panelState = computed(() => (this.panelOpen() ? 'open' : 'closed'));

  togglePanel(): void {
    const willOpen = !this.panelOpen();
    this.panelOpen.set(willOpen);

    if (willOpen && !this.notificationsService.hasLoaded()) {
      this.notificationsService.loadNotifications();
    }
  }

  onPopoverStateChanged(state: 'open' | 'closed'): void {
    this.panelOpen.set(state === 'open');
  }

  hidePanel(): void {
    this.panelOpen.set(false);
  }
}
