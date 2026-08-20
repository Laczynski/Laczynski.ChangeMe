import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import {
  email,
  form,
  FormField,
  FormRoot,
  maxLength,
  minLength,
  required,
  validate
} from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import {
  applyPasswordPolicyRules,
  defaultPasswordPolicySettings
} from '@features/auth/utils/password-policy.utils';
import { EffectivePermissionsComponent } from '@features/users/components/effective-permissions/effective-permissions.component';
import { EffectivePermissionDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import { UserConstraints, UserMessages } from '@features/users/utils/users.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { FormFieldComponent } from '@shared/components/form-field/form-field.component';
import { whenTouched } from '@shared/forms/signal-forms.utils';
import { ButtonDirective } from 'primeng/button';
import { Card } from 'primeng/card';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { InputPassword } from 'primeng/inputpassword';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Select } from 'primeng/select';
import { catchError, debounceTime, firstValueFrom, of, switchMap } from 'rxjs';

@Component({
  selector: 'app-create-user',
  imports: [
    FormField,
    FormFieldComponent,
    FormRoot,
    FormsModule,
    RouterLink,
    BackButtonComponent,
    Card,
    ButtonDirective,
    IconField,
    InputIcon,
    InputPassword,
    InputText,
    Select,
    Message,
    Panel,
    EffectivePermissionsComponent
  ],
  templateUrl: './create-user.component.html'
})
export class CreateUserComponent {
  private readonly usersService = inject(UsersService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly userConstraints = UserConstraints;
  readonly roleOptions = signal<{ id: string; name: string; isSystem: boolean }[]>([]);
  readonly effectivePermissions = signal<EffectivePermissionDto[]>([]);
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly isLoadingRoles = signal(true);
  passwordMasked = true;
  confirmPasswordMasked = true;

  readonly userModel = signal({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
    roleIds: [] as string[]
  });

  readonly userForm = form(
    this.userModel,
    (path) => {
      required(path.firstName, {
        when: whenTouched,
        message: 'First name is required.'
      });
      maxLength(path.firstName, UserConstraints.NAME_MAX_LENGTH, {
        message: `First name must be at most ${UserConstraints.NAME_MAX_LENGTH} characters.`
      });
      required(path.lastName, { when: whenTouched, message: 'Last name is required.' });
      maxLength(path.lastName, UserConstraints.NAME_MAX_LENGTH, {
        message: `Last name must be at most ${UserConstraints.NAME_MAX_LENGTH} characters.`
      });
      required(path.email, { when: whenTouched, message: 'Email is required.' });
      email(path.email, { message: 'Enter a valid email address.' });
      maxLength(path.email, UserConstraints.EMAIL_MAX_LENGTH, {
        message: `Email must be at most ${UserConstraints.EMAIL_MAX_LENGTH} characters.`
      });

      applyPasswordPolicyRules(path.password, defaultPasswordPolicySettings());
      required(path.confirmPassword, {
        when: whenTouched,
        message: 'Confirm password is required.'
      });
      validate(path.confirmPassword, ({ value, valueOf, state }) => {
        if (!state.touched()) {
          return undefined;
        }

        const confirmPassword = value();
        const password = valueOf(path.password);
        if (!confirmPassword || !password) {
          return undefined;
        }

        return confirmPassword === password
          ? undefined
          : { kind: 'passwordMismatch', message: 'Passwords do not match.' };
      });

      minLength(path.roleIds, 1, {
        when: whenTouched,
        message: 'Select at least one role.'
      });
    },
    {
      submission: {
        action: async () => {
          this.isSubmitting.set(true);
          this.submitError.set(null);

          const raw = this.userModel();

          try {
            const user = await firstValueFrom(
              this.usersService.createUser({
                firstName: raw.firstName.trim(),
                lastName: raw.lastName.trim(),
                email: raw.email.trim(),
                password: raw.password,
                roleIds: raw.roleIds
              })
            );
            this.toastService.success(UserMessages.userCreated);
            void this.router.navigate(['/users', user.id]);
          } catch (error) {
            const message = error instanceof Error ? error.message : 'Create failed.';
            this.submitError.set(
              message.includes('email') ? UserMessages.duplicateEmail : message
            );
          } finally {
            this.isSubmitting.set(false);
          }
        }
      }
    }
  );

  readonly roleIdsSelected = computed(() => this.userModel().roleIds.length > 0);

  constructor() {
    this.usersService
      .getRolesForAssignment()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (roles) => {
          this.roleOptions.set(roles);
          this.isLoadingRoles.set(false);
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isLoadingRoles.set(false);
        }
      });

    toObservable(computed(() => this.userModel().roleIds))
      .pipe(
        debounceTime(200),
        switchMap((roleIds) => {
          if (roleIds.length === 0) {
            this.effectivePermissions.set([]);
            return of([]);
          }

          return this.usersService
            .previewEffectivePermissions({ roleIds })
            .pipe(catchError(() => of([])));
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((permissions) => this.effectivePermissions.set(permissions));
  }

  setRoleIds(roleIds: string[]): void {
    this.userForm.roleIds().value.set(roleIds);
  }
}
