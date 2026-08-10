import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ConfirmDialogService } from '@core/confirm/services/confirm-dialog.service';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { RoleListItemDto } from '@features/roles/models/role.model';
import { RolesService } from '@features/roles/services/roles.service';
import {
  formatDescription,
  getDeleteRoleConfirmMessage,
  RoleMessages
} from '@features/roles/utils/roles.utils';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideEllipsisVertical,
  lucideEye,
  lucidePencil,
  lucidePlus,
  lucideRefreshCw,
  lucideTrash
} from '@ng-icons/lucide';
import {
  GridResourceFactory,
  DgColumnDirective,
  DgEmptyDirective,
  SpartanDataGridComponent,
  type GridResource
} from '@laczynski/datagrid-spartan';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { getGridListEmptyMessage } from '@shared/data/utils/grid.utils';
import type { ListRowMenuItem } from '@shared/ui/utils/list-row-menu.utils';
import { HlmAlertImports } from '@spartan/ui/alert';
import { HlmBadgeImports } from '@spartan/ui/badge';
import { HlmButtonImports } from '@spartan/ui/button';
import { HlmCardImports } from '@spartan/ui/card';
import { HlmDropdownMenuImports } from '@spartan/ui/dropdown-menu';
import { HlmTooltipImports } from '@spartan/ui/tooltip';

@Component({
  selector: 'app-roles-list',
  imports: [
    RouterLink,
    NgIcon,
    ...HlmCardImports,
    ...HlmButtonImports,
    ...HlmAlertImports,
    ...HlmBadgeImports,
    ...HlmDropdownMenuImports,
    ...HlmTooltipImports,
    SpartanDataGridComponent,
    DgColumnDirective,
    DgEmptyDirective
  ],
  providers: [
    provideIcons({
      lucideEllipsisVertical,
      lucideEye,
      lucidePencil,
      lucidePlus,
      lucideRefreshCw,
      lucideTrash
    })
  ],
  templateUrl: './roles-list.component.html'
})
export class RolesListComponent {
  private readonly rolesService = inject(RolesService);
  private readonly authService = inject(AuthService);
  private readonly confirmDialogService = inject(ConfirmDialogService);
  private readonly toastService = inject(ToastService);
  private readonly gridFactory = inject(GridResourceFactory);
  private readonly destroyRef = inject(DestroyRef);

  readonly RoleMessages = RoleMessages;
  readonly formatDescription = formatDescription;
  readonly permissionCodes = PermissionCodes;

  readonly roleActionItems = signal<ListRowMenuItem[]>([]);

  readonly grid: GridResource<RoleListItemDto>;

  readonly canManageRoles = computed(() =>
    this.authService.hasPermission(PermissionCodes.rolesManage)
  );

  readonly errorMessage = computed(() => {
    const error = this.grid.error();
    return error instanceof Error ? error.message : error ? String(error) : null;
  });

  readonly emptyMessage = computed(() => getGridListEmptyMessage(this.grid.query()));

  constructor() {
    this.grid = this.gridFactory.create<RoleListItemDto>({
      destroyRef: this.destroyRef,
      load: (query) => this.rolesService.getRoles(query),
      defaultSort: [{ field: 'Name', desc: false }],
      defaultTake: 10,
      persistState: { key: 'changeMe.roles-list', storage: 'session' }
    });
  }

  refresh(): void {
    this.grid.reload();
  }

  setRoleActionItems(role: RoleListItemDto): void {
    const items: ListRowMenuItem[] = [
      {
        label: 'Open details',
        icon: 'lucideEye',
        routerLink: ['/roles', role.id]
      }
    ];

    if (this.canManageRoles() && !role.isSystem) {
      items.push(
        {
          label: 'Edit role',
          icon: 'lucidePencil',
          routerLink: ['/roles', role.id, 'edit']
        },
        {
          label: 'Delete role',
          icon: 'lucideTrash',
          variant: 'destructive',
          command: () => this.confirmDeleteRole(role)
        }
      );
    }

    this.roleActionItems.set(items);
  }

  confirmDeleteRole(role: RoleListItemDto): void {
    this.confirmDialogService.confirm({
      header: 'Delete role',
      message: getDeleteRoleConfirmMessage(role.name),
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptVariant: 'destructive',
      accept: () => {
        this.rolesService
          .deleteRole(role.id)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.toastService.success(RoleMessages.roleDeleted);
              this.grid.reload();
            },
            error: (error: Error) => {
              this.toastService.error(error.message);
            }
          });
      }
    });
  }
}
