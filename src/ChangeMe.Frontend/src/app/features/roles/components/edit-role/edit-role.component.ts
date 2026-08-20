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
  form,
  FormField,
  FormRoot,
  maxLength,
  minLength,
  required
} from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { PermissionChecklistComponent } from '@features/roles/components/permission-checklist/permission-checklist.component';
import { PermissionCatalogItemDto } from '@features/roles/models/role.model';
import { RolesService } from '@features/roles/services/roles.service';
import { RoleConstraints, RoleMessages } from '@features/roles/utils/roles.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { FormFieldComponent } from '@shared/components/form-field/form-field.component';
import { whenTouched } from '@shared/forms/signal-forms.utils';
import { ButtonDirective } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Textarea } from 'primeng/textarea';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-edit-role',
  imports: [
    FormField,
    FormFieldComponent,
    FormRoot,
    RouterLink,
    BackButtonComponent,
    Card,
    ButtonDirective,
    InputText,
    Textarea,
    Message,
    Panel,
    ProgressSpinner,
    PermissionChecklistComponent
  ],
  templateUrl: './edit-role.component.html'
})
export class EditRoleComponent {
  readonly id = input.required<string>();

  private readonly rolesService = inject(RolesService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly roleConstraints = RoleConstraints;
  readonly RoleMessages = RoleMessages;
  readonly catalog = signal<PermissionCatalogItemDto[]>([]);
  readonly submitError = signal<string | null>(null);
  readonly recordVersion = signal(0);
  readonly loadError = signal<string | null>(null);
  readonly permissionsError = signal(false);
  readonly isLoading = signal(true);
  readonly isSubmitting = signal(false);

  readonly roleModel = signal({
    name: '',
    description: '',
    permissionCodes: [] as string[]
  });

  readonly roleForm = form(
    this.roleModel,
    (path) => {
      required(path.name, { when: whenTouched, message: 'Name is required.' });
      minLength(path.name, RoleConstraints.NAME_MIN_LENGTH, {
        when: whenTouched,
        message: `Name must be at least ${RoleConstraints.NAME_MIN_LENGTH} characters.`
      });
      maxLength(path.name, RoleConstraints.NAME_MAX_LENGTH, {
        message: `Name must be at most ${RoleConstraints.NAME_MAX_LENGTH} characters.`
      });
      maxLength(path.description, RoleConstraints.DESCRIPTION_MAX_LENGTH, {
        message: RoleMessages.descriptionTooLong
      });
      minLength(path.permissionCodes, 1, {
        when: whenTouched,
        message: RoleMessages.atLeastOnePermission
      });
    },
    {
      submission: {
        action: async () => {
          this.submitError.set(null);
          this.permissionsError.set(false);
          this.isSubmitting.set(true);

          const raw = this.roleModel();

          try {
            const role = await firstValueFrom(
              this.rolesService.updateRole({
                id: this.id(),
                version: this.recordVersion(),
                name: raw.name,
                description: raw.description.trim() ? raw.description : null,
                permissionCodes: raw.permissionCodes
              })
            );
            this.toastService.success(RoleMessages.roleSaved);
            void this.router.navigate(['/roles', role.id]);
          } catch (error) {
            this.submitError.set(
              error instanceof Error ? error.message : 'Save failed.'
            );
          } finally {
            this.isSubmitting.set(false);
          }
        },
        onInvalid: () => {
          if (this.roleForm.permissionCodes().invalid()) {
            this.permissionsError.set(true);
          }
        }
      }
    }
  );

  readonly pageTitle = computed(() => {
    const name = this.roleModel().name.trim();
    return name ? `Edit ${name}` : 'Edit Role';
  });

  constructor() {
    this.rolesService
      .getPermissionCatalog()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (items) => this.catalog.set(items)
      });

    effect(() => {
      this.loadRole(this.id());
    });
  }

  private loadRole(roleId: string): void {
    this.isLoading.set(true);
    this.loadError.set(null);
    this.submitError.set(null);

    this.rolesService.getRoleById(roleId).subscribe({
      next: (role) => {
        if (role.isSystem) {
          void this.router.navigate(['/roles', roleId], {
            queryParams: { systemRoleEditBlocked: '1' }
          });
          return;
        }

        this.recordVersion.set(role.version);
        this.roleModel.set({
          name: role.name,
          description: role.description ?? '',
          permissionCodes: role.permissions.map((permission) => permission.code)
        });
        this.isLoading.set(false);
      },
      error: (error: Error) => {
        this.loadError.set(error.message);
        this.isLoading.set(false);
      }
    });
  }

  refresh(): void {
    this.loadRole(this.id());
  }
}
