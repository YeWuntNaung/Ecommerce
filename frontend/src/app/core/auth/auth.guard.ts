import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from '@application/auth/auth.store';

/**
 * Route guard factory. It resolves the session once, then checks both
 * authentication and (optionally) one of the required roles.
 */
export function requireRoles(roles: readonly string[]): CanActivateFn {
  return async (_route, state) => {
    const auth = inject(AuthStore);
    const router = inject(Router);

    await auth.ensureSession();

    if (!auth.isAuthenticated()) {
      return router.createUrlTree(['/login'], {
        queryParams: state.url && state.url !== '/' ? { returnUrl: state.url } : {},
      });
    }

    if (roles.length > 0 && !roles.some((role) => auth.roles().includes(role))) {
      return router.createUrlTree(['/']);
    }

    return true;
  };
}

export const requireAuth = requireRoles([]);
