import { Observable } from 'rxjs';
import { PagedResponse } from '../../shared/paged';
import { PendingProduct } from '../models';

export abstract class ModerationGateway {
  abstract listPending(page: number, pageSize: number): Observable<PagedResponse<PendingProduct>>;

  abstract approve(productId: string): Observable<void>;

  abstract reject(productId: string, reason: string): Observable<void>;
}
