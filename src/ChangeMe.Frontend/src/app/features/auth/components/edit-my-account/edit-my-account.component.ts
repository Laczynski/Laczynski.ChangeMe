import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  AccordionComponent,
  ButtonComponent,
  MessageBarComponent,
  SpinnerComponent,
  TextComponent
} from '@laczynski/ui';
import { ToastService } from '@core/toast/services/toast.service';
import { MyAccountDto } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthFieldErrors, AuthMessages } from '@features/auth/utils/auth.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { DefaultExpandedAccordionDirective } from '@shared/directives/default-expanded-accordion.directive';
import { fieldError } from '@shared/forms/field-error';

@Component({
  selector: 'app-edit-my-account',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    BackButtonComponent,
    ButtonComponent,
    TextComponent,
    MessageBarComponent,
    AccordionComponent,
    DefaultExpandedAccordionDirective,
    SpinnerComponent
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
  readonly submitted = signal(false);
  protected readonly fieldError = fieldError;
  protected readonly AuthFieldErrors = AuthFieldErrors;

  readonly form = new FormGroup({
    firstName: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(AuthConstraints.NAME_MAX_LENGTH)
      ]
    }),
    lastName: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(AuthConstraints.NAME_MAX_LENGTH)
      ]
    })
  });

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
          this.form.patchValue({
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

  onSubmit(): void {
    this.submitted.set(true);

    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.submitError.set(null);

    const account = this.account();
    if (!account) {
      this.isSubmitting.set(false);
      return;
    }

    const { firstName, lastName } = this.form.getRawValue();

    this.authService
      .updateMyAccount({ version: account.version, firstName, lastName })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (account) => {
          this.authService.syncProfileToSession(account.firstName, account.lastName);
          this.toastService.success(AuthMessages.profileUpdated);
          void this.router.navigate(['/account']);
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isSubmitting.set(false);
        }
      });
  }
}
