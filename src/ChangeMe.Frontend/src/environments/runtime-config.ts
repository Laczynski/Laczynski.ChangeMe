import { environment } from './environment';

export interface TemplateRuntimeConfig {
  apiUrl: string;
}

declare global {
  var __CHANGE_ME_CONFIG__: TemplateRuntimeConfig | undefined;
}

/** Development: `environment.development.ts`. Production: `runtime-config.js` (required). */
export function getApiUrl(): string {
  if (!environment.production) {
    return (environment as typeof environment & { apiUrl: string }).apiUrl;
  }

  const url = globalThis.__CHANGE_ME_CONFIG__?.apiUrl?.trim();
  if (!url) {
    throw new Error(
      'Missing globalThis.__CHANGE_ME_CONFIG__.apiUrl. Set public/runtime-config.js or CHANGE_ME_API_URL at container start.'
    );
  }

  return url;
}

function getHubPathFromApiUrl(apiUrl: string): string {
  return apiUrl.replace(/\/api(?:\/v\d+)?\/?$/, '/hubs/notifications');
}

export function getNotificationsHubUrl(): string {
  return getHubPathFromApiUrl(getApiUrl());
}
