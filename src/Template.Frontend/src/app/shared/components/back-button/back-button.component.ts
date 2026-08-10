import { Component, inject, input } from '@angular/core';
import { Router } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideArrowLeft } from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan/ui/button';

@Component({
  selector: 'app-back-button',
  imports: [...HlmButtonImports, NgIcon],
  providers: [provideIcons({ lucideArrowLeft })],
  template: `
    <button type="button" hlmBtn variant="outline" size="sm" (click)="onBack()">
      <ng-icon name="lucideArrowLeft" aria-hidden="true" />
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
