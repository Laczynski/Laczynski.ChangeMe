import { booleanAttribute, Component, inject, input } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonComponent } from '@laczynski/ui';

@Component({
  selector: 'app-auth-page',
  host: {
    class: 'flex flex-1 flex-col items-center justify-center px-4 py-8 md:px-4 md:py-12'
  },
  imports: [ButtonComponent],
  template: `
    <div
      class="w-full rounded-xl border border-stroke bg-surface-1 p-6 md:p-8"
      [class.max-w-lg]="wide()"
      [class.max-w-md]="!wide()"
    >
      <header class="mb-6 text-center sm:text-left">
        <h1 class="m-0 text-2xl font-semibold tracking-tight">{{ title() }}</h1>
        @if (subtitle()) {
          <p class="m-0 mt-2 text-sm leading-normal text-foreground-2">
            {{ subtitle() }}
          </p>
        }
      </header>

      <ng-content />

      @if (footerPrompt() && footerLinkLabel() && footerRoute()) {
        <div class="mt-8 border-t border-stroke pt-6 text-center">
          <p
            class="m-0 flex flex-wrap items-center justify-center gap-x-2 gap-y-1 text-sm text-foreground-2"
          >
            <span>{{ footerPrompt() }}</span>
            <ui-button
              [text]="footerLinkLabel()"
              appearance="subtle"
              (click)="navigateToFooterRoute()"
            />
          </p>
        </div>
      }
    </div>
  `
})
export class AuthPageComponent {
  private readonly router = inject(Router);

  readonly title = input.required<string>();
  readonly subtitle = input('');
  readonly wide = input(false, { transform: booleanAttribute });
  readonly footerPrompt = input('');
  readonly footerLinkLabel = input('');
  readonly footerRoute = input<string | string[]>('');

  navigateToFooterRoute(): void {
    const route = this.footerRoute();
    if (!route) {
      return;
    }

    void this.router.navigate(Array.isArray(route) ? route : [route]);
  }
}
