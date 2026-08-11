export interface Result<T> {
  isSuccess: boolean;
  value?: T;
  status: ResultStatus;
  errors?: string[];
  validationErrors?: ValidationError[];
  correlationId?: string;
  location?: string;
}

export interface ValidationError {
  identifier: string;
  errorMessage: string;
  errorCode?: string;
  severity: ValidationSeverity;
}

export enum ResultStatus {
  Ok = 0,
  Error = 1,
  Forbidden = 2,
  Unauthorized = 3,
  Invalid = 4,
  NotFound = 5,
  Conflict = 6,
  CriticalError = 7,
  Unavailable = 8,
  Created = 9,
  NoContent = 10
}

export enum ValidationSeverity {
  Error = 0,
  Warning = 1,
  Info = 2
}
