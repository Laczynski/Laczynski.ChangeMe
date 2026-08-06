import { Component, effect, inject, signal } from '@angular/core';
import { ButtonComponent, PopoverDirective } from '@laczynski/ui';
import { NotificationsPanelComponent } from '@features/notifications/components/notifications-panel/notifications-panel.component';
import { NotificationsService } from '@features/notifications/services/notifications.service';

@Component({
  selector: 'app-notifications-bell',
  imports: [ButtonComponent, PopoverDirective, NotificationsPanelComponent],
  template: `
    <ui-button
      icon="alert"
      variant="secondary"
      appearance="subtle"
      shape="circular"
      ariaLabel="Notifications"
      [badge]="unreadCount() > 0 ? unreadCount().toString() : undefined"
      [uiPopover]="notificationsPanel"
      [(uiPopoverOpen)]="panelOpen"
      uiPopoverPosition="bottom"
      uiPopoverSize="large"
      uiPopoverAriaLabel="Notifications"
    />
    <ng-template #notificationsPanel>
      <app-notifications-panel (closed)="closePanel()" />
    </ng-template>
  `
})
export class NotificationsBellComponent {
  private readonly notificationsService = inject(NotificationsService);

  readonly unreadCount = this.notificationsService.unreadCount;
  readonly panelOpen = signal(false);

  constructor() {
    effect(() => {
      if (this.panelOpen() && !this.notificationsService.hasLoaded()) {
        this.notificationsService.loadNotifications();
      }
    });
  }

  closePanel(): void {
    this.panelOpen.set(false);
  }
}
