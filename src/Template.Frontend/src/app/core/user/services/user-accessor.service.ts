import { Injectable, computed, inject } from '@angular/core';
import { AuthService } from '@features/auth/services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class UserAccessor {
  private readonly authService = inject(AuthService);

  readonly $currentUser = computed(() => this.authService.currentUser());
  readonly isAuthenticated = computed(() => this.authService.isAuthenticated());
  readonly getCurrentUser = computed(() => this.authService.currentUser());
}
