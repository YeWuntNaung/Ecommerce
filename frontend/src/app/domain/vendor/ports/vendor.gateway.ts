import { Observable } from 'rxjs';
import { PagedResponse } from '../../shared/paged';
import { CreatedProduct, CreateProductDraft, VendorProduct, VendorProfile } from '../models';

export abstract class VendorGateway {
  abstract getMyVendor(): Observable<VendorProfile>;

  abstract listMyProducts(page: number, pageSize: number): Observable<PagedResponse<VendorProduct>>;

  abstract createDraft(request: CreateProductDraft): Observable<CreatedProduct>;

  abstract submitForReview(productId: string): Observable<void>;
}
