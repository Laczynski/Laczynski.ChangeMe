import { DatePipe } from '@angular/common';
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
import { RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import {
  formatUserName,
  formatUserReference
} from '@core/user/utils/user-display.utils';
import { AuthService } from '@features/auth/services/auth.service';
import { formatIpAddress } from '@features/auth/utils/auth.utils';
import { EffectivePermissionsComponent } from '@features/users/components/effective-permissions/effective-permissions.component';
import { AdminUserSessionDto, UserDetailsDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import {
  getActivateConfirmMessage,
  getDeactivateConfirmMessage,
  getUserStatusLabel,
  getUserStatusSeverity,
  UserMessages
} from '@features/users/utils/users.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { createGridQuery, DEFAULT_GRID_PAGE_SIZE } from '@shared/data/utils/grid.utils';
import { ConfirmationService } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';

@Component({
  selector: 'app-user-details',
  imports: [
    DatePipe,
    RouterLink,
    BackButtonComponent,
    Card,
    ButtonDirective,
    Message,
    Tag,
    TableModule,
    Paginator,
    Panel,
    ProgressSpinner,
    EffectivePermissionsComponent
  ],
  templateUrl: './user-details.component.html'
})
export class UserDetailsComponent {
  readonly id = input.required<string>();

  private readonly usersService = inject(UsersService);
  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly user = signal<UserDetailsDto | null>(null);
  readonly pageTitle = computed(() => {
    const profile = this.user();
    return profile ? formatUserReference(profile) : 'User details';
  });

  readonly formatUserName = formatUserName;
  readonly sessions = signal<AdminUserSessionDto[]>([]);
  readonly sessionsGrid = signal(
    createGridQuery({ sort: [{ field: 'LastActivityAt', desc: true }] })
  );
  readonly sessionsTotalCount = signal(0);
  readonly errorMessage = signal<string | null>(null);
  readonly isLoading = signal(true);
  readonly isLoadingSessions = signal(false);
  readonly hasLoadedSessions = signal(false);
  readonly pendingRevokeSessionIds = signal<string[]>([]);

  readonly UserMessages = UserMessages;
  readonly formatIpAddress = formatIpAddress;
  readonly DEFAULT_GRID_PAGE_SIZE = DEFAULT_GRID_PAGE_SIZE;
  readonly getUserStatusLabel = getUserStatusLabel;
  readonly getUserStatusSeverity = getUserStatusSeverity;
  readonly canManageUsers = () =>
    this.authService.hasPermission(PermissionCodes.usersManage);
  readonly canDeactivateUsers = () =>
    this.authService.hasPermission(PermissionCodes.usersDeactivate);
  readonly canViewSessions = () =>
    this.authService.hasPermission(PermissionCodes.sessionsViewAny);
  readonly canManageSessions = () =>
    this.authService.hasPermission(PermissionCodes.sessionsManageAny);

  constructor() {
    effect(() => {
      this.id();
      this.loadUser();
    });
  }

  confirmDeactivate(): void {
    const profile = this.user();
    if (!profile) {
      return;
    }

    this.confirmationService.confirm({
      header: 'Deactivate user',
      message: getDeactivateConfirmMessage(formatUserReference(profile)),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Deactivate', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.deactivateUser()
    });
  }

  confirmActivate(): void {
    const profile = this.user();
    if (!profile) {
      return;
    }

    this.confirmationService.confirm({
      header: 'Activate user',
      message: getActivateConfirmMessage(formatUserReference(profile)),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Activate', severity: 'success' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.activateUser()
    });
  }

  confirmRevokeAllSessions(): void {
    this.confirmationService.confirm({
      header: UserMessages.revokeAllSessionsTitle,
      message: UserMessages.revokeAllSessionsMessage,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Revoke all', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.revokeAllSessions()
    });
  }

  confirmRevokeSession(session: AdminUserSessionDto): void {
    this.confirmationService.confirm({
      header: UserMessages.revokeSessionTitle,
      message: UserMessages.revokeSessionMessage,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Revoke', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.revokeSession(session)
    });
  }

  isRevokePending(sessionId: string): boolean {
    return this.pendingRevokeSessionIds().includes(sessionId);
  }

  refresh(): void {
    this.loadUser();
  }

  private loadUser(): void {
    const userId = this.id();
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.usersService
      .getUserById(userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.isLoading.set(false);

          if (this.canViewSessions() && !user.deactivated) {
            this.loadSessions(userId);
          } else {
            this.sessions.set([]);
          }
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }

  onSessionsPageChange(event: PaginatorState): void {
    const take = event.rows ?? DEFAULT_GRID_PAGE_SIZE;
    const skip = event.first ?? 0;
    this.sessionsGrid.set(
      createGridQuery({
        skip,
        take,
        sort: [{ field: 'LastActivityAt', desc: true }]
      })
    );
    this.loadSessions(this.id());
  }

  private loadSessions(userId: string): void {
    this.isLoadingSessions.set(true);

    this.usersService
      .getUserSessions(userId, this.sessionsGrid())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.sessions.set(result.items);
          this.sessionsTotalCount.set(result.totalCount);
          this.isLoadingSessions.set(false);
          this.hasLoadedSessions.set(true);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoadingSessions.set(false);
        }
      });
  }

  private deactivateUser(): void {
    this.usersService
      .deactivateUser(this.id())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.sessions.set([]);
          this.toastService.success(UserMessages.userDeactivated);
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private activateUser(): void {
    this.usersService
      .activateUser(this.id())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.toastService.success(UserMessages.userActivated);
          if (this.canViewSessions()) {
            this.loadSessions(user.id);
          }
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private revokeAllSessions(): void {
    this.usersService
      .revokeAllUserSessions(this.id())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.sessions.set([]);
          this.toastService.success('All sessions revoked.');
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private revokeSession(session: AdminUserSessionDto): void {
    this.pendingRevokeSessionIds.update((ids) => [...ids, session.id]);

    this.usersService
      .revokeUserSession(this.id(), session.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.sessions.update((items) =>
            items.filter((item) => item.id !== session.id)
          );
          this.pendingRevokeSessionIds.update((ids) =>
            ids.filter((id) => id !== session.id)
          );
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.pendingRevokeSessionIds.update((ids) =>
            ids.filter((id) => id !== session.id)
          );
        }
      });
  }
}
