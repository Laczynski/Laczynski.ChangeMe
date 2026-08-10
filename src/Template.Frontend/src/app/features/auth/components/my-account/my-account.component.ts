import { DatePipe } from '@angular/common';
import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { MyAccountDto } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { EffectivePermissionsComponent } from '@features/users/components/effective-permissions/effective-permissions.component';
import { UserMessages } from '@features/users/utils/users.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucidePencil, lucideRefreshCw } from '@ng-icons/lucide';
import { HlmAlertImports } from '@spartan/ui/alert';
import { HlmBadgeImports } from '@spartan/ui/badge';
import { HlmButtonImports } from '@spartan/ui/button';
import { HlmCardImports } from '@spartan/ui/card';
import { HlmSpinnerImports } from '@spartan/ui/spinner';

@Component({
  selector: 'app-my-account',
  imports: [
    DatePipe,
    RouterLink,
    ...HlmAlertImports,
    ...HlmBadgeImports,
    ...HlmButtonImports,
    ...HlmCardImports,
    ...HlmSpinnerImports,
    NgIcon,
    EffectivePermissionsComponent
  ],
  providers: [provideIcons({ lucidePencil, lucideRefreshCw })],
  templateUrl: './my-account.component.html'
})
export class MyAccountComponent {
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  readonly account = signal<MyAccountDto | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly isLoading = signal(true);
  readonly UserMessages = UserMessages;

  readonly canViewRoles = computed(() =>
    this.authService.hasPermission(PermissionCodes.rolesView)
  );
  readonly effectivePermissions = computed(
    () => this.account()?.effectivePermissions ?? []
  );

  constructor() {
    this.reload();
  }

  reload(): void {
    const hasAccount = this.account() !== null;
    if (!hasAccount) {
      this.isLoading.set(true);
    }
    this.errorMessage.set(null);

    this.authService
      .getMyAccount()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (account) => {
          this.account.set(account);
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }
}
