import {
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import {
  email,
  form,
  FormField,
  FormRoot,
  maxLength,
  required,
  validate
} from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { EffectivePermissionsComponent } from '@features/users/components/effective-permissions/effective-permissions.component';
import { EffectivePermissionDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import { UserConstraints, UserMessages } from '@features/users/utils/users.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { FormFieldComponent } from '@shared/components/form-field/form-field.component';
import { whenTouched } from '@shared/forms/signal-forms.utils';
import { ButtonDirective } from 'primeng/button';
import { Card } from 'primeng/card';
import { Checkbox } from 'primeng/checkbox';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import {
  catchError,
  debounceTime,
  firstValueFrom,
  forkJoin,
  of,
  switchMap
} from 'rxjs';

@Component({
  selector: 'app-edit-user',
  imports: [
    FormField,
    FormFieldComponent,
    FormRoot,
    FormsModule,
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
  readonly isEditingSelf = signal(false);

  readonly canManageRoles = this.authService.hasPermission(PermissionCodes.rolesManage);
  readonly canDeactivateUsers = this.authService.hasPermission(
    PermissionCodes.usersDeactivate
  );
  readonly showRolesField = signal(false);
  readonly showStatusField = signal(false);

  readonly userModel = signal({
    firstName: '',
    lastName: '',
    email: '',
    roleIds: [] as string[],
    deactivated: false
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
      validate(path.roleIds, ({ value, state }) => {
        if (!state.touched()) {
          return undefined;
        }

        if (!this.showRolesField()) {
          return undefined;
        }

        return value().length === 0
          ? { kind: 'required', message: 'Select at least one role.' }
          : undefined;
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
              this.usersService.updateUser({
                id: this.id(),
                version: this.recordVersion(),
                firstName: raw.firstName.trim(),
                lastName: raw.lastName.trim(),
                email: raw.email.trim(),
                roleIds: this.showRolesField() ? raw.roleIds : undefined,
                deactivated: this.showStatusField() ? raw.deactivated : undefined
              })
            );
            this.toastService.success(UserMessages.userSaved);
            void this.router.navigate(['/users', user.id]);
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

  readonly pageTitle = computed(() => {
    const model = this.userModel();
    const name = `${model.firstName} ${model.lastName}`.trim();
    return name ? `Edit ${name}` : 'Edit User';
  });

  readonly roleIdsSelected = computed(
    () => this.showRolesField() && this.userModel().roleIds.length > 0
  );

  constructor() {
    toObservable(
      computed(() => ({
        roleIds: this.userModel().roleIds,
        showRoles: this.showRolesField()
      }))
    )
      .pipe(
        debounceTime(200),
        switchMap(({ roleIds, showRoles }) => {
          if (!showRoles || roleIds.length === 0) {
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

  setRoleIds(roleIds: string[]): void {
    this.userForm.roleIds().value.set(roleIds);
  }

  private loadUser(): void {
    const userId = this.id();
    const currentUserId = this.authService.currentUser()?.id;
    this.isEditingSelf.set(currentUserId === userId);
    this.showRolesField.set(this.canManageRoles && currentUserId !== userId);
    this.showStatusField.set(this.canDeactivateUsers && currentUserId !== userId);

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
          this.userModel.set({
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
}
