import {
  Component,
  computed,
  DestroyRef,
  inject,
  Injector
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { formatGridError } from '@query-grid/core';
import {
  QgColumnDirective,
  QgEmptyDirective,
  UiDataGridComponent
} from '@query-grid/ui';
import {
  ButtonComponent,
  MenuComponent,
  MessageBarComponent,
  TagComponent,
  type MenuItem
} from '@laczynski/ui';
import { ConfirmService } from '@core/confirm/services/confirm.service';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { RoleListItemDto } from '@features/roles/models/role.model';
import { RolesService } from '@features/roles/services/roles.service';
import {
  formatDescription,
  getDeleteRoleConfirmMessage,
  RoleMessages
} from '@features/roles/utils/roles.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import {
  createAppGridResource,
  getGridListEmptyMessage
} from '@shared/data/utils/grid.utils';

@Component({
  selector: 'app-roles-list',
  imports: [
    RouterLink,
    ButtonComponent,
    MessageBarComponent,
    TagComponent,
    MenuComponent,
    UiDataGridComponent,
    QgColumnDirective,
    QgEmptyDirective
  ],
  templateUrl: './roles-list.component.html'
})
export class RolesListComponent {
  private readonly rolesService = inject(RolesService);
  private readonly authService = inject(AuthService);
  private readonly confirmService = inject(ConfirmService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly injector = inject(Injector);

  readonly RoleMessages = RoleMessages;
  readonly formatDescription = formatDescription;
  readonly permissionCodes = PermissionCodes;
  readonly getGridListEmptyMessage = getGridListEmptyMessage;

  protected readonly rowType!: RoleListItemDto;

  readonly grid = createAppGridResource(this.injector, {
    load: (query) => this.rolesService.getRoles(query),
    defaultSort: [{ field: 'name', desc: false }],
    defaultTake: 10,
    persistKey: 'changeme.roles-list'
  });

  readonly errorMessage = computed(() => formatGridError(this.grid.error()));

  readonly canManageRoles = computed(() =>
    this.authService.hasPermission(PermissionCodes.rolesManage)
  );

  refresh(): void {
    this.grid.reload();
  }

  getRoleMenuItems(role: RoleListItemDto): MenuItem[] {
    const items: MenuItem[] = [
      { id: 'open', label: 'Open details', icon: 'eye' }
    ];

    if (this.canManageRoles() && !role.isSystem) {
      items.push(
        { id: 'edit', label: 'Edit role', icon: 'edit' },
        { id: 'delete', label: 'Delete role', icon: 'delete' }
      );
    }

    return items;
  }

  onRoleMenuAction(item: MenuItem, role: RoleListItemDto): void {
    switch (item.id) {
      case 'open':
        void this.router.navigate(['/roles', role.id]);
        break;
      case 'edit':
        void this.router.navigate(['/roles', role.id, 'edit']);
        break;
      case 'delete':
        this.confirmDeleteRole(role);
        break;
    }
  }

  confirmDeleteRole(role: RoleListItemDto): void {
    this.confirmService.confirm({
      header: 'Delete role',
      message: getDeleteRoleConfirmMessage(role.name),
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptVariant: 'danger',
      accept: () => {
        this.rolesService
          .deleteRole(role.id)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.toastService.success(RoleMessages.roleDeleted);
              this.refresh();
            },
            error: (error: Error) => {
              this.toastService.error(error.message);
            }
          });
      }
    });
  }
}
