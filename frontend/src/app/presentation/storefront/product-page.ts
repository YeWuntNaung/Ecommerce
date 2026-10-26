import { Component, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CatalogStore } from '@application/catalog/catalog.store';
import { formatKyat, formatWeight } from '@domain/shared/money';

@Component({
  selector: 'app-product-page',
  imports: [RouterLink],
  template: `
    @if (catalog.productStatus() === 'loading') {
      <div class="mx-auto max-w-6xl animate-pulse px-4 py-12">
        <div class="h-4 w-48 rounded bg-slate-200"></div>
        <div class="mt-6 grid gap-10 lg:grid-cols-2">
          <div class="h-72 rounded-2xl bg-slate-200"></div>
          <div class="space-y-4">
            <div class="h-9 w-3/4 rounded bg-slate-200"></div>
            <div class="h-4 w-full rounded bg-slate-200"></div>
            <div class="h-4 w-2/3 rounded bg-slate-200"></div>
            <div class="h-24 rounded-2xl bg-slate-200"></div>
          </div>
        </div>
      </div>
    } @else if (catalog.productStatus() === 'error') {
      <div class="mx-auto max-w-3xl px-4 py-20 text-center">
        <h1 class="text-2xl font-bold text-slate-900">Product unavailable</h1>
        <p class="mt-2 text-sm text-slate-600">{{ catalog.productError() }}</p>
        <a
          routerLink="/"
          class="mt-6 inline-block rounded-xl bg-slate-900 px-5 py-2.5 text-sm font-semibold text-white hover:bg-slate-700"
          >Back to home</a
        >
      </div>
    } @else if (catalog.product(); as product) {
      <div class="mx-auto max-w-6xl px-4 py-10">
        <nav class="text-sm text-slate-500" aria-label="Breadcrumb">
          <a [routerLink]="['/store', product.vendorSlug]" class="hover:text-emerald-700">{{
            product.vendorName
          }}</a>
          <span class="mx-2">/</span>
          <span class="text-slate-700">{{ product.categoryName }}</span>
        </nav>

        <div class="mt-6 grid gap-10 lg:grid-cols-[1fr_0.9fr]">
          <div
            class="flex min-h-72 items-center justify-center rounded-3xl border border-slate-200 bg-gradient-to-br from-slate-100 to-emerald-50 p-10"
          >
            <div class="text-center">
              <span class="text-6xl" aria-hidden="true">🛍️</span>
              <p class="mt-4 text-sm font-medium text-slate-500">
                Product photography arrives with the vendor media pipeline.
              </p>
            </div>
          </div>

          <div>
            <span
              class="inline-flex rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-600"
              >{{ product.categoryName }}</span
            >
            <h1 class="mt-3 text-3xl font-bold tracking-tight text-slate-900">{{ product.name }}</h1>
            <p class="mt-3 text-slate-600">{{ product.description }}</p>

            <div class="mt-6 rounded-2xl border border-slate-200 bg-white">
              <div class="border-b border-slate-100 px-5 py-3 text-sm font-semibold text-slate-900">
                Available variants
              </div>
              <ul class="divide-y divide-slate-100">
                @for (variant of product.variants; track variant.id) {
                  <li class="flex flex-wrap items-center justify-between gap-3 px-5 py-4">
                    <div>
                      <p class="font-medium text-slate-900">{{ variant.name }}</p>
                      <p class="mt-0.5 text-xs text-slate-500">
                        SKU {{ variant.sku }} · {{ formatWeight(variant.weightGrams) }} ·
                        {{ variant.stockOnHand }} in stock
                      </p>
                    </div>
                    <div class="text-right">
                      <p class="font-semibold text-slate-900">{{ formatKyat(variant.priceKyat) }}</p>
                      <p
                        class="mt-0.5 text-xs"
                        [class]="
                          variant.stockOnHand > 0 ? 'text-emerald-700' : 'text-rose-600'
                        "
                      >
                        {{ variant.stockOnHand > 0 ? 'Available' : 'Out of stock' }}
                      </p>
                    </div>
                  </li>
                }
              </ul>
            </div>

            <div class="mt-5 rounded-xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-900">
              <strong class="font-semibold">Ordering opens in the next phase.</strong>
              Cart, weighted shipping, and manual payment verification are the next slice of this
              marketplace.
            </div>

            <a
              [routerLink]="['/store', product.vendorSlug]"
              class="mt-6 inline-flex items-center gap-1 text-sm font-medium text-emerald-700 hover:text-emerald-900"
              >← Back to {{ product.vendorName }}</a
            >
          </div>
        </div>
      </div>
    }
  `,
})
export class ProductPage {
  readonly catalog = inject(CatalogStore);
  readonly formatKyat = formatKyat;
  readonly formatWeight = formatWeight;
  private readonly route = inject(ActivatedRoute);

  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed()).subscribe((params) => {
      const vendorSlug = params.get('slug');
      const productSlug = params.get('productSlug');

      if (vendorSlug && productSlug) {
        void this.catalog.loadProduct(vendorSlug, productSlug);
      }
    });
  }
}
