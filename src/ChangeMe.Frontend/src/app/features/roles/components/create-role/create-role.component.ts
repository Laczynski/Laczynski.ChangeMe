import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  form,
  FormField,
  FormRoot,
  maxLength,
  minLength,
  required
} from '@angular/forms/signals';
import { Router } from '@angular/router';
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
import { Textarea } from 'primeng/textarea';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-create-role',
  imports: [
    FormField,
    FormFieldComponent,
    FormRoot,
    BackButtonComponent,
    Card,
    ButtonDirective,
    InputText,
    Textarea,
    Message,
    Panel,
    PermissionChecklistComponent
  ],
  templateUrl: './create-role.component.html'
})
export class CreateRoleComponent {
  private readonly rolesService = inject(RolesService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly roleConstraints = RoleConstraints;
  readonly RoleMessages = RoleMessages;
  readonly catalog = signal<PermissionCatalogItemDto[]>([]);
  readonly submitError = signal<string | null>(null);
  readonly permissionsError = signal(false);
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
              this.rolesService.createRole({
                name: raw.name,
                description: raw.description.trim() ? raw.description : null,
                permissionCodes: raw.permissionCodes
              })
            );
            this.toastService.success(RoleMessages.roleCreated);
            void this.router.navigate(['/roles', role.id]);
          } catch (error) {
            const message = error instanceof Error ? error.message : 'Create failed.';
            this.submitError.set(
              message === RoleMessages.duplicateName
                ? RoleMessages.duplicateName
                : message
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

  constructor() {
    this.rolesService
      .getPermissionCatalog()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (items) => this.catalog.set(items),
        error: (error: Error) => this.submitError.set(error.message)
      });
  }

  cancel(): void {
    void this.router.navigate(['/roles']);
  }
}
