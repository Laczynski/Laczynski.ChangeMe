import { DatePipe } from '@angular/common';
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
import {
  formatUserName,
  formatUserReference
} from '@core/user/utils/user-display.utils';
import { AuthService } from '@features/auth/services/auth.service';
import { UserListItemDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import {
  getActivateConfirmMessage,
  getDeactivateConfirmMessage,
  getUserStatusLabel,
  getUserStatusSeverity,
  statusFilters,
  UserMessages
} from '@features/users/utils/users.utils';
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
  selector: 'app-users-list',
  imports: [
    DatePipe,
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
  templateUrl: './users-list.component.html'
})
export class UsersListComponent {
  readonly formatUserName = formatUserName;
  readonly formatUserReference = formatUserReference;

  private readonly usersService = inject(UsersService);
  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly gridFactory = inject(GridResourceFactory);
  private readonly destroyRef = inject(DestroyRef);

  readonly statusFilters = statusFilters;
  readonly getUserStatusLabel = getUserStatusLabel;
  readonly getUserStatusSeverity = getUserStatusSeverity;
  readonly UserMessages = UserMessages;
  readonly permissionCodes = PermissionCodes;

  readonly userActionItems = signal<MenuItem[]>([]);
  private readonly userActionsMenu = viewChild.required<Menu>('userActionsMenu');

  readonly grid: GridResource<UserListItemDto>;

  readonly canManageUsers = computed(() =>
    this.authService.hasPermission(PermissionCodes.usersManage)
  );
  readonly canCreateUsers = computed(
    () =>
      this.authService.hasPermission(PermissionCodes.usersManage) &&
      this.authService.hasPermission(PermissionCodes.rolesManage)
  );
  readonly canDeactivateUsers = computed(() =>
    this.authService.hasPermission(PermissionCodes.usersDeactivate)
  );

  readonly errorMessage = computed(() => {
    const error = this.grid.error();
    return error instanceof Error ? error.message : error ? String(error) : null;
  });

  readonly emptyMessage = computed(() => getGridListEmptyMessage(this.grid.query()));

  constructor() {
    this.grid = this.gridFactory.create<UserListItemDto>({
      destroyRef: this.destroyRef,
      load: (query) => this.usersService.getUsers(query),
      defaultSort: [{ field: 'LastName', desc: false }],
      defaultTake: 10,
      persistState: { key: 'changeme.users-list', storage: 'session' }
    });
  }

  refresh(): void {
    this.grid.reload();
  }

  openUserActionsMenu(event: Event, user: UserListItemDto): void {
    const items: MenuItem[] = [
      {
        label: 'Open details',
        icon: 'pi pi-eye',
        routerLink: ['/users', user.id]
      }
    ];

    if (this.canManageUsers()) {
      items.push({
        label: 'Edit',
        icon: 'pi pi-pencil',
        routerLink: ['/users', user.id, 'edit']
      });
    }

    if (this.canDeactivateUsers()) {
      if (user.deactivated) {
        items.push({
          label: 'Activate',
          icon: 'pi pi-check',
          command: () => this.confirmActivate(user)
        });
      } else {
        items.push({
          label: 'Deactivate',
          icon: 'pi pi-ban',
          command: () => this.confirmDeactivate(user)
        });
      }
    }

    this.userActionItems.set(items);
    this.userActionsMenu().toggle(event);
  }

  confirmDeactivate(user: UserListItemDto): void {
    this.confirmationService.confirm({
      header: 'Deactivate user',
      message: getDeactivateConfirmMessage(formatUserReference(user)),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Deactivate', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.deactivateUser(user)
    });
  }

  confirmActivate(user: UserListItemDto): void {
    this.confirmationService.confirm({
      header: 'Activate user',
      message: getActivateConfirmMessage(formatUserReference(user)),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Activate', severity: 'success' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.activateUser(user)
    });
  }

  private deactivateUser(user: UserListItemDto): void {
    this.usersService
      .deactivateUser(user.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(UserMessages.userDeactivated);
          this.grid.reload();
        },
        error: (error: Error) =>
          this.toastService.showApiError(error, 'Could not deactivate user')
      });
  }

  private activateUser(user: UserListItemDto): void {
    this.usersService
      .activateUser(user.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(UserMessages.userActivated);
          this.grid.reload();
        },
        error: (error: Error) =>
          this.toastService.showApiError(error, 'Could not activate user')
      });
  }
}
