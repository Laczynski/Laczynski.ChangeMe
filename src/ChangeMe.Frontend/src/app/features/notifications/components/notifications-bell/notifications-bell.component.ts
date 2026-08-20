import { Component, inject, viewChild } from '@angular/core';
import { NotificationsPanelComponent } from '@features/notifications/components/notifications-panel/notifications-panel.component';
import { NotificationsService } from '@features/notifications/services/notifications.service';
import { ButtonDirective } from 'primeng/button';
import { OverlayBadge } from 'primeng/overlaybadge';
import { Popover } from 'primeng/popover';

@Component({
  selector: 'app-notifications-bell',
  imports: [ButtonDirective, OverlayBadge, Popover, NotificationsPanelComponent],
  template: `
    <p-overlay-badge
      [value]="unreadCount() > 0 ? unreadCount() : null"
      severity="danger"
    >
      <button
        type="button"
        pButton
        [rounded]="true"
        [text]="true"
        severity="secondary"
        [iconOnly]="true"
        aria-label="Notifications"
        (click)="togglePanel($event)"
      >
        <i class="pi pi-bell" aria-hidden="true"></i>
      </button>
    </p-overlay-badge>
    <p-popover #popover [dismissable]="true" contentStyleClass="!p-0 overflow-hidden">
      <ng-template #content>
        <app-notifications-panel (closed)="hidePanel()" />
      </ng-template>
    </p-popover>
  `
})
export class NotificationsBellComponent {
  private readonly notificationsService = inject(NotificationsService);

  private readonly popover = viewChild.required<Popover>('popover');

  readonly unreadCount = this.notificationsService.unreadCount;

  togglePanel(event: Event): void {
    const popover = this.popover();

    if (popover.overlayVisible()) {
      popover.hide();
      return;
    }

    popover.show(event);

    if (popover.container) {
      popover.align();
    }

    if (!this.notificationsService.hasLoaded()) {
      this.notificationsService.loadNotifications();
    }
  }

  hidePanel(): void {
    this.popover().hide();
  }
}
