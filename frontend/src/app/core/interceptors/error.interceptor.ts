import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ApiErrorResponse } from '../models/shared';
import { AuthStorageService } from '../services/auth-storage.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authStorage = inject(AuthStorageService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        authStorage.clear();
      }

      return throwError(() => normalizeError(error));
    })
  );
};

function normalizeError(error: HttpErrorResponse): ApiErrorResponse {
  const errors = getErrors(error);

  return {
    status: error.status,
    message: getMessage(error, errors),
    errors,
    traceId: getTraceId(error),
    url: error.url ?? undefined,
  };
}

function getMessage(error: HttpErrorResponse, errors: string[]): string {
  if (typeof error.error?.message === 'string' && error.error.message.trim().length > 0) {
    return error.error.message;
  }

  if (errors.length > 0) {
    return errors[0];
  }

  switch (error.status) {
    case 0:
      return 'Unable to connect to the server. Please check your connection.';
    case 400:
      return 'The request could not be processed.';
    case 401:
      return 'Your session has expired. Please sign in again.';
    case 403:
      return 'You do not have permission to perform this action.';
    case 404:
      return 'The requested resource was not found.';
    case 500:
      return 'A server error occurred. Please try again later.';
    default:
      return 'An unexpected error occurred.';
  }
}

function getErrors(error: HttpErrorResponse): string[] {
  if (Array.isArray(error.error?.errors)) {
    return error.error.errors.map(String);
  }

  if (typeof error.error?.errors === 'object' && error.error.errors !== null) {
    return Object.values(error.error.errors).flat().map(String);
  }

  return [];
}

function getTraceId(error: HttpErrorResponse): string | undefined {
  return typeof error.error?.traceId === 'string' ? error.error.traceId : undefined;
}
