import { Component, computed, input } from '@angular/core';

const LABELS: Record<string, string> = {
  Draft: 'Draft',
  PendingReview: 'In review',
  Approved: 'Live',
  Rejected: 'Rejected',
  Suspended: 'Suspended',
  Pending: 'Awaiting approval',
  Active: 'Active',
};

@Component({
  selector: 'app-status-pill',
  template: `<span
    class="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium whitespace-nowrap"
    [class]="classes()"
    >{{ label() }}</span
  >`,
})
export class StatusPill {
  readonly status = input.required<string>();

  readonly label = computed(() => LABELS[this.status()] ?? this.status());

  readonly classes = computed(() => {
    switch (this.status()) {
      case 'Approved':
      case 'Active':
        return 'bg-emerald-50 text-emerald-700 ring-1 ring-emerald-200';
      case 'PendingReview':
      case 'Pending':
        return 'bg-amber-50 text-amber-700 ring-1 ring-amber-200';
      case 'Rejected':
      case 'Suspended':
        return 'bg-rose-50 text-rose-700 ring-1 ring-rose-200';
      default:
        return 'bg-slate-100 text-slate-600 ring-1 ring-slate-200';
    }
  });
}
