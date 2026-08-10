import { Component, inject, input } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonDirective } from 'primeng/button';

@Component({
  selector: 'app-back-button',
  imports: [ButtonDirective],
  template: `
    <button
      type="button"
      pButton
      size="small"
      severity="secondary"
      [outlined]="true"
      (click)="onBack()"
    >
      <i class="pi pi-arrow-left" aria-hidden="true"></i>
      {{ label() }}
    </button>
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
