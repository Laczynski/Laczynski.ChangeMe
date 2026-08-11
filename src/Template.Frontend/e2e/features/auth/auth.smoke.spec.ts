import { expect, test } from '@playwright/test';
import { login, logout } from './auth.fixture';

test.describe('auth smoke', () => {
  test('redirects unauthenticated users to login', async ({ page }) => {
    await page.goto('/issues');

    await expect(page).toHaveURL(/\/login\?returnUrl=%2Fissues/);
    await expect(page.getByText('Welcome back')).toBeVisible();
  });

  test('signs in and signs out with a browser session', async ({ page }) => {
    await login(page);
    await page.reload();
    await expect(page).toHaveURL(/\/issues/);

    await logout(page);
    await page.goto('/issues');
    await expect(page).toHaveURL(/\/login/);
  });
});
