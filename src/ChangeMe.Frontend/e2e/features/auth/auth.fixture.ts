import { expect, type Page } from '@playwright/test';

export { loginViaUi as login } from '../../shared/auth.fixture';

export async function logout(page: Page): Promise<void> {
  await page.getByRole('banner').getByRole('button', { name: 'Logout' }).click();
  await expect(page.getByText('Welcome back')).toBeVisible({ timeout: 15_000 });
}
