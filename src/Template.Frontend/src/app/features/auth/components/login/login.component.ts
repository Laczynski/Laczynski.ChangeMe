import { Component, inject, signal } from '@angular/core';
import {
  email,
  form,
  FormField,
  FormRoot,
  maxLength,
  minLength,
  required
} from '@angular/forms/signals';
import { ActivatedRoute } from '@angular/router';
import { AuthPageComponent } from '@features/auth/components/auth-page/auth-page.component';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import { FormFieldComponent } from '@shared/components/form-field/form-field.component';
import { whenTouched } from '@shared/forms/signal-forms.utils';
import { ButtonDirective } from 'primeng/button';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { InputPassword } from 'primeng/inputpassword';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-login',
  host: { class: 'flex min-h-0 flex-1 flex-col' },
  imports: [
    FormField,
    FormFieldComponent,
    FormRoot,
    AuthPageComponent,
    ButtonDirective,
    IconField,
    InputIcon,
    InputPassword,
    InputText,
    Message
  ],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  readonly errorMessage = signal('');
  readonly isSubmitting = signal(false);
  readonly authConstraints = AuthConstraints;
  passwordMasked = true;

  readonly loginModel = signal({
    email: '',
    password: ''
  });

  readonly loginForm = form(
    this.loginModel,
    (path) => {
      required(path.email, { when: whenTouched, message: 'Email is required.' });
      email(path.email, { message: 'Enter a valid email address.' });
      maxLength(path.email, AuthConstraints.EMAIL_MAX_LENGTH, {
        message: `Email must be less than ${AuthConstraints.EMAIL_MAX_LENGTH} characters long.`
      });

      required(path.password, { when: whenTouched, message: 'Password is required.' });
      minLength(path.password, AuthConstraints.PASSWORD_MIN_LENGTH, {
        when: whenTouched,
        message: `Password must be at least ${AuthConstraints.PASSWORD_MIN_LENGTH} characters long.`
      });
      maxLength(path.password, AuthConstraints.PASSWORD_MAX_LENGTH, {
        message: `Password must be less than ${AuthConstraints.PASSWORD_MAX_LENGTH} characters long.`
      });
    },
    {
      submission: {
        action: async () => {
          this.errorMessage.set('');
          this.isSubmitting.set(true);

          try {
            await firstValueFrom(this.authService.login(this.loginModel()));
            const returnUrl =
              this.route.snapshot.queryParamMap.get('returnUrl') ?? '/issues';
            this.authService.continueAfterLogin(returnUrl);
          } catch (error) {
            const message =
              error instanceof Error ? error.message : AuthMessages.invalidCredentials;
            this.errorMessage.set(
              message === AuthMessages.deactivatedAccount
                ? message
                : AuthMessages.invalidCredentials
            );
          } finally {
            this.isSubmitting.set(false);
          }
        }
      }
    }
  );
}
