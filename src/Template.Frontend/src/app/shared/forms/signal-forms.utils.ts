import type { FieldContext } from '@angular/forms/signals';

export function whenTouched({ state }: FieldContext<unknown>): boolean {
  return state.touched();
}
