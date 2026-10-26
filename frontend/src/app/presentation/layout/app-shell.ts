import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthStore } from '@application/auth/auth.store';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="flex min-h-screen flex-col">
      <header class="sticky top-0 z-30 border-b border-slate-200 bg-white/90 backdrop-blur">
        <div class="mx-auto flex h-16 max-w-6xl items-center gap-6 px-4">
          <a routerLink="/" class="flex items-center gap-2 font-semibold tracking-tight text-slate-900">
            <span
              class="grid h-9 w-9 place-items-center rounded-xl bg-slate-900 text-sm font-bold text-white"
              >MM</span
            >
            Bazaar MM
          </a>

          <nav class="hidden items-center gap-1 text-sm font-medium text-slate-600 sm:flex">
            <a
              routerLink="/"
              routerLinkActive="bg-slate-100 text-slate-900"
              [routerLinkActiveOptions]="{ exact: true }"
              class="rounded-lg px-3 py-2 hover:bg-slate-100"
              >Home</a
            >
            <a
              routerLink="/store/demo-store"
              routerLinkActive="bg-slate-100 text-slate-900"
              class="rounded-lg px-3 py-2 hover:bg-slate-100"
              >Demo store</a
            >
            @if (auth.isVendorOwner()) {
              <a
                routerLink="/vendor"
                routerLinkActive="bg-slate-100 text-slate-900"
                class="rounded-lg px-3 py-2 hover:bg-slate-100"
                >Vendor</a
              >
            }
            @if (auth.isAdmin()) {
              <a
                routerLink="/admin/moderation"
                routerLinkActive="bg-slate-100 text-slate-900"
                class="rounded-lg px-3 py-2 hover:bg-slate-100"
                >Moderation</a
              >
            }
          </nav>

          <div class="ml-auto flex items-center gap-3 text-sm">
            @if (auth.isAuthenticated()) {
              <span class="hidden text-slate-500 sm:inline">
                Signed in as <strong class="text-slate-800">{{ auth.user()?.username }}</strong>
              </span>
              <button
                type="button"
                (click)="signOut()"
                class="rounded-lg border border-slate-300 px-3 py-1.5 font-medium text-slate-700 hover:bg-slate-100"
              >
                Sign out
              </button>
            } @else {
              <a
                routerLink="/login"
                class="rounded-lg bg-slate-900 px-4 py-2 font-medium text-white hover:bg-slate-700"
                >Sign in</a
              >
            }
          </div>
        </div>
      </header>

      <main class="flex-1">
        <router-outlet />
      </main>

      <footer class="border-t border-slate-200 bg-white">
        <div
          class="mx-auto flex max-w-6xl flex-col gap-1 px-4 py-6 text-xs text-slate-500 sm:flex-row sm:items-center sm:justify-between"
        >
          <span>Bazaar MM — a multi-vendor marketplace for Myanmar.</span>
          <span>Prices in MMK · Manual payments verified by staff</span>
        </div>
      </footer>
    </div>
  `,
})
export class AppShell {
  readonly auth = inject(AuthStore);
  private readonly router = inject(Router);

  async signOut(): Promise<void> {
    await this.auth.logout();
    await this.router.navigate(['/']);
  }
}
