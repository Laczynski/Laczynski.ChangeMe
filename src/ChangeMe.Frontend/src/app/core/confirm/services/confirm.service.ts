import { Injectable, signal } from '@angular/core';
import type { Appearance, Variant } from '@laczynski/ui';
import type { ConfirmMessageContent } from '@core/confirm/models/confirm-message.model';

export type ConfirmRequest = {
  header: string;
  message: ConfirmMessageContent;
  acceptLabel?: string;
  rejectLabel?: string;
  acceptVariant?: Variant;
  acceptAppearance?: Appearance;
  rejectVariant?: Variant;
  rejectAppearance?: Appearance;
  accept?: () => void;
  reject?: () => void;
};

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  private readonly _request = signal<ConfirmRequest | null>(null);

  readonly request = this._request.asReadonly();

  confirm(options: ConfirmRequest): void {
    this._request.set({
      acceptLabel: 'Confirm',
      rejectLabel: 'Cancel',
      acceptVariant: 'primary',
      acceptAppearance: 'filled',
      rejectVariant: 'secondary',
      rejectAppearance: 'outline',
      ...options
    });
  }

  accept(): void {
    const current = this._request();
    current?.accept?.();
    this.clear();
  }

  reject(): void {
    const current = this._request();
    current?.reject?.();
    this.clear();
  }

  clear(): void {
    this._request.set(null);
  }
}
