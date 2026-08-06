import { AbstractControl } from '@angular/forms';

export type FieldErrorMessages = Partial<
  Record<string, string | ((error: unknown) => string)>
>;

export function fieldError(
  control: AbstractControl | null | undefined,
  messages: FieldErrorMessages,
  submitted = false
): string {
  if (!control?.errors || (!control.touched && !submitted)) {
    return '';
  }

  for (const key of Object.keys(control.errors)) {
    const message = messages[key];
    if (message) {
      return typeof message === 'function' ? message(control.errors[key]) : message;
    }
  }

  for (const value of Object.values(control.errors)) {
    if (typeof value === 'string') {
      return value;
    }

    if (value && typeof value === 'object' && 'message' in value) {
      return String((value as { message: string }).message);
    }
  }

  return '';
}
