import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { toUserMessage } from '@application/common/to-user-message';
import { CurrentUser } from '@domain/identity/models';
import { AuthGateway } from '@domain/identity/ports/auth.gateway';

export type SessionStatus = 'unknown' | 'loading' | 'authenticated' | 'anonymous';

interface AuthState {
  user: CurrentUser | null;
  status: SessionStatus;
  pending: boolean;
  error: string | null;
}

const initialState: AuthState = {
  user: null,
  status: 'unknown',
  pending: false,
  error: null,
};

// Shared across callers so the app shell and a route guard cannot issue two
// concurrent `/me` calls and race each other on the initial navigation.
let sessionRequest: Promise<void> | null = null;

export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    isAuthenticated: computed(() => store.user() !== null),
    roles: computed(() => store.user()?.roles ?? []),
    isAdmin: computed(() => (store.user()?.roles ?? []).includes('Admin')),
    isVendorOwner: computed(() => (store.user()?.roles ?? []).includes('VendorOwner')),
  })),
  withMethods((store, gateway = inject(AuthGateway)) => ({
    /** Resolves the session once per page load. Anonymous is a valid outcome. */
    async ensureSession(): Promise<void> {
      if (store.status() === 'authenticated' || store.status() === 'anonymous') {
        return;
      }

      sessionRequest ??= (async () => {
        patchState(store, { status: 'loading' });

        try {
          const user = await firstValueFrom(gateway.me());
          patchState(store, { user, status: 'authenticated', error: null });
        } catch {
          patchState(store, { user: null, status: 'anonymous' });
        }
      })();

      try {
        await sessionRequest;
      } finally {
        sessionRequest = null;
      }
    },

    async login(username: string, password: string): Promise<boolean> {
      patchState(store, { pending: true, error: null });

      try {
        await firstValueFrom(gateway.login(username, password));
        const user = await firstValueFrom(gateway.me());
        patchState(store, { user, status: 'authenticated', pending: false });
        return true;
      } catch (error) {
        patchState(store, {
          pending: false,
          user: null,
          status: 'anonymous',
          error: toUserMessage(error, 'Sign in failed. Check your username and password.'),
        });
        return false;
      }
    },

    async logout(): Promise<void> {
      try {
        await firstValueFrom(gateway.logout());
      } catch {
        // Clearing local session state matters more than the network call.
      }

      patchState(store, { user: null, status: 'anonymous', pending: false, error: null });
    },

    clearError(): void {
      patchState(store, { error: null });
    },
  })),
);
