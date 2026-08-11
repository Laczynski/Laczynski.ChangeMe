import {
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { EffectivePermissionsComponent } from '@features/users/components/effective-permissions/effective-permissions.component';
import { EffectivePermissionDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import { UserConstraints, UserMessages } from '@features/users/utils/users.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { ButtonDirective } from 'primeng/button';
import { Card } from 'primeng/card';
import { Checkbox } from 'primeng/checkbox';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { catchError, debounceTime, forkJoin, of, startWith, switchMap } from 'rxjs';

@Component({
  selector: 'app-edit-user',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    BackButtonComponent,
    Card,
    ButtonDirective,
    InputText,
    Select,
    Checkbox,
    Message,
    Panel,
    ProgressSpinner,
    EffectivePermissionsComponent
  ],
  templateUrl: './edit-user.component.html'
})
export class EditUserComponent {
  readonly UserMessages = UserMessages;

  readonly id = input.required<string>();

  private readonly usersService = inject(UsersService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly roleOptions = signal<{ id: string; name: string; isSystem: boolean }[]>([]);
  readonly effectivePermissions = signal<EffectivePermissionDto[]>([]);
  readonly submitError = signal<string | null>(null);
  readonly recordVersion = signal(0);
  readonly loadError = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly isLoading = signal(true);
  readonly pageTitle = computed(() => {
    const name =
      `${this.form.controls.firstName.value} ${this.form.controls.lastName.value}`.trim();
    return name ? `Edit ${name}` : 'Edit User';
  });
  readonly isEditingSelf = signal(false);

  readonly canManageRoles = this.authService.hasPermission(PermissionCodes.rolesManage);
  readonly canDeactivateUsers = this.authService.hasPermission(
    PermissionCodes.usersDeactivate
  );
  readonly showRolesField = signal(false);
  readonly showStatusField = signal(false);

  readonly form = new FormGroup({
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
    roleIds: new FormControl<string[]>([], { nonNullable: true }),
    deactivated: new FormControl(false, { nonNullable: true })
  });

  constructor() {
    this.form.controls.roleIds.valueChanges
      .pipe(
        startWith(this.form.controls.roleIds.value),
        debounceTime(200),
        switchMap((roleIds) => {
          if (!this.showRolesField() || roleIds.length === 0) {
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

    effect(() => {
      this.id();
      this.loadUser();
    });
  }

  refresh(): void {
    this.loadUser();
  }

  private loadUser(): void {
    const userId = this.id();
    const currentUserId = this.authService.currentUser()?.id;
    this.isEditingSelf.set(currentUserId === userId);
    this.showRolesField.set(this.canManageRoles && currentUserId !== userId);
    this.showStatusField.set(this.canDeactivateUsers && currentUserId !== userId);

    if (this.showRolesField()) {
      this.form.controls.roleIds.setValidators([Validators.required]);
    } else {
      this.form.controls.roleIds.clearValidators();
    }

    this.isLoading.set(true);
    this.loadError.set(null);
    this.submitError.set(null);

    forkJoin({
      user: this.usersService.getUserById(userId),
      roles: this.showRolesField() ? this.usersService.getRolesForAssignment() : of([])
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ user, roles }) => {
          this.recordVersion.set(user.version);
          this.form.patchValue({
            firstName: user.firstName,
            lastName: user.lastName,
            email: user.email,
            roleIds: user.roles.map((role) => role.id),
            deactivated: user.deactivated
          });

          if (this.showRolesField()) {
            this.roleOptions.set(roles);
            this.effectivePermissions.set(user.effectivePermissions);
          }

          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.loadError.set(error.message);
          this.isLoading.set(false);
        }
      });
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.submitError.set(null);

    const raw = this.form.getRawValue();

    this.usersService
      .updateUser({
        id: this.id(),
        version: this.recordVersion(),
        firstName: raw.firstName.trim(),
        lastName: raw.lastName.trim(),
        email: raw.email.trim(),
        roleIds: this.showRolesField() ? raw.roleIds : undefined,
        deactivated: this.showStatusField() ? raw.deactivated : undefined
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.toastService.success(UserMessages.userSaved);
          void this.router.navigate(['/users', user.id]);
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isSubmitting.set(false);
        }
      });
  }

  shouldShowError(control: FormControl<string | boolean | string[]>): boolean {
    return control.touched && control.invalid;
  }
}
