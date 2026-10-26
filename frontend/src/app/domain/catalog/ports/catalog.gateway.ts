import { Observable } from 'rxjs';
import { PagedResponse } from '../../shared/paged';
import { Category, ProductSummary, Storefront, StorefrontProduct } from '../models';

/** Port implemented by the infrastructure layer. Presentation never touches HttpClient. */
export abstract class CatalogGateway {
  abstract getStorefront(slug: string): Observable<Storefront>;

  abstract listStorefrontProducts(
    slug: string,
    page: number,
    pageSize: number,
  ): Observable<PagedResponse<ProductSummary>>;

  abstract getStorefrontProduct(
    vendorSlug: string,
    productSlug: string,
  ): Observable<StorefrontProduct>;

  abstract listCategories(): Observable<Category[]>;
}
