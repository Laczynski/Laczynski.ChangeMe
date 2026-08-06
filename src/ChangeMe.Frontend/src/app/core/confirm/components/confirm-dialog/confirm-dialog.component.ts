import { Component, computed, inject } from '@angular/core';
import { ConfirmService } from '@core/confirm/services/confirm.service';
import { isConfirmMessageParts } from '@core/confirm/models/confirm-message.model';
import { DialogComponent, type QuickAction } from '@laczynski/ui';

@Component({
  selector: 'app-confirm-dialog',
  imports: [DialogComponent],
  template: `
    @if (confirmService.request(); as request) {
      <ui-dialog
        [title]="request.header"
        [visible]="true"
        (visibleChange)="onVisibleChange($event)"
        [primaryAction]="primaryAction()"
        [secondaryAction]="secondaryAction()"
      >
        @if (isConfirmMessageParts(request.message)) {
          <p>
            @for (part of request.message; track $index) {
              @if (part.type === 'strong') {
                <strong>{{ part.value }}</strong>
              } @else {
                {{ part.value }}
              }
            }
          </p>
        } @else {
          <p>{{ request.message }}</p>
        }
      </ui-dialog>
    }
  `
})
export class ConfirmDialogComponent {
  readonly confirmService = inject(ConfirmService);
  protected readonly isConfirmMessageParts = isConfirmMessageParts;

  readonly primaryAction = computed<QuickAction | null>(() => {
    const request = this.confirmService.request();
    if (!request) {
      return null;
    }

    return {
      label: request.acceptLabel ?? 'Confirm',
      variant: request.acceptVariant ?? 'primary',
      appearance: request.acceptAppearance ?? 'filled',
      action: () => this.confirmService.accept()
    };
  });

  readonly secondaryAction = computed<QuickAction | null>(() => {
    const request = this.confirmService.request();
    if (!request) {
      return null;
    }

    return {
      label: request.rejectLabel ?? 'Cancel',
      variant: request.rejectVariant ?? 'secondary',
      appearance: request.rejectAppearance ?? 'outline',
      action: () => this.confirmService.reject()
    };
  });

  onVisibleChange(visible: boolean): void {
    if (!visible) {
      this.confirmService.reject();
    }
  }
}
