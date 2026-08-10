import { booleanAttribute, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonDirective } from 'primeng/button';

@Component({
  selector: 'app-auth-page',
  host: { class: 'flex min-h-0 flex-1 flex-col' },
  imports: [ButtonDirective, RouterLink],
  template: `
    <div class="flex flex-1 flex-col items-center justify-center px-4 py-8 md:py-12">
      <div
        class="border-surface-200 bg-surface-0 w-full rounded-2xl border p-6 shadow-sm md:p-8 dark:border-surface-700 dark:bg-surface-900"
        [class.max-w-lg]="wide()"
        [class.max-w-md]="!wide()"
      >
        <header class="mb-6 text-center sm:text-left">
          <h1 class="text-color m-0 text-2xl font-semibold tracking-tight">
            {{ title() }}
          </h1>
          @if (subtitle()) {
            <p class="text-muted-color m-0 mt-2 text-sm leading-relaxed">
              {{ subtitle() }}
            </p>
          }
        </header>

        <ng-content />

        @if (footerPrompt() && footerLinkLabel() && footerRoute()) {
          <div
            class="border-surface-200 mt-8 border-t pt-6 text-center dark:border-surface-700"
          >
            <p
              class="text-muted-color m-0 flex flex-wrap items-center justify-center gap-x-1 gap-y-2 text-sm leading-normal"
            >
              <span>{{ footerPrompt() }}</span>
              <a pButton [link]="true" [routerLink]="footerRoute()">
                {{ footerLinkLabel() }}
              </a>
            </p>
          </div>
        }
      </div>
    </div>
  `
})
export class AuthPageComponent {
  readonly title = input.required<string>();
  readonly subtitle = input('');
  readonly wide = input(false, { transform: booleanAttribute });
  readonly footerPrompt = input('');
  readonly footerLinkLabel = input('');
  readonly footerRoute = input<string | string[]>('');
}
