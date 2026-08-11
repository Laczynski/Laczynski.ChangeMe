import { expect, type Page } from '@playwright/test';

export async function gotoRolesList(page: Page): Promise<void> {
  await page.goto('/roles');
  await expectRolesList(page);
}

export async function expectRolesList(page: Page): Promise<void> {
  await expect(page).toHaveURL(/\/roles/);
  await expect(
    page.getByText('Browse, search and manage roles and permissions.')
  ).toBeVisible({
    timeout: 20_000
  });
}

export async function fillRoleName(page: Page, name: string): Promise<void> {
  await page.getByRole('textbox', { name: 'Name' }).fill(name);
}

export async function expectDetailsTitle(page: Page, title: string): Promise<void> {
  await expect(page.getByRole('main')).toContainText(title);
}

export async function selectViewUsersPermission(page: Page): Promise<void> {
  await page.getByRole('checkbox', { name: 'View users' }).click();
}
