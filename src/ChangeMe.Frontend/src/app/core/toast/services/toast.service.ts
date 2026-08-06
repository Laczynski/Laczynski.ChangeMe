import { InjectionToken, inject, Injectable } from '@angular/core';

import { ToastConfig, type ToastSeverity } from '@core/toast/utils/toast.utils';

export type UiToastApi = {
  success: (title: string, message?: string, options?: Record<string, unknown>) => string;
  info: (title: string, message?: string, options?: Record<string, unknown>) => string;
  warn: (title: string, message?: string, options?: Record<string, unknown>) => string;
  error: (title: string, message?: string, options?: Record<string, unknown>) => string;
  clear: () => void;
};

export const UI_TOAST_API = new InjectionToken<UiToastApi>('UI_TOAST_API');

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
  private readonly uiToastService = inject(UI_TOAST_API);

  readonly toastKey = ToastConfig.KEY;

  success(summary: string, detail?: string, life?: number): void {
    this.uiToastService.success(summary, detail, { duration: life ?? ToastConfig.LIFE_MS });
  }

  info(summary: string, detail?: string, life?: number): void {
    this.uiToastService.info(summary, detail, { duration: life ?? ToastConfig.LIFE_MS });
  }

  warn(summary: string, detail?: string, life?: number): void {
    this.uiToastService.warn(summary, detail, { duration: life ?? ToastConfig.LIFE_MS });
  }

  error(summary: string, detail?: string, life?: number): void {
    this.uiToastService.error(summary, detail, {
      duration: life ?? ToastConfig.ERROR_LIFE_MS
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
    const duration = life ?? (severity === 'error' ? ToastConfig.ERROR_LIFE_MS : ToastConfig.LIFE_MS);

    switch (severity) {
      case 'success':
        this.uiToastService.success(summary, detail, { duration, sticky });
        break;
      case 'warn':
        this.uiToastService.warn(summary, detail, { duration, sticky });
        break;
      case 'error':
        this.uiToastService.error(summary, detail, { duration, sticky });
        break;
      default:
        this.uiToastService.info(summary, detail, { duration, sticky });
    }
  }

  clear(): void {
    this.uiToastService.clear();
  }
}
