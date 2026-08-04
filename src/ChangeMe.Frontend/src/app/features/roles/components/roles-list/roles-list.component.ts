import {
  Component,
  computed,
  DestroyRef,
  inject,
  signal,
  viewChild
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { RoleListItemDto } from '@features/roles/models/role.model';
import { RolesService } from '@features/roles/services/roles.service';
import {
  formatDescription,
  getDeleteRoleConfirmMessage,
  RoleMessages
} from '@features/roles/utils/roles.utils';
import {
  GridResourceFactory,
  PrimeDataGridComponent,
  QgColumnDirective,
  QgEmptyDirective,
  type GridResource
} from '@query-grid/primeng';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { getGridListEmptyMessage } from '@shared/data/utils/grid.utils';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { Card } from 'primeng/card';
import { Menu } from 'primeng/menu';
import { Message } from 'primeng/message';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';

@Component({
  selector: 'app-roles-list',
  imports: [
    RouterLink,
    Card,
    ButtonDirective,
    Message,
    Tag,
    Menu,
    Tooltip,
    PrimeDataGridComponent,
    QgColumnDirective,
    QgEmptyDirective
  ],
  templateUrl: './roles-list.component.html'
})
export class RolesListComponent {
  private readonly rolesService = inject(RolesService);
  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly gridFactory = inject(GridResourceFactory);
  private readonly destroyRef = inject(DestroyRef);

  readonly RoleMessages = RoleMessages;
  readonly formatDescription = formatDescription;
  readonly permissionCodes = PermissionCodes;

  readonly roleActionItems = signal<MenuItem[]>([]);
  private readonly roleActionsMenu = viewChild.required<Menu>('roleActionsMenu');

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
      persistState: { key: 'changeme.roles-list', storage: 'session' }
    });
  }

  refresh(): void {
    this.grid.reload();
  }

  openRoleActionsMenu(event: Event, role: RoleListItemDto): void {
    const items: MenuItem[] = [
      {
        label: 'Open details',
        icon: 'pi pi-eye',
        routerLink: ['/roles', role.id]
      }
    ];

    if (this.canManageRoles() && !role.isSystem) {
      items.push(
        {
          label: 'Edit role',
          icon: 'pi pi-pencil',
          routerLink: ['/roles', role.id, 'edit']
        },
        {
          label: 'Delete role',
          icon: 'pi pi-trash',
          command: () => this.confirmDeleteRole(role)
        }
      );
    }

    this.roleActionItems.set(items);
    this.roleActionsMenu().toggle(event);
  }

  confirmDeleteRole(role: RoleListItemDto): void {
    this.confirmationService.confirm({
      header: 'Delete role',
      message: getDeleteRoleConfirmMessage(role.name),
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
