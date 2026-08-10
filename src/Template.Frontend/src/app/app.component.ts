import { Component, computed, inject } from '@angular/core';
import { ConfirmDialogHostComponent } from '@core/confirm/components/confirm-dialog-host/confirm-dialog-host.component';
import { AppShellComponent } from '@core/layout/components/app-shell/app-shell.component';
import { LayoutService } from '@core/layout/services/layout.service';
import { HlmToasterImports } from '@spartan/ui/sonner';

@Component({
  selector: 'app-root',
  imports: [AppShellComponent, ConfirmDialogHostComponent, ...HlmToasterImports],
  template: `
    <app-shell />
    <hlm-toaster
      position="top-right"
      [closeButton]="true"
      [theme]="toasterTheme()"
      [richColors]="true"
    />
    <app-confirm-dialog-host />
  `
})
export class AppComponent {
  private readonly layoutService = inject(LayoutService);

  readonly toasterTheme = computed(() =>
    this.layoutService.$themeMode() === 'dark' ? 'dark' : 'light'
  );
}
