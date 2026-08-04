import { registerLocaleData } from '@angular/common';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import localePl from '@angular/common/locales/pl';
import {
  ApplicationConfig,
  LOCALE_ID,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners
} from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { providePrimeNG } from 'primeng/config';

import { LayoutService } from '@core/layout/services/layout.service';
import { readCspNonce } from '@core/security/csp-nonce';
import { authTokenInterceptor } from '@features/auth/interceptors/auth-token.interceptor';
import { AuthService } from '@features/auth/services/auth.service';
import Aura from '@primeuix/themes/aura';
import { routes } from './app.routes';

registerLocaleData(localePl);

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([authTokenInterceptor])),
    provideBrowserGlobalErrorListeners(),
    provideAppInitializer(() => {
      inject(LayoutService);
      return inject(AuthService).initializeSession();
    }),
    providePrimeNG({
      // PrimeNG v22+ (PrimeUI) requires a license key for production — see docs/guides/frontend-guidelines.md
      csp: {
        nonce: readCspNonce()
      },
      ripple: true,
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: '.app-dark',
          cssLayer: {
            name: 'primeng',
            order: 'theme, base, primeng'
          }
        }
      }
    }),
    ConfirmationService,
    MessageService,
    { provide: LOCALE_ID, useValue: 'pl' }
  ]
};
