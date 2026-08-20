import { expect, type APIRequestContext } from '@playwright/test';
import { e2eApiUrl, e2eCredentials } from '../env';

interface ApiResult<T> {
  value?: T;
}

interface LoginResult {
  authSession?: {
    token?: string;
  } | null;
}

let cachedToken: string | null = null;

export class E2eApiClient {
  constructor(private readonly request: APIRequestContext) {}

  async token(): Promise<string> {
    if (cachedToken) {
      return cachedToken;
    }

    const response = await this.request.post(`${e2eApiUrl}/auth/login`, {
      data: {
        email: e2eCredentials.email,
        password: e2eCredentials.password
      }
    });

    expect(response.ok()).toBeTruthy();

    const body = (await response.json()) as ApiResult<LoginResult>;
    const token = body.value?.authSession?.token;
    expect(token).toBeTruthy();

    cachedToken = token!;
    return cachedToken;
  }

  private async authHeaders(): Promise<Record<string, string>> {
    return {
      Authorization: `Bearer ${await this.token()}`
    };
  }

  async get<T>(path: string): Promise<T> {
    const response = await this.request.get(`${e2eApiUrl}${path}`, {
      headers: await this.authHeaders()
    });

    expect(response.ok()).toBeTruthy();

    const body = (await response.json()) as ApiResult<T>;
    expect(body.value).toBeTruthy();

    return body.value!;
  }

  async post<T>(path: string, data: unknown): Promise<T> {
    const response = await this.request.post(`${e2eApiUrl}${path}`, {
      headers: await this.authHeaders(),
      data
    });

    expect(response.ok()).toBeTruthy();

    const body = (await response.json()) as ApiResult<T>;
    expect(body.value).toBeTruthy();

    return body.value!;
  }

  async delete(path: string): Promise<void> {
    const response = await this.request.delete(`${e2eApiUrl}${path}`, {
      headers: await this.authHeaders()
    });

    expect(response.ok()).toBeTruthy();
  }
}
