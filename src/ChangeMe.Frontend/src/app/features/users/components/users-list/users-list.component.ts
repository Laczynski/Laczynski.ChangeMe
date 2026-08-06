import { DatePipe } from '@angular/common';
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
import { PermissionCodes } from '@shared/authorization/permission-codes';
import {
  createAppGridResource,
  getGridListEmptyMessage
} from '@shared/data/utils/grid.utils';

@Component({
  selector: 'app-users-list',
  imports: [
    DatePipe,
    RouterLink,
    ButtonComponent,
    MessageBarComponent,
    TagComponent,
    MenuComponent,
    UiDataGridComponent,
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
  private readonly confirmService = inject(ConfirmService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly injector = inject(Injector);

  readonly statusFilters = statusFilters;
  readonly getUserStatusLabel = getUserStatusLabel;
  readonly getUserStatusSeverity = getUserStatusSeverity;
  readonly UserMessages = UserMessages;
  readonly permissionCodes = PermissionCodes;
  readonly getGridListEmptyMessage = getGridListEmptyMessage;

  protected readonly rowType!: UserListItemDto;

  readonly grid = createAppGridResource(this.injector, {
    load: (query) => this.usersService.getUsers(query),
    defaultSort: [{ field: 'lastName', desc: false }],
    defaultTake: 10,
    persistKey: 'changeme.users-list'
  });

  readonly errorMessage = computed(() => formatGridError(this.grid.error()));

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

  refresh(): void {
    this.grid.reload();
  }

  getUserMenuItems(user: UserListItemDto): MenuItem[] {
    const items: MenuItem[] = [
      { id: 'open', label: 'Open details', icon: 'eye' }
    ];

    if (this.canManageUsers()) {
      items.push({ id: 'edit', label: 'Edit', icon: 'edit' });
    }

    if (this.canDeactivateUsers()) {
      items.push({
        id: user.deactivated ? 'activate' : 'deactivate',
        label: user.deactivated ? 'Activate' : 'Deactivate',
        icon: user.deactivated ? 'checkmark' : 'prohibited'
      });
    }

    return items;
  }

  onUserMenuAction(item: MenuItem, user: UserListItemDto): void {
    switch (item.id) {
      case 'open':
        void this.router.navigate(['/users', user.id]);
        break;
      case 'edit':
        void this.router.navigate(['/users', user.id, 'edit']);
        break;
      case 'deactivate':
        this.confirmDeactivate(user);
        break;
      case 'activate':
        this.confirmActivate(user);
        break;
    }
  }

  confirmDeactivate(user: UserListItemDto): void {
    this.confirmService.confirm({
      header: 'Deactivate user',
      message: getDeactivateConfirmMessage(formatUserReference(user)),
      acceptLabel: 'Deactivate',
      rejectLabel: 'Cancel',
      acceptVariant: 'danger',
      accept: () => this.deactivateUser(user)
    });
  }

  confirmActivate(user: UserListItemDto): void {
    this.confirmService.confirm({
      header: 'Activate user',
      message: getActivateConfirmMessage(formatUserReference(user)),
      acceptLabel: 'Activate',
      rejectLabel: 'Cancel',
      acceptVariant: 'success',
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
          this.refresh();
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
          this.refresh();
        },
        error: (error: Error) =>
          this.toastService.showApiError(error, 'Could not activate user')
      });
  }
}
