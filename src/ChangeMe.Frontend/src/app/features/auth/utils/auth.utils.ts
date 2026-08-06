export const AuthConstraints = {
  NAME_MAX_LENGTH: 100,
  EMAIL_MAX_LENGTH: 320,
  PASSWORD_MIN_LENGTH: 8,
  PASSWORD_MAX_LENGTH: 128,
  RENEWAL_LEAD_TIME_MS: 60_000,
  MIN_RENEWAL_SCHEDULE_MS: 5_000
};

export const AuthMessages = {
  invalidCredentials: 'Invalid email or password.',
  deactivatedAccount: 'This account has been deactivated. Contact an administrator.',
  profileUpdated: 'Profile updated.',
  permissionDenied: 'You do not have permission to perform this action.'
};

export const AuthFieldErrors = {
  email: {
    required: 'Email is required.',
    email: 'Enter a valid email address.',
    maxlength: `Email must be less than ${AuthConstraints.EMAIL_MAX_LENGTH} characters long.`
  },
  password: {
    required: 'Password is required.',
    minlength: `Password must be at least ${AuthConstraints.PASSWORD_MIN_LENGTH} characters long.`,
    maxlength: `Password must be less than ${AuthConstraints.PASSWORD_MAX_LENGTH} characters long.`,
    passwordPolicy: (error: unknown) =>
      typeof error === 'string' ? error : 'Password does not meet policy requirements.'
  },
  firstName: {
    required: 'First name is required.',
    maxlength: `First name must be at most ${AuthConstraints.NAME_MAX_LENGTH} characters.`
  },
  lastName: {
    required: 'Last name is required.',
    maxlength: `Last name must be at most ${AuthConstraints.NAME_MAX_LENGTH} characters.`
  }
} as const;

export { PermissionCodes } from '@shared/authorization/permission-codes';

export function formatIpAddress(ipAddress: string | null | undefined): string {
  return ipAddress?.trim() ? ipAddress : 'Unknown';
}
