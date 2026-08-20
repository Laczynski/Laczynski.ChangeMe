import { e2eTitle } from '../../shared/env';
import { expect, test } from '../../shared/test';
import {
  expectDetailsTitle,
  fillRoleName,
  gotoRolesList,
  selectViewUsersPermission
} from './roles.fixture';

test.describe('roles smoke', () => {
  test('lists roles, creates a custom role, and edits it', async ({ page }) => {
    const roleName = e2eTitle('roles');
    const updatedRoleName = `${roleName}-edited`;

    await test.step('open roles list', async () => {
      await gotoRolesList(page);
    });

    await test.step('create role', async () => {
      await page.getByRole('link', { name: 'Add role' }).click();
      await expect(page).toHaveURL(/\/roles\/create/);

      await fillRoleName(page, roleName);
      await selectViewUsersPermission(page);
      await page.getByRole('button', { name: 'Create role' }).click();

      await expect(page).toHaveURL(/\/roles\/[0-9a-f-]+/i, { timeout: 20_000 });
      await expectDetailsTitle(page, roleName);
    });

    await test.step('edit role', async () => {
      await page.getByRole('link', { name: 'Edit' }).click();
      await expect(page).toHaveURL(/\/roles\/[0-9a-f-]+\/edit/);
      await fillRoleName(page, updatedRoleName);
      await page.getByRole('button', { name: 'Save changes' }).click();

      await expect(page).toHaveURL(/\/roles\/[0-9a-f-]+(?!\/edit)/, {
        timeout: 20_000
      });
      await expectDetailsTitle(page, updatedRoleName);
    });
  });

  test('opens role details from the list', async ({ page }) => {
    await gotoRolesList(page);

    const firstRoleLink = page.locator('table tbody tr').first().getByRole('link');
    const roleName = (await firstRoleLink.textContent())?.trim();
    expect(roleName).toBeTruthy();

    await firstRoleLink.click();
    await expect(page).toHaveURL(/\/roles\/[0-9a-f-]+/i);
    await expect(
      page.getByRole('button', { name: 'Back to roles list' })
    ).toBeVisible();
    await expectDetailsTitle(page, roleName!);
  });
});
