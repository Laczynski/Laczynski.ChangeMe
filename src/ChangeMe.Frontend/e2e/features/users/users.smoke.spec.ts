import { e2eEmail, e2eTestPassword } from '../../shared/env';
import { expect, test } from '../../shared/test';
import {
  expectDetailsTitle,
  gotoUsersList,
  selectMultiselectOption
} from './users.fixture';

test.describe('users smoke', () => {
  test('lists users, creates a user, and edits their profile', async ({ page }) => {
    const email = e2eEmail('users');
    const firstName = 'E2E';
    const lastName = 'User';
    const updatedFirstName = 'E2EUpdated';

    await test.step('open users list', async () => {
      await gotoUsersList(page);
    });

    await test.step('create user', async () => {
      await page.getByRole('link', { name: 'Create user' }).click();
      await expect(page).toHaveURL(/\/users\/create/);

      await page.getByLabel('First name').fill(firstName);
      await page.getByLabel('Last name').fill(lastName);
      await page.getByLabel('Email').fill(email);
      await page
        .getByRole('textbox', { name: 'Password', exact: true })
        .fill(e2eTestPassword);
      await page
        .getByRole('textbox', { name: 'Confirm password' })
        .fill(e2eTestPassword);
      await selectMultiselectOption(page, 'Roles', 'User');
      await page.getByRole('button', { name: 'Create user' }).click();

      await expect(page.getByRole('alert')).not.toBeVisible();
      await expect(page).toHaveURL(/\/users\/[0-9a-f-]+/i, { timeout: 20_000 });
      await expectDetailsTitle(page, `${firstName} ${lastName}`);
    });

    await test.step('edit profile', async () => {
      await page.getByRole('link', { name: 'Edit' }).click();
      await expect(page).toHaveURL(/\/users\/[0-9a-f-]+\/edit/);
      await page.getByLabel('First name').fill(updatedFirstName);
      await page.getByRole('button', { name: 'Save changes' }).click();

      await expect(page).toHaveURL(/\/users\/[0-9a-f-]+(?!\/edit)/, {
        timeout: 20_000
      });
      await expectDetailsTitle(page, `${updatedFirstName} ${lastName}`);
    });
  });
});
