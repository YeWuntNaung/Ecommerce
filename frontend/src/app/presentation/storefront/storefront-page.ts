import { Component, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { CatalogStore } from '@application/catalog/catalog.store';
import { ProductCard } from '@presentation/shared/product-card';

@Component({
  selector: 'app-storefront-page',
  imports: [ProductCard],
  template: `
    @if (catalog.status() === 'loading') {
      <div class="mx-auto max-w-6xl animate-pulse px-4 py-12">
        <div class="h-4 w-28 rounded bg-slate-200"></div>
        <div class="mt-4 h-9 w-64 rounded bg-slate-200"></div>
        <div class="mt-3 h-4 w-full max-w-xl rounded bg-slate-200"></div>
        <div class="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          @for (placeholder of [1, 2, 3]; track placeholder) {
            <div class="h-48 rounded-2xl border border-slate-200 bg-white"></div>
          }
        </div>
      </div>
    } @else if (catalog.status() === 'error') {
      <div class="mx-auto max-w-3xl px-4 py-20 text-center">
        <h1 class="text-2xl font-bold text-slate-900">Storefront unavailable</h1>
        <p class="mt-2 text-sm text-slate-600">{{ catalog.error() }}</p>
        <button
          type="button"
          (click)="retry()"
          class="mt-6 rounded-xl bg-slate-900 px-5 py-2.5 text-sm font-semibold text-white hover:bg-slate-700"
        >
          Try again
        </button>
      </div>
    } @else if (catalog.storefront(); as store) {
      <section class="border-b border-slate-200 bg-white">
        <div class="mx-auto max-w-6xl px-4 py-10">
          <p class="text-xs font-semibold tracking-wider text-emerald-700 uppercase">Storefront</p>
          <h1 class="mt-2 text-3xl font-bold tracking-tight text-slate-900 sm:text-4xl">
            {{ store.name }}
          </h1>
          <p class="mt-3 max-w-2xl text-slate-600">{{ store.description }}</p>
          <p class="mt-4 text-sm text-slate-500">
            Every listing is reviewed by the marketplace team before it appears here.
          </p>
        </div>
      </section>

      <div class="mx-auto max-w-6xl px-4 py-10">
        @if (catalog.products(); as page) {
          @if (page.items.length === 0) {
            <div class="rounded-2xl border border-dashed border-slate-300 bg-white px-6 py-16 text-center">
              <h2 class="text-lg font-semibold text-slate-900">Nothing on the shelves yet</h2>
              <p class="mt-2 text-sm text-slate-500">
                This vendor has no approved products. Check back after the next review.
              </p>
            </div>
          } @else {
            <div class="flex flex-wrap items-end justify-between gap-3">
              <h2 class="text-lg font-semibold text-slate-900">
                {{ page.totalCount }} product{{ page.totalCount === 1 ? '' : 's' }}
              </h2>
              <p class="text-sm text-slate-500">Page {{ page.page }} of {{ page.totalPages }}</p>
            </div>

            <div class="mt-5 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
              @for (product of page.items; track product.id) {
                <app-product-card [product]="product" />
              }
            </div>

            @if (page.totalPages > 1) {
              <nav class="mt-8 flex items-center justify-center gap-3" aria-label="Pagination">
                <button
                  type="button"
                  [disabled]="page.page <= 1"
                  (click)="goToPage(page.page - 1)"
                  class="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40"
                >
                  Previous
                </button>
                <span class="text-sm text-slate-500">{{ page.page }} / {{ page.totalPages }}</span>
                <button
                  type="button"
                  [disabled]="page.page >= page.totalPages"
                  (click)="goToPage(page.page + 1)"
                  class="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40"
                >
                  Next
                </button>
              </nav>
            }
          }
        }
      </div>
    }
  `,
})
export class StorefrontPage {
  readonly catalog = inject(CatalogStore);
  private readonly route = inject(ActivatedRoute);

  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed()).subscribe((params) => {
      const slug = params.get('slug');
      if (slug) {
        void this.catalog.loadStorefront(slug);
      }
    });
  }

  retry(): void {
    const slug = this.catalog.slug();
    if (slug) {
      void this.catalog.loadStorefront(slug, this.catalog.products()?.page ?? 1);
    }
  }

  goToPage(page: number): void {
    const slug = this.catalog.slug();
    if (slug) {
      void this.catalog.loadStorefront(slug, page);
    }
  }
}
