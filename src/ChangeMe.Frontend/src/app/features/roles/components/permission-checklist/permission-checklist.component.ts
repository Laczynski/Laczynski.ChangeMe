import { Component, computed, input } from '@angular/core';
import { FormControl } from '@angular/forms';
import { CardComponent } from '@laczynski/ui';
import { PermissionCatalogItemDto } from '@features/roles/models/role.model';
import {
  RoleMessages,
  groupEffectivePermissions
} from '@features/roles/utils/roles.utils';

@Component({
  selector: 'app-permission-checklist',
  imports: [CardComponent],
  template: `
    <div class="flex flex-col gap-4">
      @if (showFormError()) {
        <span class="input-label input-label--error input-label--medium" role="status">
          {{ RoleMessages.atLeastOnePermission }}
        </span>
      }

      @for (group of groupedPermissions(); track group.group) {
        <section class="flex flex-col gap-3" [attr.aria-label]="group.group">
          <h4 class="m-0 text-sm font-semibold text-foreground-2">{{ group.group }}</h4>
          <div class="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
            @for (permission of group.items; track permission.code) {
              <ui-card
                appearance="filled-alternative"
                [checkbox]="true"
                [selected]="isChecked(permission.code)"
                [checkboxAriaLabel]="permission.label"
                [ariaLabel]="permission.label"
                (selectedChange)="togglePermission(permission.code, $event)"
              >
                <div uiCardHeader>{{ permission.label }}</div>
                <p uiCardBody>{{ permission.description }}</p>
              </ui-card>
            }
          </div>
        </section>
      }
    </div>
  `
})
export class PermissionChecklistComponent {
  readonly catalog = input<PermissionCatalogItemDto[]>([]);
  readonly control = input.required<FormControl<string[]>>();
  readonly showFormError = input(false);

  readonly RoleMessages = RoleMessages;

  readonly groupedPermissions = computed(() =>
    groupEffectivePermissions(this.catalog())
  );

  isChecked(code: string): boolean {
    return this.control().value.includes(code);
  }

  togglePermission(code: string, checked: boolean): void {
    const current = this.control().value;
    const next = checked
      ? [...current, code]
      : current.filter((value) => value !== code);
    this.control().setValue(next);
    this.control().markAsDirty();
  }
}
