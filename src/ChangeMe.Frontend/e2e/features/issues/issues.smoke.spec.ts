import { e2eTitle } from '../../shared/env';
import { expect, test } from '../../shared/test';
import { createIssue } from './issues.api';
import { gotoIssuesList } from './issues.fixture';

test.describe('issues smoke', () => {
  test('opens issue details from the list', async ({ page, apiClient }) => {
    const issueTitle = e2eTitle('issues-list');
    await createIssue(apiClient, issueTitle);

    await gotoIssuesList(page);
    await page.getByRole('link', { name: issueTitle }).click();

    await expect(
      page.getByRole('button', { name: 'Back to issues list' })
    ).toBeVisible();
    await expect(page.getByRole('main').getByText(issueTitle)).toBeVisible();
  });

  test('filters the issues list by search text', async ({ page, apiClient }) => {
    const issueTitle = e2eTitle('issues-filter');
    await createIssue(apiClient, issueTitle);

    await gotoIssuesList(page);
    await page.getByPlaceholder('Search issues…').fill(issueTitle);

    await expect(page.getByRole('link', { name: issueTitle })).toBeVisible();
  });

  test('creates an issue through the form', async ({ page }) => {
    const issueTitle = e2eTitle('issues-create');

    await gotoIssuesList(page);
    await page.getByRole('link', { name: 'Add issue' }).click();
    await expect(
      page.getByText('Create a new issue and move to its details page after saving')
    ).toBeVisible();
    await page.getByLabel('Title').fill(issueTitle);
    await page.getByLabel('Description').fill('Created by Playwright E2E.');
    await page.getByRole('button', { name: 'Create issue' }).click();

    await expect(page.getByRole('button', { name: 'Back to issues list' })).toBeVisible(
      {
        timeout: 20_000
      }
    );
    await expect(page.getByRole('main').getByText(issueTitle)).toBeVisible();
  });
});
