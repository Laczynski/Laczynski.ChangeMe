import { Injectable } from '@angular/core';
import { ToastConfig, ToastSeverity } from '@core/toast/utils/toast.utils';
import { toast } from '@spartan-ng/brain/sonner';

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
  readonly toastKey = ToastConfig.KEY;

  success(summary: string, detail?: string, life?: number): void {
    this.add('success', summary, detail, life);
  }

  info(summary: string, detail?: string, life?: number): void {
    this.add('info', summary, detail, life);
  }

  warn(summary: string, detail?: string, life?: number): void {
    this.add('warn', summary, detail, life);
  }

  error(summary: string, detail?: string, life?: number): void {
    this.add('error', summary, detail, life ?? ToastConfig.ERROR_LIFE_MS);
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
    this.add(severity, summary, detail, life, sticky);
  }

  clear(): void {
    toast.dismiss();
  }

  private add(
    severity: ToastSeverity,
    summary: string,
    detail?: string,
    life?: number,
    sticky?: boolean
  ): void {
    const duration = sticky ? Number.MAX_SAFE_INTEGER : (life ?? ToastConfig.LIFE_MS);
    const options = {
      description: detail,
      duration,
      dismissible: true
    };

    switch (severity) {
      case 'success':
        toast.success(summary, options);
        break;
      case 'error':
        toast.error(summary, options);
        break;
      case 'warn':
        toast.warning(summary, options);
        break;
      default:
        toast.info(summary, options);
        break;
    }
  }
}
