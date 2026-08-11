import { defineConfig, devices } from '@playwright/test';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { authStoragePath, e2eBaseUrl } from './shared/env';

const e2eDir = path.dirname(fileURLToPath(import.meta.url));
const frontendRoot = path.resolve(e2eDir, '..');
const repoRoot = path.resolve(frontendRoot, '../..');

function backendWebServerEnv(): Record<string, string> {
  const env: Record<string, string> = {
    ASPNETCORE_ENVIRONMENT: 'Development'
  };

  const connectionString = process.env['ConnectionStrings__DefaultConnection'];
  if (connectionString) {
    env['ConnectionStrings__DefaultConnection'] = connectionString;
  }

  return env;
}

export default defineConfig({
  testDir: path.join(e2eDir, 'features'),
  fullyParallel: false,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 1 : 0,
  workers: 1,
  timeout: 60_000,
  globalSetup: path.join(e2eDir, 'shared/global-setup.ts'),
  reporter: process.env['CI']
    ? [['github'], ['html', { open: 'never' }], ['list']]
    : [['list']],
  use: {
    baseURL: e2eBaseUrl,
    screenshot: 'only-on-failure',
    trace: 'retain-on-failure'
  },
  projects: [
    {
      name: 'auth',
      testDir: path.join(e2eDir, 'features/auth'),
      use: {
        ...devices['Desktop Chrome'],
        storageState: { cookies: [], origins: [] }
      }
    },
    {
      name: 'app',
      testIgnore: '**/auth/**',
      use: {
        ...devices['Desktop Chrome'],
        storageState: authStoragePath
      }
    }
  ],
  webServer: [
    ...(process.env['CI']
      ? []
      : [
          {
            command:
              'docker run --rm --name template-e2e-mailhog -p 1025:1025 -p 8025:8025 mailhog/mailhog',
            url: 'http://localhost:8025',
            timeout: 120_000,
            reuseExistingServer: true
          }
        ]),
    {
      command:
        'dotnet run --no-launch-profile --project src/Template.Backend/src/Template.Backend.Web/Template.Backend.Web.csproj',
      cwd: repoRoot,
      env: backendWebServerEnv(),
      url: 'http://localhost:5000/swagger/index.html',
      timeout: 180_000,
      reuseExistingServer: !process.env['CI']
    },
    {
      command: 'npm start',
      cwd: frontendRoot,
      url: e2eBaseUrl,
      timeout: 180_000,
      reuseExistingServer: !process.env['CI']
    }
  ]
});
