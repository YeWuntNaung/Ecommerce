import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { toUserMessage } from '@application/common/to-user-message';
import { ProductSummary, Storefront, StorefrontProduct } from '@domain/catalog/models';
import { CatalogGateway } from '@domain/catalog/ports/catalog.gateway';
import { LoadStatus } from '@domain/shared/load-status';
import { PagedResponse } from '@domain/shared/paged';

const PAGE_SIZE = 12;

interface CatalogState {
  slug: string | null;
  storefront: Storefront | null;
  products: PagedResponse<ProductSummary> | null;
  status: LoadStatus;
  error: string | null;

  product: StorefrontProduct | null;
  productStatus: LoadStatus;
  productError: string | null;
}

const initialState: CatalogState = {
  slug: null,
  storefront: null,
  products: null,
  status: 'idle',
  error: null,
  product: null,
  productStatus: 'idle',
  productError: null,
};

export const CatalogStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, gateway = inject(CatalogGateway)) => ({
    async loadStorefront(slug: string, page = 1): Promise<void> {
      const sameStore = store.slug() === slug;

      patchState(
        store,
        sameStore
          ? { status: 'loading', error: null }
          : {
              slug,
              storefront: null,
              products: null,
              status: 'loading',
              error: null,
              product: null,
              productStatus: 'idle',
              productError: null,
            },
      );

      try {
        const [storefront, products] = await Promise.all([
          firstValueFrom(gateway.getStorefront(slug)),
          firstValueFrom(gateway.listStorefrontProducts(slug, page, PAGE_SIZE)),
        ]);

        patchState(store, { storefront, products, status: 'loaded' });
      } catch (error) {
        patchState(store, {
          status: 'error',
          error: toUserMessage(error, 'This storefront could not be loaded.'),
        });
      }
    },

    async loadProduct(vendorSlug: string, productSlug: string): Promise<void> {
      patchState(store, { product: null, productStatus: 'loading', productError: null });

      try {
        const product = await firstValueFrom(
          gateway.getStorefrontProduct(vendorSlug, productSlug),
        );
        patchState(store, { product, productStatus: 'loaded' });
      } catch (error) {
        patchState(store, {
          productStatus: 'error',
          productError: toUserMessage(error, 'This product could not be loaded.'),
        });
      }
    },
  })),
);
