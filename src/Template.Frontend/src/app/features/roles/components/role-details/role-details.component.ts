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
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ConfirmDialogService } from '@core/confirm/services/confirm-dialog.service';
import { ToastService } from '@core/toast/services/toast.service';
import {
  formatUserName,
  formatUserReference
} from '@core/user/utils/user-display.utils';
import { AuthService } from '@features/auth/services/auth.service';
import { RoleAssignedUserDto, RoleDetailsDto } from '@features/roles/models/role.model';
import { RolesService } from '@features/roles/services/roles.service';
import {
  formatDescription,
  getDeleteRoleConfirmMessage,
  getRemoveUserFromRoleConfirmMessage,
  RoleMessages
} from '@features/roles/utils/roles.utils';
import { EffectivePermissionsComponent } from '@features/users/components/effective-permissions/effective-permissions.component';
import {
  getUserStatusLabel,
  getUserStatusSeverity,
  toUserMembershipStatus
} from '@features/users/utils/users.utils';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideLoader2,
  lucidePencil,
  lucideRefreshCw,
  lucideTrash2,
  lucideUserMinus
} from '@ng-icons/lucide';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { GridPaginatorComponent } from '@shared/components/grid-paginator/grid-paginator.component';
import {
  createGridQuery,
  DEFAULT_GRID_PAGE_SIZE,
  type GridPageChangeEvent
} from '@shared/data/utils/grid.utils';
import { mapBadgeSeverity } from '@shared/ui/utils/badge.utils';
import { HlmAlertImports } from '@spartan/ui/alert';
import { HlmBadgeImports } from '@spartan/ui/badge';
import { HlmButtonImports } from '@spartan/ui/button';
import { HlmCardImports } from '@spartan/ui/card';
import { HlmInputImports } from '@spartan/ui/input';
import { HlmLabelImports } from '@spartan/ui/label';
import { HlmSpinnerImports } from '@spartan/ui/spinner';
import { debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-role-details',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    BackButtonComponent,
    GridPaginatorComponent,
    ...HlmCardImports,
    ...HlmButtonImports,
    ...HlmInputImports,
    ...HlmLabelImports,
    ...HlmAlertImports,
    ...HlmBadgeImports,
    ...HlmSpinnerImports,
    NgIcon,
    EffectivePermissionsComponent
  ],
  providers: [
    provideIcons({
      lucideLoader2,
      lucidePencil,
      lucideRefreshCw,
      lucideTrash2,
      lucideUserMinus
    })
  ],
  templateUrl: './role-details.component.html'
})
export class RoleDetailsComponent {
  readonly formatUserName = formatUserName;

  readonly id = input.required<string>();

  private readonly rolesService = inject(RolesService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly confirmDialogService = inject(ConfirmDialogService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly RoleMessages = RoleMessages;
  readonly formatDescription = formatDescription;
  readonly getUserStatusLabel = getUserStatusLabel;
  readonly getUserStatusSeverity = getUserStatusSeverity;
  readonly toUserMembershipStatus = toUserMembershipStatus;
  readonly mapBadgeSeverity = mapBadgeSeverity;
  readonly DEFAULT_GRID_PAGE_SIZE = DEFAULT_GRID_PAGE_SIZE;

  readonly role = signal<RoleDetailsDto | null>(null);
  readonly pageTitle = computed(() => this.role()?.name ?? 'Role details');
  readonly assignedUsers = signal<RoleAssignedUserDto[]>([]);
  readonly assignedUsersGrid = signal(
    createGridQuery({ sort: [{ field: 'LastName', desc: false }] })
  );
  readonly assignedUsersTotalCount = signal(0);
  readonly isLoading = signal(true);
  readonly isLoadingUsers = signal(true);
  readonly hasLoadedUsers = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showSystemRoleEditBlockedMessage = signal(false);
  readonly usersSearchControl = new FormControl('', { nonNullable: true });

  readonly canManageRoles = computed(() =>
    this.authService.hasPermission(PermissionCodes.rolesManage)
  );

  constructor() {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const blocked = params.get('systemRoleEditBlocked') === '1';
        this.showSystemRoleEditBlockedMessage.set(blocked);

        if (blocked) {
          void this.router.navigate([], {
            relativeTo: this.route,
            queryParams: { systemRoleEditBlocked: null },
            queryParamsHandling: 'merge',
            replaceUrl: true
          });
        }
      });

    effect(() => {
      this.id();
      this.loadRole();
    });

    this.usersSearchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        const search = this.usersSearchControl.value.trim() || undefined;
        this.assignedUsersGrid.set(
          createGridQuery({
            skip: 0,
            sort: [{ field: 'LastName', desc: false }],
            search
          })
        );
        this.loadAssignedUsers();
      });
  }

  refresh(): void {
    this.loadRole();
  }

  onAssignedUsersPageChange(event: GridPageChangeEvent): void {
    const { take, skip } = event;
    this.assignedUsersGrid.update((current) =>
      createGridQuery({
        skip,
        take,
        sort: current.sort ?? [{ field: 'LastName', desc: false }],
        search: current.search ?? undefined
      })
    );
    this.loadAssignedUsers();
  }

  private loadRole(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.rolesService.getRoleById(this.id()).subscribe({
      next: (details) => {
        this.role.set(details);
        this.isLoading.set(false);
        this.loadAssignedUsers();
      },
      error: (error: Error) => {
        this.errorMessage.set(error.message);
        this.isLoading.set(false);
      }
    });
  }

  private loadAssignedUsers(): void {
    this.isLoadingUsers.set(true);

    this.rolesService
      .getRoleAssignedUsers(this.id(), this.assignedUsersGrid())
      .subscribe({
        next: (result) => {
          this.assignedUsers.set(result.items);
          this.assignedUsersTotalCount.set(result.totalCount);
          this.isLoadingUsers.set(false);
          this.hasLoadedUsers.set(true);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoadingUsers.set(false);
        }
      });
  }

  confirmDeleteRole(): void {
    const current = this.role();
    if (!current) {
      return;
    }

    this.confirmDialogService.confirm({
      header: 'Delete role',
      message: getDeleteRoleConfirmMessage(current.name),
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptVariant: 'destructive',
      accept: () => {
        this.rolesService.deleteRole(current.id).subscribe({
          next: () => {
            this.toastService.success(RoleMessages.roleDeleted);
            void this.router.navigateByUrl('/roles');
          },
          error: (error: Error) => this.toastService.error(error.message)
        });
      }
    });
  }

  confirmRemoveUser(user: RoleAssignedUserDto): void {
    const current = this.role();
    if (!current) {
      return;
    }

    this.confirmDialogService.confirm({
      header: 'Remove from role',
      message: getRemoveUserFromRoleConfirmMessage(
        formatUserReference(user),
        current.name
      ),
      acceptLabel: 'Remove',
      rejectLabel: 'Cancel',
      acceptVariant: 'destructive',
      accept: () => {
        this.rolesService.removeUserFromRole(current.id, user.id).subscribe({
          next: () => {
            this.toastService.success(RoleMessages.userRemovedFromRole);
            this.loadRole();
          },
          error: (error: Error) => this.toastService.error(error.message)
        });
      }
    });
  }
}
