import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '@core/config/api-base-url';
import { ModerationGateway } from '@domain/moderation/ports/moderation.gateway';
import { PendingProduct } from '@domain/moderation/models';
import { PagedResponse } from '@domain/shared/paged';

@Injectable()
export class HttpModerationGateway extends ModerationGateway {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  override listPending(
    page: number,
    pageSize: number,
  ): Observable<PagedResponse<PendingProduct>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResponse<PendingProduct>>(
      `${this.baseUrl}/api/v1/admin/products/pending`,
      { params },
    );
  }

  override approve(productId: string): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/api/v1/admin/products/${encodeURIComponent(productId)}/approve`,
      {},
    );
  }

  override reject(productId: string, reason: string): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/api/v1/admin/products/${encodeURIComponent(productId)}/reject`,
      { reason },
    );
  }
}
