import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ModerationStore } from '@application/admin/moderation.store';
import { PendingProduct } from '@domain/moderation/models';
import { formatDate } from '@domain/shared/dates';

@Component({
  selector: 'app-moderation-page',
  imports: [FormsModule],
  template: `
    @if (store.status() === 'loading' || store.status() === 'idle') {
      <div class="mx-auto max-w-5xl animate-pulse px-4 py-12">
        <div class="h-4 w-32 rounded bg-slate-200"></div>
        <div class="mt-3 h-9 w-72 rounded bg-slate-200"></div>
        <div class="mt-8 space-y-4">
          @for (placeholder of [1, 2]; track placeholder) {
            <div class="h-40 rounded-2xl bg-slate-200"></div>
          }
        </div>
      </div>
    } @else if (store.status() === 'error') {
      <div class="mx-auto max-w-3xl px-4 py-20 text-center">
        <h1 class="text-2xl font-bold text-slate-900">Moderation queue unavailable</h1>
        <p class="mt-2 text-sm text-slate-600">{{ store.error() }}</p>
        <button
          type="button"
          (click)="store.load()"
          class="mt-6 rounded-xl bg-slate-900 px-5 py-2.5 text-sm font-semibold text-white hover:bg-slate-700"
        >
          Try again
        </button>
      </div>
    } @else {
      <div class="mx-auto max-w-5xl px-4 py-10">
        <p class="text-xs font-semibold tracking-wider text-emerald-700 uppercase">Admin</p>
        <div class="mt-1 flex flex-wrap items-end justify-between gap-3">
          <div>
            <h1 class="text-3xl font-bold tracking-tight text-slate-900">Moderation queue</h1>
            <p class="mt-2 text-sm text-slate-600">
              Products stay invisible on the storefront until approved. Rejections send the reason
              back to the vendor.
            </p>
          </div>
          <span class="rounded-full bg-amber-50 px-3 py-1 text-sm font-medium text-amber-700">
            {{ store.pending()?.totalCount ?? 0 }} awaiting review
          </span>
        </div>

        @if (store.actionMessage(); as message) {
          <div
            class="mt-6 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800"
            role="status"
          >
            {{ message }}
          </div>
        }
        @if (store.actionError(); as message) {
          <div
            class="mt-6 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800"
            role="alert"
          >
            {{ message }}
          </div>
        }

        @if (store.pending(); as page) {
          @if (page.items.length === 0) {
            <div
              class="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white px-6 py-16 text-center"
            >
              <p class="text-lg font-semibold text-slate-900">The queue is clear</p>
              <p class="mt-2 text-sm text-slate-500">
                New submissions from vendors will appear here for review.
              </p>
            </div>
          } @else {
            <ul class="mt-8 space-y-4">
              @for (product of page.items; track product.id) {
                <li class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                  <div class="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <h2 class="text-lg font-semibold text-slate-900">{{ product.name }}</h2>
                      <p class="mt-1 text-sm text-slate-500">
                        {{ product.vendorName }} · {{ product.categoryName }} ·
                        {{ product.variantCount }} variant{{ product.variantCount === 1 ? '' : 's' }}
                      </p>
                      <p class="mt-1 text-xs text-slate-400">
                        Submitted {{ formatDate(product.submittedAt) }}
                      </p>
                    </div>

                    <div class="flex flex-wrap gap-2">
                      <button
                        type="button"
                        (click)="store.approve(product)"
                        [disabled]="store.workingId() === product.id"
                        class="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-700 disabled:opacity-50"
                      >
                        Approve
                      </button>
                      <button
                        type="button"
                        (click)="startReject(product)"
                        [disabled]="store.workingId() === product.id"
                        class="rounded-lg border border-rose-300 px-4 py-2 text-sm font-semibold text-rose-700 hover:bg-rose-50 disabled:opacity-50"
                      >
                        Reject
                      </button>
                    </div>
                  </div>

                  @if (rejectingId() === product.id) {
                    <div class="mt-4 rounded-xl border border-rose-200 bg-rose-50/60 p-4">
                      <label class="block">
                        <span class="text-sm font-medium text-rose-900"
                          >Reason shown to the vendor</span
                        >
                        <textarea
                          [ngModel]="rejectReason()"
                          (ngModelChange)="rejectReason.set($event)"
                          rows="2"
                          maxlength="1000"
                          placeholder="Photos are unclear. Please add a white-background image."
                          class="mt-1.5 w-full rounded-lg border border-rose-200 bg-white px-3 py-2 text-sm focus:border-rose-500 focus:ring-2 focus:ring-rose-100 focus:outline-none"
                        ></textarea>
                      </label>
                      <div class="mt-3 flex justify-end gap-2">
                        <button
                          type="button"
                          (click)="cancelReject()"
                          class="rounded-lg border border-slate-300 bg-white px-3 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-50"
                        >
                          Cancel
                        </button>
                        <button
                          type="button"
                          (click)="confirmReject(product)"
                          [disabled]="store.workingId() === product.id"
                          class="rounded-lg bg-rose-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-rose-700 disabled:opacity-50"
                        >
                          Confirm rejection
                        </button>
                      </div>
                    </div>
                  }
                </li>
              }
            </ul>

            @if (page.totalPages > 1) {
              <div class="mt-6 flex items-center justify-center gap-3">
                <button
                  type="button"
                  [disabled]="page.page <= 1"
                  (click)="store.load(page.page - 1)"
                  class="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-40"
                >
                  Previous
                </button>
                <span class="text-sm text-slate-500">{{ page.page }} / {{ page.totalPages }}</span>
                <button
                  type="button"
                  [disabled]="page.page >= page.totalPages"
                  (click)="store.load(page.page + 1)"
                  class="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-40"
                >
                  Next
                </button>
              </div>
            }
          }
        }
      </div>
    }
  `,
})
export class ModerationPage implements OnInit {
  readonly store = inject(ModerationStore);
  readonly formatDate = formatDate;

  readonly rejectingId = signal<string | null>(null);
  readonly rejectReason = signal('');

  ngOnInit(): void {
    void this.store.load();
  }

  startReject(product: PendingProduct): void {
    this.store.clearAction();
    this.rejectReason.set('');
    this.rejectingId.set(product.id);
  }

  cancelReject(): void {
    this.rejectingId.set(null);
    this.rejectReason.set('');
  }

  async confirmReject(product: PendingProduct): Promise<void> {
    const rejected = await this.store.reject(product, this.rejectReason());

    if (rejected) {
      this.cancelReject();
    }
  }
}
