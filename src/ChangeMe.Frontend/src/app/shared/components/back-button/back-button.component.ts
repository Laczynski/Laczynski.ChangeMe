import { Component, inject, input } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonComponent } from '@laczynski/ui';

@Component({
  selector: 'app-back-button',
  imports: [ButtonComponent],
  template: `
    <ui-button
      [text]="label()"
      icon="arrow_left"
      variant="secondary"
      appearance="outline"
      (click)="onBack()"
    />
  `
})
export class BackButtonComponent {
  private readonly router = inject(Router);

  readonly label = input.required<string>();
  readonly route = input.required<readonly string[]>();

  onBack(): void {
    void this.router.navigate(this.route());
  }
}
