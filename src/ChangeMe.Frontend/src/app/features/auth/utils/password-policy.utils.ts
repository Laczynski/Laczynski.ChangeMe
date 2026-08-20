import {
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import {
  maxLength,
  minLength,
  required,
  validate,
  type SchemaPath
} from '@angular/forms/signals';
import { AuthConstraints } from '@features/auth/utils/auth.utils';
import { whenTouched } from '@shared/forms/signal-forms.utils';

export interface PasswordPolicySettings {
  minimumLength: number;
  maximumLength: number;
  requireUppercase: boolean;
  requireLowercase: boolean;
  requireDigit: boolean;
  requireSpecialCharacter: boolean;
}

export function defaultPasswordPolicySettings(): PasswordPolicySettings {
  return {
    minimumLength: AuthConstraints.PASSWORD_MIN_LENGTH,
    maximumLength: AuthConstraints.PASSWORD_MAX_LENGTH,
    requireUppercase: true,
    requireLowercase: true,
    requireDigit: true,
    requireSpecialCharacter: false
  };
}

export function buildPasswordPolicyValidators(
  policy: PasswordPolicySettings
): ValidatorFn[] {
  return [
    Validators.required,
    Validators.minLength(policy.minimumLength),
    Validators.maxLength(policy.maximumLength),
    passwordPolicyValidator(policy)
  ];
}

export function passwordPolicyValidator(policy: PasswordPolicySettings): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const password = control.value;
    if (typeof password !== 'string' || password.length === 0) {
      return null;
    }

    if (policy.requireUppercase && !/[A-Z]/.test(password)) {
      return { passwordPolicy: 'Password must contain at least one uppercase letter.' };
    }

    if (policy.requireLowercase && !/[a-z]/.test(password)) {
      return { passwordPolicy: 'Password must contain at least one lowercase letter.' };
    }

    if (policy.requireDigit && !/\d/.test(password)) {
      return { passwordPolicy: 'Password must contain at least one digit.' };
    }

    if (policy.requireSpecialCharacter && !/[^A-Za-z0-9]/.test(password)) {
      return {
        passwordPolicy: 'Password must contain at least one special character.'
      };
    }

    return null;
  };
}

export function getPasswordPolicyValidationError(
  password: string,
  policy: PasswordPolicySettings
): string | undefined {
  if (policy.requireUppercase && !/[A-Z]/.test(password)) {
    return 'Password must contain at least one uppercase letter.';
  }

  if (policy.requireLowercase && !/[a-z]/.test(password)) {
    return 'Password must contain at least one lowercase letter.';
  }

  if (policy.requireDigit && !/\d/.test(password)) {
    return 'Password must contain at least one digit.';
  }

  if (policy.requireSpecialCharacter && !/[^A-Za-z0-9]/.test(password)) {
    return 'Password must contain at least one special character.';
  }

  return undefined;
}

export function applyPasswordPolicyRules(
  passwordPath: SchemaPath<string>,
  policy: PasswordPolicySettings
): void {
  required(passwordPath, { when: whenTouched, message: 'Password is required.' });
  minLength(passwordPath, policy.minimumLength, {
    when: whenTouched,
    message: `Password must be at least ${policy.minimumLength} characters long.`
  });
  maxLength(passwordPath, policy.maximumLength, {
    when: whenTouched,
    message: `Password must be less than ${policy.maximumLength} characters long.`
  });
  validate(passwordPath, ({ value, state }) => {
    if (!state.touched()) {
      return undefined;
    }

    const message = getPasswordPolicyValidationError(value(), policy);
    return message ? { kind: 'passwordPolicy', message } : undefined;
  });
}
