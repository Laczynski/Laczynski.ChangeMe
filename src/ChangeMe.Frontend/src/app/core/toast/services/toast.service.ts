import { inject, Injectable } from '@angular/core';
import { ToastConfig, ToastSeverity } from '@core/toast/utils/toast.utils';
import { MessageService, ToastMessageOptions } from 'primeng/api';

export type ToastShowOptions = {
  severity?: ToastSeverity;
  summary: string;
  detail?: string;
  life?: number;
  sticky?: boolean;
};

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private readonly messageService = inject(MessageService);

  readonly toastKey = ToastConfig.KEY;

  success(summary: string, detail?: string, life?: number): void {
    this.add({ severity: 'success', summary, detail, life });
  }

  info(summary: string, detail?: string, life?: number): void {
    this.add({ severity: 'info', summary, detail, life });
  }

  warn(summary: string, detail?: string, life?: number): void {
    this.add({ severity: 'warn', summary, detail, life });
  }

  error(summary: string, detail?: string, life?: number): void {
    this.add({
      severity: 'error',
      summary,
      detail,
      life: life ?? ToastConfig.ERROR_LIFE_MS
    });
  }

  showIssueNotification(issueTitle: string, message: string): void {
    this.info(issueTitle, message);
  }

  showApiError(error: unknown, summary = 'Request failed'): void {
    const detail =
      error instanceof Error
        ? error.message
        : 'Something went wrong. Please try again.';
    this.error(summary, detail);
  }

  show(options: ToastShowOptions): void {
    const { severity = 'info', summary, detail, life, sticky } = options;

    this.add({
      severity,
      summary,
      detail,
      life,
      sticky
    });
  }

  clear(): void {
    this.messageService.clear(ToastConfig.KEY);
  }

  private add(message: ToastMessageOptions): void {
    this.messageService.add({
      key: ToastConfig.KEY,
      life: ToastConfig.LIFE_MS,
      closable: true,
      ...message
    });
  }
}
