import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { form, FormField, FormRoot, maxLength, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { MyAccountDto } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { FormFieldComponent } from '@shared/components/form-field/form-field.component';
import { whenTouched } from '@shared/forms/signal-forms.utils';
import { ButtonDirective } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-edit-my-account',
  imports: [
    FormField,
    FormFieldComponent,
    FormRoot,
    RouterLink,
    BackButtonComponent,
    Card,
    ButtonDirective,
    InputText,
    Message,
    Panel,
    ProgressSpinner
  ],
  templateUrl: './edit-my-account.component.html'
})
export class EditMyAccountComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly account = signal<MyAccountDto | null>(null);
  readonly loadError = signal<string | null>(null);
  readonly submitError = signal<string | null>(null);
  readonly isLoading = signal(true);
  readonly isSubmitting = signal(false);

  readonly profileModel = signal({
    firstName: '',
    lastName: ''
  });

  readonly profileForm = form(
    this.profileModel,
    (path) => {
      required(path.firstName, {
        when: whenTouched,
        message: 'First name is required.'
      });
      maxLength(path.firstName, AuthConstraints.NAME_MAX_LENGTH, {
        message: `First name must be at most ${AuthConstraints.NAME_MAX_LENGTH} characters.`
      });
      required(path.lastName, { when: whenTouched, message: 'Last name is required.' });
      maxLength(path.lastName, AuthConstraints.NAME_MAX_LENGTH, {
        message: `Last name must be at most ${AuthConstraints.NAME_MAX_LENGTH} characters.`
      });
    },
    {
      submission: {
        action: async () => {
          const account = this.account();
          if (!account) {
            return;
          }

          this.isSubmitting.set(true);
          this.submitError.set(null);

          const { firstName, lastName } = this.profileModel();

          try {
            const updatedAccount = await firstValueFrom(
              this.authService.updateMyAccount({
                version: account.version,
                firstName,
                lastName
              })
            );
            this.authService.syncProfileToSession(
              updatedAccount.firstName,
              updatedAccount.lastName
            );
            this.toastService.success(AuthMessages.profileUpdated);
            void this.router.navigate(['/account']);
          } catch (error) {
            this.submitError.set(
              error instanceof Error ? error.message : 'Save failed.'
            );
          } finally {
            this.isSubmitting.set(false);
          }
        }
      }
    }
  );

  constructor() {
    this.reload();
  }

  reload(): void {
    this.isLoading.set(true);
    this.loadError.set(null);
    this.submitError.set(null);

    this.authService
      .getMyAccount()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (account) => {
          this.account.set(account);
          this.profileModel.set({
            firstName: account.firstName,
            lastName: account.lastName
          });
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.loadError.set(error.message);
          this.isLoading.set(false);
        }
      });
  }
}
