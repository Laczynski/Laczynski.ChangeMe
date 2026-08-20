import { DestroyRef, Injectable, effect, inject, signal } from '@angular/core';
import { getNotificationsHubUrl } from '@environments/runtime-config';
import { AuthService } from '@features/auth/services/auth.service';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState
} from '@microsoft/signalr';
import { NotificationRealtimeMessage } from '../models/notification.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationsRealtimeConnectionService {
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  private connection: HubConnection | null = null;

  readonly lastNotificationMessage = signal<NotificationRealtimeMessage | null>(null);
  readonly notificationMessageVersion = signal(0);
  readonly connectionState = signal<'disconnected' | 'connecting' | 'connected'>(
    'disconnected'
  );
  readonly reconnectCount = signal(0);

  constructor() {
    effect(() => {
      const token = this.authService.token();
      if (!token) {
        void this.disconnect();
        return;
      }

      void this.connect();
    });

    this.destroyRef.onDestroy(() => {
      void this.disconnect();
    });
  }

  private async connect(): Promise<void> {
    if (
      this.connection &&
      (this.connection.state === HubConnectionState.Connected ||
        this.connection.state === HubConnectionState.Connecting ||
        this.connection.state === HubConnectionState.Reconnecting)
    ) {
      return;
    }

    this.connectionState.set('connecting');

    this.connection = new HubConnectionBuilder()
      .withUrl(this.getHubUrl(), {
        accessTokenFactory: () => this.authService.token() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on(
      'notificationReceived',
      (message: NotificationRealtimeMessage) => {
        this.lastNotificationMessage.set(message);
        this.notificationMessageVersion.update((value) => value + 1);
      }
    );

    this.connection.onreconnected(() => {
      this.connectionState.set('connected');
      this.reconnectCount.update((count) => count + 1);
    });

    this.connection.onclose(() => {
      this.connectionState.set('disconnected');
    });

    try {
      await this.connection.start();
      this.connectionState.set('connected');
    } catch {
      this.connectionState.set('disconnected');
    }
  }

  private async disconnect(): Promise<void> {
    if (!this.connection) {
      this.connectionState.set('disconnected');
      return;
    }

    const currentConnection = this.connection;
    this.connection = null;
    this.lastNotificationMessage.set(null);
    this.connectionState.set('disconnected');

    try {
      await currentConnection.stop();
    } catch {
      this.connectionState.set('disconnected');
    }
  }

  private getHubUrl(): string {
    return getNotificationsHubUrl();
  }
}
