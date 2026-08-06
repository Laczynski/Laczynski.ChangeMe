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
import { ToastService as UiToastService } from '@laczynski/ui';

import { LayoutService } from '@core/layout/services/layout.service';
import { UI_TOAST_API } from '@core/toast/services/toast.service';
import { authTokenInterceptor } from '@features/auth/interceptors/auth-token.interceptor';
import { AuthService } from '@features/auth/services/auth.service';
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
    { provide: UI_TOAST_API, useExisting: UiToastService },
    { provide: LOCALE_ID, useValue: 'pl' }
  ]
};
