import { Component, computed, input } from '@angular/core';
import {
  UserMessages,
  formatFromRoles,
  groupEffectivePermissions
} from '@features/users/utils/users.utils';
import { Panel } from 'primeng/panel';
export interface PermissionListItem {
  code: string;
  label: string;
  description: string;
  group: string;
  fromRoleNames?: string[];
}

@Component({
  selector: 'app-effective-permissions',
  imports: [Panel],
  templateUrl: './effective-permissions.component.html',
  host: { class: 'block' }
})
export class EffectivePermissionsComponent {
  readonly permissions = input<PermissionListItem[]>([]);
  readonly roleIdsSelected = input(true);
  readonly header = input('Permissions');
  readonly collapsed = input(true);
  readonly showFromRoles = input(true);

  readonly UserMessages = UserMessages;
  readonly formatFromRoles = formatFromRoles;

  readonly groupedPermissions = computed(() =>
    groupEffectivePermissions(this.permissions())
  );

  readonly emptySelectionMessage = computed(() =>
    this.roleIdsSelected() ? null : UserMessages.selectRoleToPreview
  );

  readonly panelHeader = computed(() => {
    const count = this.permissions().length;
    const title = this.header();
    return count > 0 ? `${title} (${count})` : title;
  });
}
