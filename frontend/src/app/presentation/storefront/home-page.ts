import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home-page',
  imports: [RouterLink],
  template: `
    <section
      class="border-b border-slate-200 bg-gradient-to-br from-slate-900 via-slate-800 to-emerald-900 text-white"
    >
      <div class="mx-auto grid max-w-6xl gap-12 px-4 py-20 lg:grid-cols-[1.15fr_0.85fr] lg:items-center">
        <div>
          <span
            class="inline-flex items-center rounded-full bg-white/10 px-3 py-1 text-xs font-medium tracking-wide text-emerald-200"
            >Myanmar marketplace · Prices in MMK</span
          >
          <h1 class="mt-5 text-4xl font-bold tracking-tight sm:text-5xl">
            Every local shop,<br class="hidden sm:block" />
            one trusted place.
          </h1>
          <p class="mt-5 max-w-xl text-lg text-slate-300">
            Independent vendors get their own storefront. Every product is reviewed before it goes
            live, and every payment is verified by staff — no card gateway required.
          </p>
          <div class="mt-8 flex flex-wrap gap-3">
            <a
              routerLink="/store/demo-store"
              class="rounded-xl bg-emerald-500 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-emerald-400"
              >Visit the demo store</a
            >
            <a
              routerLink="/login"
              class="rounded-xl border border-white/25 px-5 py-3 text-sm font-semibold text-white hover:bg-white/10"
              >Vendor sign in</a
            >
          </div>
          <p class="mt-4 text-xs text-slate-400">
            Demo accounts: <span class="font-mono text-slate-200">admin / Admin123!</span> ·
            <span class="font-mono text-slate-200">vendor / Vendor123!</span>
          </p>
        </div>

        <div class="rounded-3xl border border-white/15 bg-white/5 p-5 shadow-2xl backdrop-blur">
          <div class="flex items-center justify-between text-xs text-slate-300">
            <span>demo-store</span>
            <span class="rounded-full bg-emerald-400/15 px-2 py-1 text-emerald-200">Live storefront</span>
          </div>
          <div class="mt-4 space-y-3">
            <div class="rounded-2xl bg-white p-4 text-slate-900">
              <div class="flex items-start justify-between gap-3">
                <div>
                  <p class="text-xs font-medium text-slate-500">Electronics</p>
                  <p class="mt-1 font-semibold">Wireless Earbuds</p>
                  <p class="mt-1 text-sm text-slate-500">From 85,000 Ks</p>
                </div>
                <span
                  class="rounded-full bg-emerald-50 px-2 py-1 text-[10px] font-semibold text-emerald-700"
                  >Live</span
                >
              </div>
            </div>
            <div class="rounded-2xl bg-white/10 p-4 text-slate-200">
              <div class="flex items-start justify-between gap-3">
                <div>
                  <p class="text-xs font-medium text-slate-400">Electronics</p>
                  <p class="mt-1 font-semibold">Smart Watch</p>
                  <p class="mt-1 text-sm text-slate-400">From 145,000 Ks</p>
                </div>
                <span
                  class="rounded-full bg-amber-400/15 px-2 py-1 text-[10px] font-semibold text-amber-200"
                  >In review</span
                >
              </div>
            </div>
          </div>
          <p class="mt-4 text-xs text-slate-400">
            Products only appear publicly after an admin approves them.
          </p>
        </div>
      </div>
    </section>

    <section class="mx-auto max-w-6xl px-4 py-16">
      <div class="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h2 class="text-2xl font-bold tracking-tight text-slate-900">Browse by category</h2>
          <p class="mt-1 text-sm text-slate-500">Seeded for the walking skeleton; management lands next.</p>
        </div>
        <a
          routerLink="/store/demo-store"
          class="text-sm font-medium text-emerald-700 hover:text-emerald-900"
          >See everything in the demo store →</a
        >
      </div>

      <div class="mt-6 grid gap-4 sm:grid-cols-3">
        @for (category of categories; track category.name) {
          <div class="rounded-2xl border border-slate-200 bg-white p-6">
            <span class="text-2xl" aria-hidden="true">{{ category.icon }}</span>
            <h3 class="mt-3 font-semibold text-slate-900">{{ category.name }}</h3>
            <p class="mt-1 text-sm text-slate-500">{{ category.description }}</p>
          </div>
        }
      </div>
    </section>

    <section class="border-y border-slate-200 bg-white">
      <div class="mx-auto max-w-6xl px-4 py-16">
        <h2 class="text-2xl font-bold tracking-tight text-slate-900">How the marketplace works</h2>
        <div class="mt-8 grid gap-6 md:grid-cols-3">
          <div class="rounded-2xl border border-slate-200 p-6">
            <span class="text-xs font-semibold text-emerald-700">01 · Vendors</span>
            <h3 class="mt-2 font-semibold text-slate-900">Open a storefront</h3>
            <p class="mt-2 text-sm text-slate-500">
              Each vendor owns a slug-based store URL, manages products, and keeps stock in kg-based
              shipping weights.
            </p>
          </div>
          <div class="rounded-2xl border border-slate-200 p-6">
            <span class="text-xs font-semibold text-emerald-700">02 · Admins</span>
            <h3 class="mt-2 font-semibold text-slate-900">Review every listing</h3>
            <p class="mt-2 text-sm text-slate-500">
              Nothing is public until an admin approves it. Rejections carry a reason back to the
              vendor so they can resubmit.
            </p>
          </div>
          <div class="rounded-2xl border border-slate-200 p-6">
            <span class="text-xs font-semibold text-emerald-700">03 · Payments</span>
            <h3 class="mt-2 font-semibold text-slate-900">Pay the way you already do</h3>
            <p class="mt-2 text-sm text-slate-500">
              Bank transfer, mobile wallet, or cash on delivery — staff verify the proof before an
              order is fulfilled.
            </p>
          </div>
        </div>
      </div>
    </section>

    <section class="mx-auto max-w-6xl px-4 py-16">
      <div
        class="flex flex-col items-start justify-between gap-6 rounded-3xl bg-slate-900 px-8 py-10 text-white sm:flex-row sm:items-center"
      >
        <div>
          <h2 class="text-2xl font-bold tracking-tight">Ready to look around?</h2>
          <p class="mt-2 max-w-xl text-sm text-slate-300">
            The demo store is seeded with approved and in-review products so you can see both sides
            of the workflow.
          </p>
        </div>
        <a
          routerLink="/store/demo-store"
          class="rounded-xl bg-white px-5 py-3 text-sm font-semibold text-slate-900 hover:bg-slate-100"
          >Open demo-store</a
        >
      </div>
    </section>
  `,
})
export class HomePage {
  readonly categories = [
    {
      icon: '🎧',
      name: 'Electronics',
      description: 'Earbuds, watches, accessories and everyday tech.',
    },
    {
      icon: '👕',
      name: 'Fashion',
      description: 'Clothing and accessories from local vendors.',
    },
    {
      icon: '🏠',
      name: 'Home & Living',
      description: 'Useful things for the house, sourced locally.',
    },
  ];
}
