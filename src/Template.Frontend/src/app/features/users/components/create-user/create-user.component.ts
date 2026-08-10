import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import {
  buildPasswordPolicyValidators,
  defaultPasswordPolicySettings
} from '@features/auth/utils/password-policy.utils';
import { EffectivePermissionsComponent } from '@features/users/components/effective-permissions/effective-permissions.component';
import { EffectivePermissionDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import { UserConstraints, UserMessages } from '@features/users/utils/users.utils';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideEye,
  lucideEyeOff,
  lucideLoader2,
  lucideUserPlus
} from '@ng-icons/lucide';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { HlmAlertImports } from '@spartan/ui/alert';
import { HlmButtonImports } from '@spartan/ui/button';
import { HlmCardImports } from '@spartan/ui/card';
import { HlmFieldImports } from '@spartan/ui/field';
import { HlmInputImports } from '@spartan/ui/input';
import { HlmSelectImports } from '@spartan/ui/select';
import { catchError, debounceTime, of, startWith, switchMap } from 'rxjs';

@Component({
  selector: 'app-create-user',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    BackButtonComponent,
    ...HlmCardImports,
    ...HlmButtonImports,
    ...HlmFieldImports,
    ...HlmInputImports,
    ...HlmSelectImports,
    ...HlmAlertImports,
    NgIcon,
    EffectivePermissionsComponent
  ],
  providers: [provideIcons({ lucideEye, lucideEyeOff, lucideLoader2, lucideUserPlus })],
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

  readonly form = new FormGroup(
    {
      firstName: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.maxLength(UserConstraints.NAME_MAX_LENGTH)
        ]
      }),
      lastName: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.maxLength(UserConstraints.NAME_MAX_LENGTH)
        ]
      }),
      email: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.email,
          Validators.maxLength(UserConstraints.EMAIL_MAX_LENGTH)
        ]
      }),
      password: new FormControl('', {
        nonNullable: true,
        validators: buildPasswordPolicyValidators(defaultPasswordPolicySettings())
      }),
      confirmPassword: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required]
      }),
      roleIds: new FormControl<string[]>([], {
        nonNullable: true,
        validators: [Validators.required]
      })
    },
    { validators: [passwordMatchValidator] }
  );

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

    this.form.controls.roleIds.valueChanges
      .pipe(
        startWith(this.form.controls.roleIds.value),
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

  roleItemToString = (item: unknown): string => {
    const id = String(item);
    const role = this.roleOptions().find((option) => option.id === id);
    return role?.name ?? id;
  };

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.submitError.set(null);

    const raw = this.form.getRawValue();

    this.usersService
      .createUser({
        firstName: raw.firstName.trim(),
        lastName: raw.lastName.trim(),
        email: raw.email.trim(),
        password: raw.password,
        roleIds: raw.roleIds
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.toastService.success(UserMessages.userCreated);
          void this.router.navigate(['/users', user.id]);
        },
        error: (error: Error) => {
          const message = error.message;
          this.submitError.set(
            message.includes('email') ? UserMessages.duplicateEmail : message
          );
          this.isSubmitting.set(false);
        }
      });
  }
}

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  if (!password || !confirmPassword) {
    return null;
  }

  return password === confirmPassword ? null : { passwordMismatch: true };
}
