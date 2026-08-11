import { computed, effect, inject, Injectable, signal, untracked } from '@angular/core';
import { GridQuery } from '@laczynski/datagrid';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { ApiService } from '@shared/api/services/api.service';
import { createGridQuery, hasMoreGridItems } from '@shared/data/utils/grid.utils';
import {
  NotificationDto,
  NotificationListDto,
  NotificationRealtimeMessage
} from '../models/notification.model';
import { NotificationsRealtimeConnectionService } from './notifications-realtime-connection.service';

const NOTIFICATION_GRID_SORT = [{ field: 'CreatedAt', desc: true }];

@Injectable({
  providedIn: 'root'
})
export class NotificationsService {
  private readonly apiService = inject(ApiService);
  private readonly authService = inject(AuthService);
  private readonly realtimeConnectionService = inject(
    NotificationsRealtimeConnectionService
  );
  private readonly toastService = inject(ToastService);

  readonly unreadNotifications = signal<NotificationDto[]>([]);
  readonly readNotifications = signal<NotificationDto[]>([]);
  readonly unreadTotalCount = signal(0);
  readonly readTotalCount = signal(0);
  readonly unreadCount = signal(0);
  readonly isLoading = signal(false);
  readonly isLoadingMoreUnread = signal(false);
  readonly isLoadingMoreRead = signal(false);
  readonly hasLoaded = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly canShowMoreUnread = computed(() =>
    hasMoreGridItems(this.unreadNotifications().length, this.unreadTotalCount())
  );
  readonly canShowMoreRead = computed(() =>
    hasMoreGridItems(this.readNotifications().length, this.readTotalCount())
  );

  readonly notifications = computed(() => [
    ...this.unreadNotifications(),
    ...this.readNotifications()
  ]);

  private unreadRequestId = 0;
  private readRequestId = 0;
  private hasInitializedForAuth = false;
  private lastHandledReconnectCount = 0;
  private lastProcessedNotificationVersion = 0;

  constructor() {
    effect(() => {
      const isAuthenticated = this.authService.isAuthenticated();
      if (!isAuthenticated) {
        this.resetAuthState();
        return;
      }

      if (this.hasInitializedForAuth) {
        return;
      }

      this.hasInitializedForAuth = true;
      untracked(() => this.reloadUnreadFromStart());
    });

    effect(() => {
      const version = this.realtimeConnectionService.notificationMessageVersion();
      if (
        version === 0 ||
        version === this.lastProcessedNotificationVersion ||
        !this.authService.isAuthenticated()
      ) {
        return;
      }

      const message = this.realtimeConnectionService.lastNotificationMessage();
      if (!message) {
        return;
      }

      this.lastProcessedNotificationVersion = version;
      untracked(() => this.syncFromRealtime(message));
    });

    effect(() => {
      const reconnectCount = this.realtimeConnectionService.reconnectCount();
      if (
        reconnectCount === 0 ||
        reconnectCount <= this.lastHandledReconnectCount ||
        !this.authService.isAuthenticated()
      ) {
        return;
      }

      this.lastHandledReconnectCount = reconnectCount;
      untracked(() => this.reloadUnreadFromStart());
    });
  }

  private resetAuthState(): void {
    this.hasInitializedForAuth = false;
    this.lastHandledReconnectCount = 0;
    this.lastProcessedNotificationVersion = 0;
    this.unreadNotifications.set([]);
    this.readNotifications.set([]);
    this.unreadTotalCount.set(0);
    this.readTotalCount.set(0);
    this.unreadCount.set(0);
    this.hasLoaded.set(false);
    this.errorMessage.set(null);
  }

  loadNotifications(): void {
    this.reloadUnreadFromStart();
  }

  reloadUnreadFromStart(): void {
    this.loadUnreadNotifications();
  }

  reloadReadFromStart(): void {
    this.loadReadNotifications();
  }

  showMoreUnread(): void {
    if (!this.canShowMoreUnread() || this.isLoadingMoreUnread()) {
      return;
    }

    this.loadUnreadNotifications({
      append: true,
      skip: this.unreadNotifications().length
    });
  }

  showMoreRead(): void {
    if (!this.canShowMoreRead() || this.isLoadingMoreRead()) {
      return;
    }

    this.loadReadNotifications({
      append: true,
      skip: this.readNotifications().length
    });
  }

  loadUnreadNotifications(options: { append?: boolean; skip?: number } = {}): void {
    const append = options.append ?? false;
    const skip = options.skip ?? 0;
    const requestId = ++this.unreadRequestId;

    this.loadNotificationsPage(
      false,
      createGridQuery({ skip, sort: NOTIFICATION_GRID_SORT }),
      requestId,
      append,
      (result) => {
        if (requestId !== this.unreadRequestId) {
          return;
        }

        if (append) {
          const current = this.unreadNotifications();
          const merged = this.appendUniqueNotifications(current, result.page.items);
          const addedCount = merged.length - current.length;
          this.unreadNotifications.set(merged);
          if (
            addedCount === 0 &&
            hasMoreGridItems(merged.length, result.page.totalCount)
          ) {
            this.unreadTotalCount.set(merged.length);
          } else {
            this.unreadTotalCount.set(result.page.totalCount);
          }
        } else {
          this.unreadNotifications.set(result.page.items);
          this.unreadTotalCount.set(result.page.totalCount);
        }

        this.unreadCount.set(result.unreadCount);
        this.isLoadingMoreUnread.set(false);
      },
      true
    );
  }

