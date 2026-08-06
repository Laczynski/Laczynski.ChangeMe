import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import {
  AccordionComponent,
  ButtonComponent,
  MessageBarComponent,
  TextareaComponent,
  TextComponent
} from '@laczynski/ui';
import { ToastService } from '@core/toast/services/toast.service';
import { PermissionChecklistComponent } from '@features/roles/components/permission-checklist/permission-checklist.component';
import { PermissionCatalogItemDto } from '@features/roles/models/role.model';
import { RolesService } from '@features/roles/services/roles.service';
import { RoleConstraints, RoleFieldErrors, RoleMessages } from '@features/roles/utils/roles.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { DefaultExpandedAccordionDirective } from '@shared/directives/default-expanded-accordion.directive';
import { fieldError } from '@shared/forms/field-error';

@Component({
  selector: 'app-create-role',
  imports: [
    ReactiveFormsModule,
    BackButtonComponent,
    ButtonComponent,
    TextComponent,
    TextareaComponent,
    MessageBarComponent,
    AccordionComponent,
    DefaultExpandedAccordionDirective,
    PermissionChecklistComponent
  ],
  templateUrl: './create-role.component.html'
})
export class CreateRoleComponent {
  private readonly rolesService = inject(RolesService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly RoleMessages = RoleMessages;
  readonly catalog = signal<PermissionCatalogItemDto[]>([]);
  readonly submitError = signal<string | null>(null);
  readonly permissionsError = signal(false);
  readonly isSubmitting = signal(false);
  readonly submitted = signal(false);
  protected readonly fieldError = fieldError;
  protected readonly RoleFieldErrors = RoleFieldErrors;

  readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(RoleConstraints.NAME_MIN_LENGTH),
        Validators.maxLength(RoleConstraints.NAME_MAX_LENGTH)
      ]
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(RoleConstraints.DESCRIPTION_MAX_LENGTH)]
    }),
    permissionCodes: new FormControl<string[]>([], {
      nonNullable: true,
      validators: [Validators.required]
    })
  });

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

  onSubmit(): void {
    this.submitError.set(null);
    this.permissionsError.set(false);
    this.submitted.set(true);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      if (this.form.controls.permissionCodes.invalid) {
        this.permissionsError.set(true);
      }
      return;
    }

    this.isSubmitting.set(true);
    const raw = this.form.getRawValue();

    this.rolesService
      .createRole({
        name: raw.name,
        description: raw.description.trim() ? raw.description : null,
        permissionCodes: raw.permissionCodes
      })
      .subscribe({
        next: (role) => {
          this.toastService.success(RoleMessages.roleCreated);
          void this.router.navigate(['/roles', role.id]);
        },
        error: (error: Error) => {
          this.submitError.set(
            error.message === RoleMessages.duplicateName
              ? RoleMessages.duplicateName
              : error.message
          );
          this.isSubmitting.set(false);
        },
        complete: () => this.isSubmitting.set(false)
      });
  }
}
