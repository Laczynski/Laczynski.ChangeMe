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

export { PermissionCodes } from '@shared/authorization/permission-codes';

export function formatIpAddress(ipAddress: string | null | undefined): string {
  return ipAddress?.trim() ? ipAddress : 'Unknown';
}
