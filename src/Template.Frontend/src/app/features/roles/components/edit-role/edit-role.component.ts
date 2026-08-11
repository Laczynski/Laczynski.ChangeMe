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
import { PermissionChecklistComponent } from '@features/roles/components/permission-checklist/permission-checklist.component';
import { PermissionCatalogItemDto } from '@features/roles/models/role.model';
import { RolesService } from '@features/roles/services/roles.service';
import { RoleConstraints, RoleMessages } from '@features/roles/utils/roles.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { ButtonDirective } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Textarea } from 'primeng/textarea';

@Component({
  selector: 'app-edit-role',
  imports: [
    ReactiveFormsModule,
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
  readonly pageTitle = computed(() => {
    const name = this.form.controls.name.value.trim();
    return name ? `Edit ${name}` : 'Edit Role';
  });

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
        this.form.patchValue({
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

  shouldShowError(control: {
    invalid: boolean;
    dirty: boolean;
    touched: boolean;
  }): boolean {
    return control.invalid && (control.dirty || control.touched);
  }

  onSubmit(): void {
    this.submitError.set(null);
    this.permissionsError.set(false);

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
      .updateRole({
        id: this.id(),
        version: this.recordVersion(),
        name: raw.name,
        description: raw.description.trim() ? raw.description : null,
        permissionCodes: raw.permissionCodes
      })
      .subscribe({
        next: (role) => {
          this.toastService.success(RoleMessages.roleSaved);
          void this.router.navigate(['/roles', role.id]);
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isSubmitting.set(false);
        },
        complete: () => this.isSubmitting.set(false)
      });
  }
}
