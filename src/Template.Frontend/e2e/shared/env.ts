import path from 'node:path';
import { fileURLToPath } from 'node:url';

const e2eDir = path.dirname(fileURLToPath(import.meta.url));

export const e2eBaseUrl = process.env['E2E_BASE_URL'] ?? 'http://localhost:4200';
export const e2eApiUrl = process.env['E2E_API_URL'] ?? 'http://localhost:5000/api/v1';

export const e2eCredentials = {
  email: process.env['E2E_USER_EMAIL'] ?? 'admin@example.local',
  password: process.env['E2E_USER_PASSWORD'] ?? 'admin123'
};

export const e2eTestPassword = 'StrongPass123!';

export const authStoragePath = path.join(e2eDir, 'auth-storage.json');

export function e2eTitle(feature: string): string {
  return `E2E-${feature}-${Date.now()}`;
}

export function e2eEmail(feature: string): string {
  return `E2E-${feature}-${crypto.randomUUID()}@example.com`;
}
