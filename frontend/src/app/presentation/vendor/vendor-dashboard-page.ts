import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { VendorStore } from '@application/vendor/vendor.store';
import { formatDate } from '@domain/shared/dates';
import { formatKyat } from '@domain/shared/money';
import { StatusPill } from '@presentation/shared/status-pill';

interface DraftVariantForm {
  name: string;
  sku: string;
  priceKyat: number | null;
  weightKg: number | null;
  stockOnHand: number | null;
}

interface DraftProductForm {
  name: string;
  description: string;
  categoryId: string;
  variants: DraftVariantForm[];
}

function emptyVariant(): DraftVariantForm {
  return { name: '', sku: '', priceKyat: null, weightKg: null, stockOnHand: null };
}

@Component({
  selector: 'app-vendor-dashboard-page',
  imports: [FormsModule, RouterLink, StatusPill],
  template: `
    @if (vendor.status() === 'loading' || vendor.status() === 'idle') {
      <div class="mx-auto max-w-6xl animate-pulse px-4 py-12">
        <div class="h-4 w-32 rounded bg-slate-200"></div>
        <div class="mt-3 h-9 w-64 rounded bg-slate-200"></div>
        <div class="mt-8 grid gap-6 lg:grid-cols-[1fr_1.2fr]">
          <div class="h-96 rounded-2xl bg-slate-200"></div>
          <div class="h-96 rounded-2xl bg-slate-200"></div>
        </div>
      </div>
    } @else if (vendor.status() === 'error') {
      <div class="mx-auto max-w-3xl px-4 py-20 text-center">
        <h1 class="text-2xl font-bold text-slate-900">Vendor workspace unavailable</h1>
        <p class="mt-2 text-sm text-slate-600">{{ vendor.error() }}</p>
        <button
          type="button"
          (click)="vendor.load()"
          class="mt-6 rounded-xl bg-slate-900 px-5 py-2.5 text-sm font-semibold text-white hover:bg-slate-700"
        >
          Try again
        </button>
      </div>
    } @else {
      <div class="mx-auto max-w-6xl px-4 py-10">
        <div class="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p class="text-xs font-semibold tracking-wider text-emerald-700 uppercase">
              Vendor workspace
            </p>
            <h1 class="mt-1 text-3xl font-bold tracking-tight text-slate-900">
              {{ vendor.profile()?.name ?? 'Your store' }}
            </h1>
            @if (vendor.profile(); as profile) {
              <p class="mt-2 text-sm text-slate-600">
                Public storefront:
                <a
                  [routerLink]="['/store', profile.slug]"
                  class="font-medium text-emerald-700 hover:underline"
                  >/store/{{ profile.slug }}</a
                >
              </p>
            }
          </div>
          @if (vendor.profile(); as profile) {
            <app-status-pill [status]="profile.status" />
          }
        </div>

        @if (vendor.actionMessage(); as message) {
          <div
            class="mt-6 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800"
            role="status"
          >
            {{ message }}
          </div>
        }
        @if (vendor.actionError(); as message) {
          <div
            class="mt-6 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800"
            role="alert"
          >
            {{ message }}
          </div>
        }

        <div class="mt-8 grid gap-6 lg:grid-cols-[0.95fr_1.05fr] lg:items-start">
          <form
            (ngSubmit)="save()"
            class="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm"
          >
            <h2 class="text-lg font-semibold text-slate-900">Add a product</h2>
            <p class="mt-1 text-sm text-slate-500">
              Drafts stay private until you submit them and an admin approves them.
            </p>

            <label class="mt-5 block">
              <span class="text-sm font-medium text-slate-700">Product name</span>
              <input
                type="text"
                name="productName"
                [(ngModel)]="draft.name"
                maxlength="200"
                class="mt-1.5 w-full rounded-xl border border-slate-300 px-3 py-2.5 text-sm focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100 focus:outline-none"
                placeholder="Wireless Earbuds"
              />
            </label>

            <label class="mt-4 block">
              <span class="text-sm font-medium text-slate-700">Category</span>
              <select
                name="categoryId"
                [(ngModel)]="draft.categoryId"
                class="mt-1.5 w-full rounded-xl border border-slate-300 bg-white px-3 py-2.5 text-sm focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100 focus:outline-none"
              >
                <option value="" disabled>Choose a category</option>
                @for (category of vendor.categories(); track category.id) {
                  <option [value]="category.id">{{ category.name }}</option>
                }
              </select>
            </label>

            <label class="mt-4 block">
              <span class="text-sm font-medium text-slate-700">Description</span>
              <textarea
                name="description"
                [(ngModel)]="draft.description"
                rows="3"
                maxlength="4000"
                class="mt-1.5 w-full rounded-xl border border-slate-300 px-3 py-2.5 text-sm focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100 focus:outline-none"
                placeholder="What makes this product useful?"
              ></textarea>
            </label>

            <div class="mt-6 flex items-center justify-between">
              <h3 class="text-sm font-semibold text-slate-900">Variants</h3>
              <button
                type="button"
                (click)="addVariant()"
                class="rounded-lg border border-slate-300 px-3 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-50"
              >
                Add variant
              </button>
            </div>

            <div class="mt-3 space-y-4">
              @for (variant of draft.variants; track $index; let index = $index) {
                <fieldset class="rounded-xl border border-slate-200 bg-slate-50/60 p-4">
                  <legend class="px-1 text-xs font-semibold text-slate-500">
                    Variant {{ index + 1 }}
                  </legend>

                  <div class="grid gap-3 sm:grid-cols-2">
                    <label class="block">
                      <span class="text-xs font-medium text-slate-600">Name</span>
                      <input
                        type="text"
                        [name]="'variantName' + index"
                        [(ngModel)]="variant.name"
                        maxlength="120"
                        class="mt-1 w-full rounded-lg border border-slate-300 bg-white px-2.5 py-2 text-sm focus:border-emerald-600 focus:outline-none"
                        placeholder="Black"
                      />
                    </label>
                    <label class="block">
                      <span class="text-xs font-medium text-slate-600">SKU</span>
                      <input
                        type="text"
                        [name]="'variantSku' + index"
                        [(ngModel)]="variant.sku"
                        maxlength="64"
                        class="mt-1 w-full rounded-lg border border-slate-300 bg-white px-2.5 py-2 text-sm uppercase focus:border-emerald-600 focus:outline-none"
                        placeholder="EAR-BLK"
                      />
                    </label>
                    <label class="block">
                      <span class="text-xs font-medium text-slate-600">Price (kyat)</span>
                      <input
                        type="number"
                        [name]="'variantPrice' + index"
                        [(ngModel)]="variant.priceKyat"
                        min="1"
                        step="1"
                        class="mt-1 w-full rounded-lg border border-slate-300 bg-white px-2.5 py-2 text-sm focus:border-emerald-600 focus:outline-none"
                        placeholder="85000"
                      />
                    </label>
                    <label class="block">
                      <span class="text-xs font-medium text-slate-600">Weight (kg)</span>
                      <input
                        type="number"
                        [name]="'variantWeight' + index"
                        [(ngModel)]="variant.weightKg"
                        min="0.001"
                        step="0.001"
                        class="mt-1 w-full rounded-lg border border-slate-300 bg-white px-2.5 py-2 text-sm focus:border-emerald-600 focus:outline-none"
                        placeholder="0.200"
                      />
                    </label>
                    <label class="block">
                      <span class="text-xs font-medium text-slate-600">Stock on hand</span>
                      <input
                        type="number"
                        [name]="'variantStock' + index"
                        [(ngModel)]="variant.stockOnHand"
                        min="0"
                        step="1"
                        class="mt-1 w-full rounded-lg border border-slate-300 bg-white px-2.5 py-2 text-sm focus:border-emerald-600 focus:outline-none"
                        placeholder="25"
                      />
                    </label>
                    @if (draft.variants.length > 1) {
                      <div class="flex items-end">
                        <button
                          type="button"
                          (click)="removeVariant(index)"
                          class="w-full rounded-lg border border-rose-200 px-3 py-2 text-xs font-medium text-rose-700 hover:bg-rose-50"
                        >
                          Remove variant
                        </button>
                      </div>
                    }
                  </div>
                </fieldset>
              }
            </div>

            <label class="mt-5 flex items-start gap-2 text-sm text-slate-700">
              <input
                type="checkbox"
                name="submitForReview"
                [ngModel]="submitForReview()"
                (ngModelChange)="submitForReview.set($event)"
                class="mt-0.5 h-4 w-4 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500"
              />
              Submit for admin review immediately
            </label>

            @if (formError(); as message) {
              <p
                class="mt-4 rounded-xl border border-rose-200 bg-rose-50 px-3 py-2.5 text-sm text-rose-700"
                role="alert"
              >
                {{ message }}
              </p>
            }

            <button
              type="submit"
              [disabled]="vendor.saving()"
              class="mt-5 w-full rounded-xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white hover:bg-slate-700 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {{ vendor.saving() ? 'Saving…' : submitForReview() ? 'Save and submit' : 'Save draft' }}
            </button>
          </form>

          <section class="overflow-hidden rounded-2xl border border-slate-200 bg-white">
            <div class="flex items-center justify-between border-b border-slate-200 px-5 py-4">
              <h2 class="text-lg font-semibold text-slate-900">Your products</h2>
              <span class="text-sm text-slate-500">
                {{ vendor.products()?.totalCount ?? 0 }} total
              </span>
            </div>

            @if (vendor.products(); as page) {
              @if (page.items.length === 0) {
                <div class="px-6 py-16 text-center">
                  <p class="font-medium text-slate-900">No products yet</p>
                  <p class="mt-1 text-sm text-slate-500">
                    Use the form to create your first draft.
                  </p>
                </div>
              } @else {
                <ul class="divide-y divide-slate-100">
                  @for (product of page.items; track product.id) {
                    <li class="px-5 py-4">
                      <div class="flex flex-wrap items-start justify-between gap-3">
                        <div class="min-w-0 flex-1">
                          <div class="flex flex-wrap items-center gap-2">
                            <p class="truncate font-medium text-slate-900">{{ product.name }}</p>
                            <app-status-pill [status]="product.status" />
                          </div>
                          <p class="mt-1 text-xs text-slate-500">
                            {{ product.variantCount }} variant{{ product.variantCount === 1 ? '' : 's' }}
                            · updated {{ formatDate(product.updatedAt) }}
                          </p>
                          @if (product.rejectionReason; as reason) {
                            <p class="mt-2 rounded-lg bg-rose-50 px-3 py-2 text-xs text-rose-700">
                              Reviewer: {{ reason }}
                            </p>
                          }
                        </div>

                        @if (canSubmit(product.status)) {
                          <button
                            type="button"
                            (click)="vendor.submitForReview(product.id, product.name)"
                            [disabled]="vendor.saving()"
                            class="rounded-lg border border-slate-300 px-3 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-50"
                          >
                            Submit for review
                          </button>
                        }
                      </div>
                    </li>
                  }
                </ul>

                @if (page.totalPages > 1) {
                  <div class="flex items-center justify-between border-t border-slate-200 px-5 py-3">
                    <button
                      type="button"
                      [disabled]="page.page <= 1"
                      (click)="vendor.load(page.page - 1)"
                      class="rounded-lg border border-slate-300 px-3 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-40"
                    >
                      Previous
                    </button>
                    <span class="text-xs text-slate-500">{{ page.page }} / {{ page.totalPages }}</span>
                    <button
                      type="button"
                      [disabled]="page.page >= page.totalPages"
                      (click)="vendor.load(page.page + 1)"
                      class="rounded-lg border border-slate-300 px-3 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-40"
                    >
                      Next
                    </button>
                  </div>
                }
              }
            }
          </section>
        </div>

        <p class="mt-6 text-xs text-slate-500">
          Prices are whole kyat ({{ formatKyat(85000) }} style). Shipping weights are entered in
          kilograms and stored as grams.
        </p>
      </div>
    }
  `,
})
export class VendorDashboardPage implements OnInit {
  readonly vendor = inject(VendorStore);
  readonly formatDate = formatDate;
  readonly formatKyat = formatKyat;

