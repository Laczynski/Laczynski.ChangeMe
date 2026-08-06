import { DatePipe } from '@angular/common';
import { Component, computed, inject, output, signal } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationDto } from '@features/notifications/models/notification.model';
import { NotificationsService } from '@features/notifications/services/notifications.service';
import {
  ButtonComponent,
  IconComponent,
  MessageBarComponent,
  SpinnerComponent,
  TabsComponent,
  TagComponent,
  type Tab
} from '@laczynski/ui';

@Component({
  selector: 'app-notifications-panel',
  host: { class: 'block' },
  imports: [
    DatePipe,
    ButtonComponent,
    IconComponent,
    MessageBarComponent,
    SpinnerComponent,
    TabsComponent,
    TagComponent
  ],
  templateUrl: './notifications-panel.component.html'
})
export class NotificationsPanelComponent {
  private readonly router = inject(Router);

  readonly notificationsService = inject(NotificationsService);
  readonly unreadNotifications = this.notificationsService.unreadNotifications;
  readonly readNotifications = this.notificationsService.readNotifications;
  readonly unreadCount = this.notificationsService.unreadCount;
  readonly isLoading = this.notificationsService.isLoading;
  readonly hasLoaded = this.notificationsService.hasLoaded;
  readonly errorMessage = this.notificationsService.errorMessage;
  readonly unreadTotalCount = this.notificationsService.unreadTotalCount;
  readonly readTotalCount = this.notificationsService.readTotalCount;
  readonly canShowMoreUnread = this.notificationsService.canShowMoreUnread;
  readonly canShowMoreRead = this.notificationsService.canShowMoreRead;
  readonly isLoadingMoreUnread = this.notificationsService.isLoadingMoreUnread;
  readonly isLoadingMoreRead = this.notificationsService.isLoadingMoreRead;

  readonly closed = output<void>();

  readonly activeTab = signal<'unread' | 'read'>('unread');
  readonly selectedTabId = signal<string | number>('unread');

  readonly notificationTabs: Tab[] = [
    { id: 'unread', label: 'Unread' },
    { id: 'read', label: 'Read' }
  ];

  readonly displayedNotifications = computed(() =>
    this.activeTab() === 'unread'
      ? this.unreadNotifications()
      : this.readNotifications()
  );

  readonly isUnreadTab = computed(() => this.activeTab() === 'unread');

  readonly canShowMore = computed(() =>
    this.isUnreadTab() ? this.canShowMoreUnread() : this.canShowMoreRead()
  );

  readonly isLoadingMore = computed(() =>
    this.isUnreadTab() ? this.isLoadingMoreUnread() : this.isLoadingMoreRead()
  );

  readonly statusMessage = computed(() => {
    if (this.isLoading() && !this.hasLoaded()) {
      return 'Loading notifications...';
    }

    if (this.unreadCount() === 0) {
      return "You're all caught up.";
    }

    if (this.isUnreadTab()) {
      const shown = this.unreadTotalCount();
      return shown
        ? `${this.unreadCount()} unread · ${shown} shown`
        : `${this.unreadCount()} unread`;
    }

    const shown = this.readTotalCount();
    return shown ? `${shown} read shown` : 'Read notifications';
  });

  readonly emptyTabMessage = computed(() =>
    this.isUnreadTab() ? 'No unread notifications' : 'No read notifications'
  );

  openNotification(notification: NotificationDto): void {
    if (!notification.isRead) {
      this.notificationsService.markAsRead(notification.id);
    }

    this.closed.emit();
    void this.router.navigateByUrl(notification.link);
  }

  markAsRead(event: Event, notification: NotificationDto): void {
    event.stopPropagation();
    event.preventDefault();

    if (!notification.isRead) {
      this.notificationsService.markAsRead(notification.id);
    }
  }

  markAllAsRead(): void {
    this.notificationsService.markAllAsRead();
  }

  showMore(): void {
    if (this.isUnreadTab()) {
      this.notificationsService.showMoreUnread();
    } else {
      this.notificationsService.showMoreRead();
    }
  }

  onTabChange(tabId: string | number): void {
    const value: 'unread' | 'read' = tabId === 'read' ? 'read' : 'unread';
    if (value === this.activeTab()) {
      return;
    }

    this.activeTab.set(value);
    this.selectedTabId.set(value);

    if (value === 'read') {
      this.notificationsService.reloadReadFromStart();
    } else {
      this.notificationsService.reloadUnreadFromStart();
    }
  }
}
