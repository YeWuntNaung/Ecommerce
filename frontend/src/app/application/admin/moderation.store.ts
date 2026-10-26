import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { toUserMessage } from '@application/common/to-user-message';
import { PendingProduct } from '@domain/moderation/models';
import { ModerationGateway } from '@domain/moderation/ports/moderation.gateway';
import { LoadStatus } from '@domain/shared/load-status';
import { PagedResponse } from '@domain/shared/paged';

const PAGE_SIZE = 20;

interface ModerationState {
  pending: PagedResponse<PendingProduct> | null;
  status: LoadStatus;
  error: string | null;
  workingId: string | null;
  actionError: string | null;
  actionMessage: string | null;
}

const initialState: ModerationState = {
  pending: null,
  status: 'idle',
  error: null,
  workingId: null,
  actionError: null,
  actionMessage: null,
};

export const ModerationStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, gateway = inject(ModerationGateway)) => {
    const load = async (page = 1): Promise<void> => {
      patchState(store, { status: 'loading', error: null });

      try {
        const pending = await firstValueFrom(gateway.listPending(page, PAGE_SIZE));
        patchState(store, { pending, status: 'loaded' });
      } catch (error) {
        patchState(store, {
          status: 'error',
          error: toUserMessage(error, 'The moderation queue could not be loaded.'),
        });
      }
    };

    return {
      load,

      async approve(product: PendingProduct): Promise<void> {
        patchState(store, {
          workingId: product.id,
          actionError: null,
          actionMessage: null,
        });

        try {
          await firstValueFrom(gateway.approve(product.id));
          patchState(store, {
            workingId: null,
            actionMessage: `“${product.name}” is now live on the storefront.`,
          });
          await load(store.pending()?.page ?? 1);
        } catch (error) {
          patchState(store, {
            workingId: null,
            actionError: toUserMessage(error, 'The product could not be approved.'),
          });
        }
      },

      async reject(product: PendingProduct, reason: string): Promise<boolean> {
        if (reason.trim().length === 0) {
          patchState(store, { actionError: 'A rejection reason is required.' });
          return false;
        }

        patchState(store, {
          workingId: product.id,
          actionError: null,
          actionMessage: null,
        });

        try {
          await firstValueFrom(gateway.reject(product.id, reason.trim()));
          patchState(store, {
            workingId: null,
            actionMessage: `“${product.name}” was rejected with feedback.`,
          });
          await load(store.pending()?.page ?? 1);
          return true;
        } catch (error) {
          patchState(store, {
            workingId: null,
            actionError: toUserMessage(error, 'The product could not be rejected.'),
          });
          return false;
        }
      },

      clearAction(): void {
        patchState(store, { actionError: null, actionMessage: null });
      },
    };
  }),
);
