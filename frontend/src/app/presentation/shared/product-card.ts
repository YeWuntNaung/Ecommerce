import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductSummary } from '@domain/catalog/models';
import { formatKyat } from '@domain/shared/money';

@Component({
  selector: 'app-product-card',
  imports: [RouterLink],
  template: `
    <article
      class="group flex h-full flex-col rounded-2xl border border-slate-200 bg-white p-5 shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
    >
      <div class="flex items-center justify-between gap-2 text-xs">
        <span class="rounded-full bg-slate-100 px-2.5 py-1 font-medium text-slate-600">{{
          product().categoryName
        }}</span>
        <span class="truncate text-slate-500">{{ product().vendorName }}</span>
      </div>

      <h2 class="mt-4 text-lg leading-snug font-semibold text-slate-900">
        <a
          [routerLink]="['/store', product().vendorSlug, 'product', product().slug]"
          class="group-hover:text-emerald-700 focus:outline-none"
          >{{ product().name }}</a
        >
      </h2>

      <p class="mt-2 text-sm text-slate-500">
        From <span class="font-semibold text-slate-900">{{ price() }}</span>
      </p>

      <div class="mt-auto pt-5">
        <a
          [routerLink]="['/store', product().vendorSlug, 'product', product().slug]"
          class="inline-flex items-center gap-1 text-sm font-medium text-emerald-700 hover:text-emerald-900"
        >
          View product <span aria-hidden="true">→</span>
        </a>
      </div>
    </article>
  `,
})
export class ProductCard {
  readonly product = input.required<ProductSummary>();

  readonly price = computed(() => formatKyat(this.product().priceFromKyat));
}
