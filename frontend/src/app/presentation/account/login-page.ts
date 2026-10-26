import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthStore } from '@application/auth/auth.store';

@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule],
  template: `
    <div class="mx-auto grid max-w-6xl gap-12 px-4 py-16 lg:grid-cols-2 lg:items-start">
      <div>
        <p class="text-xs font-semibold tracking-wider text-emerald-700 uppercase">Accounts</p>
        <h1 class="mt-2 text-3xl font-bold tracking-tight text-slate-900">Sign in to continue</h1>
        <p class="mt-3 max-w-md text-slate-600">
          Usernames and passwords only. Customers browse without an account; vendors manage their
          storefront and admins review listings.
        </p>

        <div class="mt-8 rounded-2xl border border-slate-200 bg-white p-5">
          <h2 class="text-sm font-semibold text-slate-900">Seeded demo accounts</h2>
          <div class="mt-3 space-y-3 text-sm">
            <div class="flex flex-wrap items-center justify-between gap-2">
              <div>
                <p class="font-medium text-slate-800">Admin</p>
                <p class="font-mono text-xs text-slate-500">admin / Admin123!</p>
              </div>
              <button
                type="button"
                (click)="fill('admin', 'Admin123!')"
                class="rounded-lg border border-slate-300 px-3 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-50"
              >
                Use admin
              </button>
            </div>
            <div class="flex flex-wrap items-center justify-between gap-2">
              <div>
                <p class="font-medium text-slate-800">Vendor</p>
                <p class="font-mono text-xs text-slate-500">vendor / Vendor123!</p>
              </div>
              <button
                type="button"
                (click)="fill('vendor', 'Vendor123!')"
                class="rounded-lg border border-slate-300 px-3 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-50"
              >
                Use vendor
              </button>
            </div>
          </div>
          <p class="mt-4 text-xs text-slate-500">
            The admin lands in the moderation queue; the vendor lands in their storefront workspace.
          </p>
        </div>
      </div>

      <form
        [formGroup]="form"
        (ngSubmit)="submit()"
        class="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm"
      >
        <h2 class="text-lg font-semibold text-slate-900">Username and password</h2>
        <p class="mt-1 text-sm text-slate-500">
          Sessions use a secure, HTTP-only cookie — no tokens stored in the browser.
        </p>

        <label class="mt-6 block">
          <span class="text-sm font-medium text-slate-700">Username</span>
          <input
            type="text"
            formControlName="username"
            autocomplete="username"
            class="mt-1.5 w-full rounded-xl border border-slate-300 px-3 py-2.5 text-sm text-slate-900 focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100 focus:outline-none"
            placeholder="vendor"
          />
          @if (form.controls.username.touched && form.controls.username.invalid) {
            <span class="mt-1 block text-xs text-rose-600">Enter your username.</span>
          }
        </label>

        <label class="mt-4 block">
          <span class="text-sm font-medium text-slate-700">Password</span>
          <input
            type="password"
            formControlName="password"
            autocomplete="current-password"
            class="mt-1.5 w-full rounded-xl border border-slate-300 px-3 py-2.5 text-sm text-slate-900 focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100 focus:outline-none"
            placeholder="••••••••"
          />
          @if (form.controls.password.touched && form.controls.password.invalid) {
            <span class="mt-1 block text-xs text-rose-600">Enter your password.</span>
          }
        </label>

        @if (auth.error(); as error) {
          <p
            class="mt-4 rounded-xl border border-rose-200 bg-rose-50 px-3 py-2.5 text-sm text-rose-700"
            role="alert"
          >
            {{ error }}
          </p>
        }

        <button
          type="submit"
          [disabled]="auth.pending() || form.invalid"
          class="mt-6 w-full rounded-xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white hover:bg-slate-700 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {{ auth.pending() ? 'Signing in…' : 'Sign in' }}
        </button>

        <p class="mt-4 text-center text-xs text-slate-500">
          New vendors are onboarded by the marketplace team.
        </p>
      </form>
    </div>
  `,
})
export class LoginPage {
  readonly auth = inject(AuthStore);

  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  readonly form = this.fb.nonNullable.group({
    username: ['', Validators.required],
    password: ['', Validators.required],
  });

  fill(username: string, password: string): void {
    this.auth.clearError();
    this.form.setValue({ username, password });
  }

  async submit(): Promise<void> {
    if (this.form.invalid || this.auth.pending()) {
      this.form.markAllAsTouched();
      return;
    }

    const { username, password } = this.form.getRawValue();
    const signedIn = await this.auth.login(username.trim(), password);

    if (!signedIn) {
      return;
    }

    const returnUrl = this.router.parseUrl(this.router.url).queryParams['returnUrl'];

    if (typeof returnUrl === 'string' && returnUrl.startsWith('/')) {
      await this.router.navigateByUrl(returnUrl);
      return;
    }

    if (this.auth.isAdmin()) {
      await this.router.navigate(['/admin/moderation']);
    } else if (this.auth.isVendorOwner()) {
      await this.router.navigate(['/vendor']);
    } else {
      await this.router.navigate(['/']);
    }
  }
}
