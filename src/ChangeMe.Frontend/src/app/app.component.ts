import { Component } from '@angular/core';
import { ConfirmDialogComponent } from '@core/confirm/components/confirm-dialog/confirm-dialog.component';
import { AppShellComponent } from '@core/layout/components/app-shell/app-shell.component';
import { ToastContainerComponent } from '@laczynski/ui';

@Component({
  selector: 'app-root',
  imports: [AppShellComponent, ToastContainerComponent, ConfirmDialogComponent],
  template: `
    <app-shell />
    <ui-toast-container />
    <app-confirm-dialog />
  `
})
export class AppComponent {}
