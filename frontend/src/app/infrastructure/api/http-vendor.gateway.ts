import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '@core/config/api-base-url';
import { VendorGateway } from '@domain/vendor/ports/vendor.gateway';
import {
  CreatedProduct,
  CreateProductDraft,
  VendorProduct,
  VendorProfile,
} from '@domain/vendor/models';
import { PagedResponse } from '@domain/shared/paged';

@Injectable()
export class HttpVendorGateway extends VendorGateway {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  override getMyVendor(): Observable<VendorProfile> {
    return this.http.get<VendorProfile>(`${this.baseUrl}/api/v1/vendor/me`);
  }

  override listMyProducts(
    page: number,
    pageSize: number,
  ): Observable<PagedResponse<VendorProduct>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResponse<VendorProduct>>(`${this.baseUrl}/api/v1/vendor/products`, {
      params,
    });
  }

  override createDraft(request: CreateProductDraft): Observable<CreatedProduct> {
    return this.http.post<CreatedProduct>(`${this.baseUrl}/api/v1/vendor/products`, request);
  }

  override submitForReview(productId: string): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/api/v1/vendor/products/${encodeURIComponent(productId)}/submit`,
      {},
    );
  }
}
