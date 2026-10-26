import { Provider } from '@angular/core';
import { CatalogGateway } from '@domain/catalog/ports/catalog.gateway';
import { AuthGateway } from '@domain/identity/ports/auth.gateway';
import { ModerationGateway } from '@domain/moderation/ports/moderation.gateway';
import { VendorGateway } from '@domain/vendor/ports/vendor.gateway';
import { HttpAuthGateway } from '@infrastructure/api/http-auth.gateway';
import { HttpCatalogGateway } from '@infrastructure/api/http-catalog.gateway';
import { HttpModerationGateway } from '@infrastructure/api/http-moderation.gateway';
import { HttpVendorGateway } from '@infrastructure/api/http-vendor.gateway';

/**
 * Binds domain ports to HTTP adapters. This is the single seam where the
 * application layer learns which transport is used; swapping REST for
 * something else means changing only this list.
 */
export const infrastructureProviders: Provider[] = [
  { provide: AuthGateway, useClass: HttpAuthGateway },
  { provide: CatalogGateway, useClass: HttpCatalogGateway },
  { provide: VendorGateway, useClass: HttpVendorGateway },
  { provide: ModerationGateway, useClass: HttpModerationGateway },
];
