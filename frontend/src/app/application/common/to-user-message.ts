import { HttpErrorResponse } from '@angular/common/http';

interface ProblemBody {
  detail?: unknown;
  title?: unknown;
  errors?: unknown;
}

function firstValidationMessage(errors: unknown): string | null {
  if (!errors || typeof errors !== 'object') {
    return null;
  }

  for (const value of Object.values(errors as Record<string, unknown>)) {
    if (Array.isArray(value)) {
      const message = value.find(
        (item): item is string => typeof item === 'string' && item.trim().length > 0,
      );
      if (message) {
        return message;
      }
    } else if (typeof value === 'string' && value.trim().length > 0) {
      return value;
    }
  }

  return null;
}

/**
 * Turns an unknown failure into a sentence a shopper or vendor can act on.
 * Validation messages from ProblemDetails win, because they are the most
 * specific thing the API can say.
 */
export function toUserMessage(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  if (error.status === 0) {
    return 'Cannot reach the marketplace. Check your connection and try again.';
  }

  const body = error.error as ProblemBody | string | null;

  if (body && typeof body === 'object') {
    const validation = firstValidationMessage(body.errors);
    if (validation) {
      return validation;
    }

    if (typeof body.detail === 'string' && body.detail.trim().length > 0) {
      return body.detail;
    }

    if (
      typeof body.title === 'string' &&
      body.title.trim().length > 0 &&
      !body.title.startsWith('One or more validation')
    ) {
      return body.title;
    }
  }

  if (typeof body === 'string' && body.trim().length > 0 && body.length < 300) {
    return body;
  }

  switch (error.status) {
    case 401:
      return 'Please sign in to continue.';
    case 403:
      return 'Your account does not have permission to do that.';
    case 404:
      return 'We could not find what you were looking for.';
    case 409:
      return 'That action conflicts with the current state. Refresh and try again.';
    default:
      return fallback;
  }
}
