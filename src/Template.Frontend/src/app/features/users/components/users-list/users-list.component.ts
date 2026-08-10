import { DatePipe } from '@angular/common';
import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ConfirmDialogService } from '@core/confirm/services/confirm-dialog.service';
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
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideBan,
  lucideCheck,
  lucideEllipsisVertical,
  lucideEye,
  lucidePencil,
  lucideRefreshCw,
  lucideUserPlus
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
import { mapBadgeSeverity } from '@shared/ui/utils/badge.utils';
import type { ListRowMenuItem } from '@shared/ui/utils/list-row-menu.utils';
import { HlmAlertImports } from '@spartan/ui/alert';
import { HlmBadgeImports } from '@spartan/ui/badge';
import { HlmButtonImports } from '@spartan/ui/button';
import { HlmCardImports } from '@spartan/ui/card';
import { HlmDropdownMenuImports } from '@spartan/ui/dropdown-menu';
import { HlmTooltipImports } from '@spartan/ui/tooltip';

@Component({
  selector: 'app-users-list',
  imports: [
    DatePipe,
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
      lucideBan,
      lucideCheck,
      lucideEllipsisVertical,
      lucideEye,
      lucidePencil,
      lucideRefreshCw,
      lucideUserPlus
    })
  ],
  templateUrl: './users-list.component.html'
})
export class UsersListComponent {
  readonly formatUserName = formatUserName;
  readonly formatUserReference = formatUserReference;
  readonly mapBadgeSeverity = mapBadgeSeverity;

  private readonly usersService = inject(UsersService);
  private readonly authService = inject(AuthService);
  private readonly confirmDialogService = inject(ConfirmDialogService);
  private readonly toastService = inject(ToastService);
  private readonly gridFactory = inject(GridResourceFactory);
  private readonly destroyRef = inject(DestroyRef);

  readonly statusFilters = statusFilters;
  readonly getUserStatusLabel = getUserStatusLabel;
  readonly getUserStatusSeverity = getUserStatusSeverity;
  readonly UserMessages = UserMessages;
  readonly permissionCodes = PermissionCodes;

  readonly userActionItems = signal<ListRowMenuItem[]>([]);

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
      persistState: { key: 'changeMe.users-list', storage: 'session' }
    });
  }

  refresh(): void {
    this.grid.reload();
  }

  setUserActionItems(user: UserListItemDto): void {
    const items: ListRowMenuItem[] = [
      {
        label: 'Open details',
        icon: 'lucideEye',
        routerLink: ['/users', user.id]
      }
    ];

    if (this.canManageUsers()) {
      items.push({
        label: 'Edit',
        icon: 'lucidePencil',
        routerLink: ['/users', user.id, 'edit']
      });
    }

    if (this.canDeactivateUsers()) {
      if (user.deactivated) {
        items.push({
          label: 'Activate',
          icon: 'lucideCheck',
          command: () => this.confirmActivate(user)
        });
      } else {
        items.push({
          label: 'Deactivate',
          icon: 'lucideBan',
          command: () => this.confirmDeactivate(user)
        });
      }
    }

    this.userActionItems.set(items);
  }

  confirmDeactivate(user: UserListItemDto): void {
    this.confirmDialogService.confirm({
      header: 'Deactivate user',
      message: getDeactivateConfirmMessage(formatUserReference(user)),
      acceptLabel: 'Deactivate',
      rejectLabel: 'Cancel',
      acceptVariant: 'destructive',
      accept: () => this.deactivateUser(user)
    });
  }

  confirmActivate(user: UserListItemDto): void {
    this.confirmDialogService.confirm({
      header: 'Activate user',
      message: getActivateConfirmMessage(formatUserReference(user)),
      acceptLabel: 'Activate',
      rejectLabel: 'Cancel',
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