  loadReadNotifications(options: { append?: boolean; skip?: number } = {}): void {
    const append = options.append ?? false;
    const skip = options.skip ?? 0;
    const requestId = ++this.readRequestId;

    this.loadNotificationsPage(
      true,
      createGridQuery({ skip, sort: NOTIFICATION_GRID_SORT }),
      requestId,
      append,
      (result) => {
        if (requestId !== this.readRequestId) {
          return;
        }

        if (append) {
          const current = this.readNotifications();
          const merged = this.appendUniqueNotifications(current, result.page.items);
          const addedCount = merged.length - current.length;
          this.readNotifications.set(merged);
          if (
            addedCount === 0 &&
            hasMoreGridItems(merged.length, result.page.totalCount)
          ) {
            this.readTotalCount.set(merged.length);
          } else {
            this.readTotalCount.set(result.page.totalCount);
          }
        } else {
          this.readNotifications.set(result.page.items);
          this.readTotalCount.set(result.page.totalCount);
        }

        this.unreadCount.set(result.unreadCount);
        this.isLoadingMoreRead.set(false);
      },
      false
    );
  }

  markAsRead(notificationId: string): void {
    this.apiService
      .put<NotificationDto>(`notifications/${notificationId}/read`, {})
      .subscribe({
        next: (notification) => {
          const wasUnread = this.unreadNotifications().some(
            (item) => item.id === notification.id && !item.isRead
          );
          this.unreadNotifications.update((items) =>
            items.filter((item) => item.id !== notification.id)
          );
          this.readNotifications.update((items) => {
            if (items.some((item) => item.id === notification.id)) {
              return items.map((item) =>
                item.id === notification.id ? notification : item
              );
            }

            return [notification, ...items];
          });
          if (wasUnread) {
            this.unreadCount.update((count) => Math.max(0, count - 1));
            this.unreadTotalCount.update((count) => Math.max(0, count - 1));
          }
        }
      });
  }

  markAllAsRead(): void {
    this.apiService.put<boolean>('notifications/read-all', {}).subscribe({
      next: () => {
        this.unreadNotifications.set([]);
        this.unreadTotalCount.set(0);
        this.unreadCount.set(0);
        this.reloadReadFromStart();
      }
    });
  }

  private appendUniqueNotifications(
    existing: NotificationDto[],
    incoming: NotificationDto[]
  ): NotificationDto[] {
    const existingIds = new Set(existing.map((item) => item.id));
    const uniqueIncoming = incoming.filter((item) => !existingIds.has(item.id));
    return [...existing, ...uniqueIncoming];
  }

  syncFromRealtime(message: NotificationRealtimeMessage): void {
    const notification: NotificationDto = {
      id: message.notificationId,
      issueId: message.issueId,
      eventType: message.eventType,
      issueTitle: message.issueTitle,
      message: message.message,
      link: message.link,
      createdAt: message.createdAt,
      isRead: false,
      readAt: null
    };

    this.unreadNotifications.update((items) => {
      if (items.some((item) => item.id === notification.id)) {
        return items;
      }

      return [notification, ...items];
    });

    this.unreadCount.update((count) => count + 1);
    this.unreadTotalCount.update((count) => count + 1);
    this.toastService.showIssueNotification(
      notification.issueTitle,
      notification.message
    );
  }

  private loadNotificationsPage(
    isRead: boolean,
    grid: GridQuery,
    requestId: number,
    append: boolean,
    onSuccess: (result: NotificationListDto) => void,
    isUnreadList: boolean
  ): void {
    if (!this.authService.isAuthenticated()) {
      this.unreadNotifications.set([]);
      this.readNotifications.set([]);
      this.unreadCount.set(0);
      this.hasLoaded.set(false);
      return;
    }

    if (append) {
      if (isUnreadList) {
        this.isLoadingMoreUnread.set(true);
      } else {
        this.isLoadingMoreRead.set(true);
      }
    } else {
      this.isLoading.set(true);
    }

    this.errorMessage.set(null);

    this.apiService
      .get<NotificationListDto>('notifications', { isRead, grid })
      .subscribe({
        next: (result) => {
          const isStale =
            (isUnreadList && requestId !== this.unreadRequestId) ||
            (!isUnreadList && requestId !== this.readRequestId);
          if (isStale) {
            this.clearLoadingFlags(append, isUnreadList);
            return;
          }

          onSuccess(result);
          this.hasLoaded.set(true);
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          const isStale =
            (isUnreadList && requestId !== this.unreadRequestId) ||
            (!isUnreadList && requestId !== this.readRequestId);
          if (isStale) {
            this.clearLoadingFlags(append, isUnreadList);
            return;
          }

          this.errorMessage.set(error.message);
          this.isLoading.set(false);
          this.isLoadingMoreUnread.set(false);
          this.isLoadingMoreRead.set(false);
        }
      });
  }

  private clearLoadingFlags(append: boolean, isUnreadList: boolean): void {
    if (!append) {
      return;
    }

    if (isUnreadList) {
      this.isLoadingMoreUnread.set(false);
    } else {
      this.isLoadingMoreRead.set(false);
    }
  }
}
