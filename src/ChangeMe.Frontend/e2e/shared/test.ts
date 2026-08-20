import { test as base } from '@playwright/test';
import { E2eApiClient } from './api/client';

export const test = base.extend<{ apiClient: E2eApiClient }>({
  apiClient: async ({ request }, use) => {
    await use(new E2eApiClient(request));
  }
});

export { expect } from '@playwright/test';
