import { expect, type Page } from '@playwright/test';

export async function gotoIssuesList(page: Page): Promise<void> {
  await page.goto('/issues');
  await expectIssuesList(page);
}

export async function expectIssuesList(page: Page): Promise<void> {
  await expect(page).toHaveURL(/\/issues/);
  await expect(page.getByText('Browse, filter and monitor issues.')).toBeVisible({
    timeout: 20_000
  });
}
