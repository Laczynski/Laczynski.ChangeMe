import { Injectable, signal } from '@angular/core';
import { ConfirmDialogRequest } from '@core/confirm/models/confirm-dialog.model';

@Injectable({
  providedIn: 'root'
})
export class ConfirmDialogService {
  private readonly _request = signal<ConfirmDialogRequest | null>(null);

  readonly request = this._request.asReadonly();

  confirm(options: ConfirmDialogRequest): void {
    this._request.set({
      acceptLabel: 'Confirm',
      rejectLabel: 'Cancel',
      acceptVariant: 'default',
      ...options
    });
  }

  accept(): void {
    const request = this._request();
    this._request.set(null);
    request?.accept?.();
  }

  reject(): void {
    const request = this._request();
    this._request.set(null);
    request?.reject?.();
  }
}
