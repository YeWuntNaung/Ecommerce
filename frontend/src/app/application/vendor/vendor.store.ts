import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { toUserMessage } from '@application/common/to-user-message';
import { Category } from '@domain/catalog/models';
import { CatalogGateway } from '@domain/catalog/ports/catalog.gateway';
import { LoadStatus } from '@domain/shared/load-status';
import { PagedResponse } from '@domain/shared/paged';
import { CreateProductDraft, VendorProduct, VendorProfile } from '@domain/vendor/models';
import { VendorGateway } from '@domain/vendor/ports/vendor.gateway';

const PAGE_SIZE = 20;

interface VendorState {
  profile: VendorProfile | null;
  products: PagedResponse<VendorProduct> | null;
  categories: Category[];
  status: LoadStatus;
  error: string | null;
  saving: boolean;
  actionError: string | null;
  actionMessage: string | null;
}

const initialState: VendorState = {
  profile: null,
  products: null,
  categories: [],
  status: 'idle',
  error: null,
  saving: false,
  actionError: null,
  actionMessage: null,
};

export const VendorStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods(
    (
      store,
      gateway = inject(VendorGateway),
      catalog = inject(CatalogGateway),
    ) => {
      const load = async (page = 1): Promise<void> => {
        patchState(store, { status: 'loading', error: null });

        try {
          const [profile, products, categories] = await Promise.all([
            firstValueFrom(gateway.getMyVendor()),
            firstValueFrom(gateway.listMyProducts(page, PAGE_SIZE)),
            firstValueFrom(catalog.listCategories()),
          ]);

          patchState(store, { profile, products, categories, status: 'loaded' });
        } catch (error) {
          patchState(store, {
            status: 'error',
            error: toUserMessage(error, 'Your vendor workspace could not be loaded.'),
          });
        }
      };

      return {
        load,

        async createDraft(
          request: CreateProductDraft,
          submitForReview: boolean,
        ): Promise<boolean> {
          patchState(store, { saving: true, actionError: null, actionMessage: null });

          try {
            const created = await firstValueFrom(gateway.createDraft(request));

            if (submitForReview) {
              await firstValueFrom(gateway.submitForReview(created.id));
              patchState(store, {
                saving: false,
                actionMessage: `“${request.name}” was created and sent for review.`,
              });
            } else {
              patchState(store, {
                saving: false,
                actionMessage: `“${request.name}” was saved as a draft.`,
              });
            }

            await load(1);
            return true;
          } catch (error) {
            patchState(store, {
              saving: false,
              actionError: toUserMessage(error, 'The product could not be saved.'),
            });
            return false;
          }
        },

        async submitForReview(productId: string, productName: string): Promise<void> {
          patchState(store, { saving: true, actionError: null, actionMessage: null });

          try {
            await firstValueFrom(gateway.submitForReview(productId));
            patchState(store, {
              saving: false,
              actionMessage: `“${productName}” was sent for review.`,
            });
            await load(store.products()?.page ?? 1);
          } catch (error) {
            patchState(store, {
              saving: false,
              actionError: toUserMessage(error, 'The product could not be submitted.'),
            });
          }
        },

        clearAction(): void {
          patchState(store, { actionError: null, actionMessage: null });
        },
      };
    },
  ),
);