  readonly formError = signal<string | null>(null);
  readonly submitForReview = signal(true);

  draft: DraftProductForm = {
    name: '',
    description: '',
    categoryId: '',
    variants: [emptyVariant()],
  };

  constructor() {
    effect(() => {
      const categories = this.vendor.categories();
      if (categories.length > 0 && !this.draft.categoryId) {
        this.draft.categoryId = categories[0].id;
      }
    });
  }

  ngOnInit(): void {
    void this.vendor.load();
  }

  addVariant(): void {
    this.draft.variants.push(emptyVariant());
  }

  removeVariant(index: number): void {
    if (this.draft.variants.length > 1) {
      this.draft.variants.splice(index, 1);
    }
  }

  canSubmit(status: string): boolean {
    return status === 'Draft' || status === 'Rejected';
  }

  async save(): Promise<void> {
    this.formError.set(null);

    const name = this.draft.name.trim();

    if (name.length < 3) {
      this.formError.set('Product name must be at least 3 characters.');
      return;
    }

    if (!this.draft.categoryId) {
      this.formError.set('Choose a category.');
      return;
    }

    if (this.draft.description.length > 4000) {
      this.formError.set('Description must be at most 4000 characters.');
      return;
    }

    const variants = [];

    for (const variant of this.draft.variants) {
      const variantName = variant.name.trim();
      const sku = variant.sku.trim();
      const priceKyat = Number(variant.priceKyat);
      const weightKg = Number(variant.weightKg);
      const stockOnHand = Number(variant.stockOnHand);

      if (variantName.length === 0) {
        this.formError.set('Every variant needs a name.');
        return;
      }

      if (sku.length === 0) {
        this.formError.set('Every variant needs a SKU.');
        return;
      }

      if (!Number.isFinite(priceKyat) || priceKyat <= 0) {
        this.formError.set('Every variant needs a price greater than zero kyat.');
        return;
      }

      if (!Number.isFinite(weightKg) || weightKg <= 0) {
        this.formError.set('Every variant needs a weight greater than zero kilograms.');
        return;
      }

      if (!Number.isFinite(stockOnHand) || stockOnHand < 0) {
        this.formError.set('Stock cannot be negative.');
        return;
      }

      variants.push({
        name: variantName,
        sku,
        priceKyat: Math.round(priceKyat),
        weightGrams: Math.max(1, Math.round(weightKg * 1000)),
        stockOnHand: Math.round(stockOnHand),
      });
    }

    const created = await this.vendor.createDraft(
      {
        categoryId: this.draft.categoryId,
        name,
        description: this.draft.description.trim(),
        variants,
      },
      this.submitForReview(),
    );

    if (created) {
      this.resetForm();
    }
  }

  private resetForm(): void {
    this.draft = {
      name: '',
      description: '',
      categoryId: this.vendor.categories()[0]?.id ?? '',
      variants: [emptyVariant()],
    };
  }
}
