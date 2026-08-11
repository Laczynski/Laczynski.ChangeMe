import { expect, type Page } from '@playwright/test';

export async function gotoUsersList(page: Page): Promise<void> {
  await page.goto('/users');
  await expectUsersList(page);
}

export async function expectUsersList(page: Page): Promise<void> {
  await expect(page).toHaveURL(/\/users/);
  await expect(page.getByText('Browse, search and manage user accounts.')).toBeVisible({
    timeout: 20_000
  });
}

export async function selectMultiselectOption(
  page: Page,
  label: string,
  optionLabel: string
): Promise<void> {
  const panel = page
    .getByRole('main')
    .getByRole('region', { name: label, exact: true });
  await panel.locator('.p-select').click();
  await page.getByRole('option', { name: optionLabel, exact: true }).click();
  await page.keyboard.press('Escape');
}

export async function expectDetailsTitle(page: Page, title: string): Promise<void> {
  await expect(page.getByRole('main')).toContainText(title);
}
