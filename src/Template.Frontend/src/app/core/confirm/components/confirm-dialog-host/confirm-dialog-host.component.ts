import { Component, computed, inject } from '@angular/core';
import { ConfirmDialogService } from '@core/confirm/services/confirm-dialog.service';
import { HlmAlertDialogImports } from '@spartan/ui/alert-dialog';
import { HlmButtonImports } from '@spartan/ui/button';

@Component({
  selector: 'app-confirm-dialog-host',
  imports: [...HlmAlertDialogImports, ...HlmButtonImports],
  template: `
    <hlm-alert-dialog [state]="dialogState()">
      <hlm-alert-dialog-content *hlmAlertDialogPortal>
        <hlm-alert-dialog-header>
          <h2 hlmAlertDialogTitle>{{ request()?.header }}</h2>
          <p hlmAlertDialogDescription [innerHTML]="request()?.message ?? ''"></p>
        </hlm-alert-dialog-header>
        <hlm-alert-dialog-footer>
          <button type="button" hlmAlertDialogCancel (click)="onReject()">
            {{ request()?.rejectLabel }}
          </button>
          <button
            type="button"
            hlmAlertDialogAction
            [variant]="
              request()?.acceptVariant === 'destructive' ? 'destructive' : 'default'
            "
            (click)="onAccept()"
          >
            {{ request()?.acceptLabel }}
          </button>
        </hlm-alert-dialog-footer>
      </hlm-alert-dialog-content>
    </hlm-alert-dialog>
  `
})
export class ConfirmDialogHostComponent {
  private readonly confirmDialogService = inject(ConfirmDialogService);

  readonly request = this.confirmDialogService.request;
  readonly dialogState = computed(() => (this.request() ? 'open' : 'closed'));

  onAccept(): void {
    this.confirmDialogService.accept();
  }

  onReject(): void {
    this.confirmDialogService.reject();
  }
}
