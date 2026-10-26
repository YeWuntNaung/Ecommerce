import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ProductSummary, Storefront, StorefrontProduct } from '@domain/catalog/models';
import { CatalogGateway } from '@domain/catalog/ports/catalog.gateway';
import { PagedResponse } from '@domain/shared/paged';
import { Observable, of, throwError } from 'rxjs';
import { CatalogStore } from './catalog.store';

const storefront: Storefront = {
  id: 'vendor-1',
  name: 'Demo Store',
  slug: 'demo-store',
  description: 'A demonstration storefront.',
};

const summary: ProductSummary = {
  id: 'product-1',
  name: 'Wireless Earbuds',
  slug: 'wireless-earbuds',
  priceFromKyat: 85_000,
  currency: 'MMK',
  vendorName: 'Demo Store',
  vendorSlug: 'demo-store',
  categoryName: 'Electronics',
};

const page: PagedResponse<ProductSummary> = {
  items: [summary],
  page: 1,
  pageSize: 12,
  totalCount: 1,
  totalPages: 1,
};

const product: StorefrontProduct = {
  id: 'product-1',
  vendorId: 'vendor-1',
  name: 'Wireless Earbuds',
  slug: 'wireless-earbuds',
  description: 'Compact Bluetooth earbuds.',
  categoryName: 'Electronics',
  vendorName: 'Demo Store',
  vendorSlug: 'demo-store',
  variants: [],
};

class FakeCatalogGateway extends CatalogGateway {
  getStorefront = vi.fn((_slug: string): Observable<Storefront> => of(storefront));
  listStorefrontProducts = vi.fn(
    (_slug: string, _page: number, _pageSize: number): Observable<PagedResponse<ProductSummary>> =>
      of(page),
  );
  getStorefrontProduct = vi.fn(
    (_vendorSlug: string, _productSlug: string): Observable<StorefrontProduct> => of(product),
  );
  listCategories = vi.fn(() => of([]));
}

describe('CatalogStore', () => {
  let gateway: FakeCatalogGateway;
  let store: InstanceType<typeof CatalogStore>;

  beforeEach(() => {
    gateway = new FakeCatalogGateway();

    TestBed.configureTestingModule({
      providers: [{ provide: CatalogGateway, useValue: gateway }],
    });

    store = TestBed.inject(CatalogStore);
  });

  it('loads a storefront and its approved products', async () => {
    await store.loadStorefront('demo-store');

    expect(store.status()).toBe('loaded');
    expect(store.storefront()?.name).toBe('Demo Store');
    expect(store.products()?.items).toHaveLength(1);
    expect(gateway.listStorefrontProducts).toHaveBeenCalledWith('demo-store', 1, 12);
  });

  it('turns a missing storefront into a readable error', async () => {
    gateway.getStorefront.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 404,
            error: { detail: 'Storefront not found.' },
          }),
      ),
    );

    await store.loadStorefront('missing-store');

    expect(store.status()).toBe('error');
    expect(store.error()).toBe('Storefront not found.');
  });

  it('loads a product detail', async () => {
    await store.loadProduct('demo-store', 'wireless-earbuds');

    expect(store.productStatus()).toBe('loaded');
    expect(store.product()?.name).toBe('Wireless Earbuds');
    expect(gateway.getStorefrontProduct).toHaveBeenCalledWith(
      'demo-store',
      'wireless-earbuds',
    );
  });
});
