import { Component, computed, input } from '@angular/core';
import { Field } from '@angular/forms/signals';
import { Message } from 'primeng/message';

@Component({
  selector: 'app-form-field',
  host: { class: 'flex flex-col gap-1.5' },
  imports: [Message],
  template: `
    <ng-content />
    @if (showErrors()) {
      @for (error of errors(); track error.kind) {
        <p-message severity="error" variant="simple">{{ error.message }}</p-message>
      }
    }
  `
})
export class FormFieldComponent {
  readonly field = input.required<Field<unknown>>();

  private readonly fieldState = computed(() => this.field()());

  readonly showErrors = computed(() => {
    const state = this.fieldState();
    return state.touched() && state.invalid();
  });

  readonly errors = computed(() => this.fieldState().errors());
}
