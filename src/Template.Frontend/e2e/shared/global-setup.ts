import { chromium } from '@playwright/test';
import { loginViaUi } from './auth.fixture';
import { authStoragePath, e2eBaseUrl } from './env';

async function globalSetup(): Promise<void> {
  const browser = await chromium.launch();
  const context = await browser.newContext({ baseURL: e2eBaseUrl });
  const page = await context.newPage();

  await loginViaUi(page);
  await context.storageState({ path: authStoragePath });

  await browser.close();
}

export default globalSetup;
