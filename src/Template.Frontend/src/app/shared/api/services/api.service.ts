import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { getApiUrl } from '../../../../environments/runtime-config';
import { Result, ResultStatus, ValidationError } from '../models/api-response.model';

const HTTP_STATUS_TO_RESULT: Partial<Record<number, ResultStatus>> = {
  400: ResultStatus.Invalid,
  401: ResultStatus.Unauthorized,
  403: ResultStatus.Forbidden,
  404: ResultStatus.NotFound,
  409: ResultStatus.Conflict,
  500: ResultStatus.CriticalError,
  502: ResultStatus.CriticalError,
  503: ResultStatus.Unavailable,
  504: ResultStatus.CriticalError
};

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly baseUrl = getApiUrl() + '/';

  private readonly http = inject(HttpClient);

  public get<T>(endpoint: string, params?: object): Observable<T> {
    const httpParams = this.buildHttpParams(params);

    return this.pipeResult(
      this.http.get<Result<T>>(`${this.baseUrl}${endpoint}`, { params: httpParams })
    );
  }

  public post<T>(endpoint: string, body: unknown): Observable<T> {
    return this.pipeResult(
      this.http.post<Result<T>>(`${this.baseUrl}${endpoint}`, body)
    );
  }

  public put<T>(endpoint: string, body: unknown): Observable<T> {
    return this.pipeResult(
      this.http.put<Result<T>>(`${this.baseUrl}${endpoint}`, body)
    );
  }

  public delete<T>(endpoint: string): Observable<T> {
    return this.pipeResult(this.http.delete<Result<T>>(`${this.baseUrl}${endpoint}`));
  }

  public postFormData<T>(endpoint: string, formData: FormData): Observable<T> {
    return this.pipeResult(
      this.http.post<Result<T>>(`${this.baseUrl}${endpoint}`, formData)
    );
  }

  public getBlob(endpoint: string): Observable<Blob> {
    return this.http
      .get(`${this.baseUrl}${endpoint}`, { responseType: 'blob' })
      .pipe(catchError((error: unknown) => throwError(() => this.toError(error))));
  }

  private pipeResult<T>(source: Observable<Result<T>>): Observable<T> {
    return source.pipe(
      map((response) => this.handleResponse(response)),
      catchError((error: unknown) => throwError(() => this.toError(error)))
    );
  }

  private handleResponse<T>(result: Result<T>): T {
    if (result.isSuccess) {
      return result.value as T;
    }

    throw new Error(this.getErrorMessageFromResult(result));
  }

  private toError(error: unknown): Error {
    if (error instanceof HttpErrorResponse) {
      const result = this.parseResultBody(error.error);
      if (result) {
        return new Error(this.getErrorMessageFromResult(result));
      }

      if (error.status === 0) {
        return new Error(
          "We couldn't connect to the server. Check your internet connection and try again."
        );
      }

      return new Error(this.getDefaultErrorMessageFromHttpStatus(error.status));
    }

    if (error instanceof Error) {
      return error;
    }

    return new Error(this.getDefaultErrorMessage(ResultStatus.Error));
  }

  private parseResultBody(body: unknown): Result<unknown> | null {
    if (!body || typeof body !== 'object') {
      return null;
    }

    const candidate = body as Partial<Result<unknown>>;
    if (typeof candidate.isSuccess !== 'boolean') {
      return null;
    }

    return candidate as Result<unknown>;
  }

  private getErrorMessageFromResult<T>(result: Result<T>): string {
    const messages: string[] = [];

    if (result.validationErrors?.length) {
      messages.push(
        ...result.validationErrors.map((validationError) =>
          this.formatValidationError(validationError)
        )
      );
    }

    if (result.errors?.length) {
      messages.push(...result.errors.filter((message) => message.trim().length > 0));
    }

    if (messages.length === 0) {
      messages.push(
        this.getDefaultErrorMessage(this.normalizeResultStatus(result.status))
      );
    }

    return [...new Set(messages)].join(' ');
  }

  private formatValidationError(validationError: ValidationError): string {
    const message = validationError.errorMessage?.trim();
    if (message) {
      return message;
    }

    return `Please check ${this.humanizeFieldName(validationError.identifier)}.`;
  }

  private humanizeFieldName(identifier: string): string {
    const normalized = identifier
      .replaceAll(/[[\].]/g, ' ')
      .replaceAll(/([a-z])([A-Z])/g, '$1 $2')
      .replaceAll('_', ' ')
      .trim()
      .toLowerCase();

    return normalized.length > 0 ? normalized : 'this field';
  }

  private normalizeResultStatus(status: unknown): ResultStatus {
    if (typeof status === 'number') {
      return status;
    }

    if (typeof status === 'string') {
      const mapped = ResultStatus[status as keyof typeof ResultStatus];
      if (typeof mapped === 'number') {
        return mapped;
      }
    }

    return ResultStatus.Error;
  }

  private getDefaultErrorMessage(status: ResultStatus): string {
    switch (status) {
      case ResultStatus.NotFound:
        return "We couldn't find what you're looking for. It may have been removed or the link is no longer valid.";
      case ResultStatus.Unauthorized:
        return 'Please sign in to continue.';
      case ResultStatus.Forbidden:
        return "You don't have permission to do that.";
      case ResultStatus.Invalid:
        return "Some of the information isn't valid. Please review the form and try again.";
      case ResultStatus.Conflict:
        return "This couldn't be completed because something changed. Refresh the page and try again.";
      case ResultStatus.Unavailable:
        return 'The service is temporarily unavailable. Please try again in a moment.';
      case ResultStatus.CriticalError:
        return 'Something went wrong on our end. Please try again later.';
      default:
        return 'Something went wrong. Please try again.';
    }
  }

  private getDefaultErrorMessageFromHttpStatus(httpStatus: number): string {
    const status = HTTP_STATUS_TO_RESULT[httpStatus] ?? ResultStatus.Error;
    return this.getDefaultErrorMessage(status);
  }

  private buildHttpParams(params?: object): HttpParams {
    let httpParams = new HttpParams();

    if (params) {
      Object.entries(params as Record<string, unknown>).forEach(([key, value]) => {
        if (value === null || value === undefined) {
          return;
        }

        if (Array.isArray(value)) {
          value.forEach((item) => {
            if (item !== null && item !== undefined) {
              httpParams = httpParams.append(key, this.serializeQueryParamValue(item));
            }
          });

          return;
        }

        httpParams = httpParams.set(key, this.serializeQueryParamValue(value));
      });
    }

    return httpParams;
  }

  private serializeQueryParamValue(value: unknown): string {
    if (
      typeof value === 'string' ||
      typeof value === 'number' ||
      typeof value === 'boolean' ||
      typeof value === 'bigint'
    ) {
      return String(value);
    }

    if (value instanceof Date) {
      return value.toISOString();
    }

    return JSON.stringify(value);
  }
}
