import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '@core/config/api-base-url';
import { CatalogGateway } from '@domain/catalog/ports/catalog.gateway';
import { Category, ProductSummary, Storefront, StorefrontProduct } from '@domain/catalog/models';
import { PagedResponse } from '@domain/shared/paged';

@Injectable()
export class HttpCatalogGateway extends CatalogGateway {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  override getStorefront(slug: string): Observable<Storefront> {
    return this.http.get<Storefront>(`${this.baseUrl}/api/v1/stores/${encodeURIComponent(slug)}`);
  }

  override listStorefrontProducts(
    slug: string,
    page: number,
    pageSize: number,
  ): Observable<PagedResponse<ProductSummary>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResponse<ProductSummary>>(
      `${this.baseUrl}/api/v1/stores/${encodeURIComponent(slug)}/products`,
      { params },
    );
  }

  override getStorefrontProduct(
    vendorSlug: string,
    productSlug: string,
  ): Observable<StorefrontProduct> {
    return this.http.get<StorefrontProduct>(
      `${this.baseUrl}/api/v1/stores/${encodeURIComponent(vendorSlug)}/products/${encodeURIComponent(productSlug)}`,
    );
  }

  override listCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.baseUrl}/api/v1/categories`);
  }
}
