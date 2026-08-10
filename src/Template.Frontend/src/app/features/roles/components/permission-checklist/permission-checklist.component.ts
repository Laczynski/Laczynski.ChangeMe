import { Component, computed, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { PermissionCatalogItemDto } from '@features/roles/models/role.model';
import {
  RoleMessages,
  groupEffectivePermissions
} from '@features/roles/utils/roles.utils';
import { Checkbox } from 'primeng/checkbox';
import { Message } from 'primeng/message';

@Component({
  selector: 'app-permission-checklist',
  imports: [ReactiveFormsModule, Checkbox, Message],
  template: `
    <div class="flex flex-col gap-4">
      @if (showFormError()) {
        <p-message severity="error">{{ RoleMessages.atLeastOnePermission }}</p-message>
      }

      @for (group of groupedPermissions(); track group.group) {
        <div class="flex flex-col gap-2">
          <h4
            class="text-muted-color m-0 text-sm font-semibold tracking-wide uppercase"
          >
            {{ group.group }}
          </h4>
          <div class="flex flex-col gap-3">
            @for (permission of group.items; track permission.code) {
              <div
                class="border-surface-200 flex cursor-pointer gap-3 rounded-lg border p-3 dark:border-surface-700"
              >
                <p-checkbox
                  [inputId]="permission.code"
                  [formControl]="control()"
                  [value]="permission.code"
                />
                <label
                  [attr.for]="permission.code"
                  class="flex flex-1 cursor-pointer flex-col gap-1"
                >
                  <span class="text-color font-medium">{{ permission.label }}</span>
                  <span class="text-muted-color text-sm">
                    {{ permission.description }}
                  </span>
                </label>
              </div>
            }
          </div>
        </div>
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
}
