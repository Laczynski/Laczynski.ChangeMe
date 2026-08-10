import { CommonModule } from '@angular/common';
import { Component, inject, output, signal } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationDto } from '@features/notifications/models/notification.model';
import { NotificationsService } from '@features/notifications/services/notifications.service';
import { ButtonDirective } from 'primeng/button';
import { Message } from 'primeng/message';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tab, TabList, TabPanel, TabPanels, Tabs } from 'primeng/tabs';

@Component({
  selector: 'app-notifications-panel',
  imports: [
    CommonModule,
    ButtonDirective,
    Message,
    ProgressSpinner,
    Tabs,
    TabList,
    Tab,
    TabPanels,
    TabPanel
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

  showMoreUnread(): void {
    this.notificationsService.showMoreUnread();
  }

  showMoreRead(): void {
    this.notificationsService.showMoreRead();
  }

  onTabChange(tab: string | number | undefined): void {
    const value: 'unread' | 'read' = tab === 'read' ? 'read' : 'unread';
    if (value === this.activeTab()) {
      return;
    }

    this.activeTab.set(value);

    if (value === 'read') {
      this.notificationsService.reloadReadFromStart();
    } else {
      this.notificationsService.reloadUnreadFromStart();
    }
  }
}
