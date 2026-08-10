import { Component, computed, input } from '@angular/core';
import { FormControl } from '@angular/forms';
import { PermissionCatalogItemDto } from '@features/roles/models/role.model';
import {
  RoleMessages,
  groupEffectivePermissions
} from '@features/roles/utils/roles.utils';
import { HlmAlertImports } from '@spartan/ui/alert';
import { HlmCheckboxImports } from '@spartan/ui/checkbox';
import { HlmLabelImports } from '@spartan/ui/label';

@Component({
  selector: 'app-permission-checklist',
  imports: [...HlmCheckboxImports, ...HlmLabelImports, ...HlmAlertImports],
  templateUrl: './permission-checklist.component.html'
})
export class PermissionChecklistComponent {
  readonly catalog = input<PermissionCatalogItemDto[]>([]);
  readonly control = input.required<FormControl<string[]>>();
  readonly showFormError = input(false);

  readonly RoleMessages = RoleMessages;

  readonly groupedPermissions = computed(() =>
    groupEffectivePermissions(this.catalog())
  );

  isPermissionSelected(code: string): boolean {
    return this.control().value.includes(code);
  }

  togglePermission(code: string, checked: boolean): void {
    const control = this.control();
    const current = control.value;

    if (checked) {
      control.setValue([...current, code]);
    } else {
      control.setValue(current.filter((item) => item !== code));
    }

    control.markAsTouched();
  }
}
