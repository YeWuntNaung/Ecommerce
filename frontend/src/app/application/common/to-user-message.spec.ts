import { HttpErrorResponse } from '@angular/common/http';
import { toUserMessage } from './to-user-message';

describe('toUserMessage', () => {
  it('prefers the first validation message from ProblemDetails', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        title: 'One or more validation errors occurred.',
        errors: { name: ['Product name must be at least 3 characters.'] },
      },
    });

    expect(toUserMessage(error, 'fallback')).toBe(
      'Product name must be at least 3 characters.',
    );
  });

  it('falls back to the ProblemDetails detail', () => {
    const error = new HttpErrorResponse({
      status: 401,
      error: { detail: 'Invalid username or password.' },
    });

    expect(toUserMessage(error, 'fallback')).toBe('Invalid username or password.');
  });

  it('explains an unreachable API', () => {
    const error = new HttpErrorResponse({ status: 0 });

    expect(toUserMessage(error, 'fallback')).toContain('Cannot reach');
  });

  it('uses the fallback for unknown errors', () => {
    expect(toUserMessage(new Error('boom'), 'Something went wrong.')).toBe(
      'Something went wrong.',
    );
  });
});
