export interface UserNameFields {
  firstName: string;
  lastName: string;
}

export interface UserReferenceFields extends UserNameFields {
  email: string;
}

export function formatUserName(profile: UserNameFields): string {
  return [profile.firstName.trim(), profile.lastName.trim()].filter(Boolean).join(' ');
}

export function formatUserReference(profile: UserReferenceFields): string {
  const name = formatUserName(profile);
  return name ? `${name} (${profile.email})` : profile.email;
}
