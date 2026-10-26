import { Routes } from '@angular/router';
import { requireRoles } from '@core/auth/auth.guard';

export const routes: Routes = [
  {
    path: '',
    title: 'Bazaar MM — Multi-vendor marketplace',
    loadComponent: () =>
      import('@presentation/storefront/home-page').then((module) => module.HomePage),
  },
  {
    path: 'store/:slug',
    title: 'Storefront — Bazaar MM',
    loadComponent: () =>
      import('@presentation/storefront/storefront-page').then(
        (module) => module.StorefrontPage,
      ),
  },
  {
    path: 'store/:slug/product/:productSlug',
    title: 'Product — Bazaar MM',
    loadComponent: () =>
      import('@presentation/storefront/product-page').then((module) => module.ProductPage),
  },
  {
    path: 'login',
    title: 'Sign in — Bazaar MM',
    loadComponent: () =>
      import('@presentation/account/login-page').then((module) => module.LoginPage),
  },
  {
    path: 'vendor',
    title: 'Vendor workspace — Bazaar MM',
    canActivate: [requireRoles(['VendorOwner'])],
    loadComponent: () =>
      import('@presentation/vendor/vendor-dashboard-page').then(
        (module) => module.VendorDashboardPage,
      ),
  },
  {
    path: 'admin/moderation',
    title: 'Moderation — Bazaar MM',
    canActivate: [requireRoles(['Admin'])],
    loadComponent: () =>
      import('@presentation/admin/moderation-page').then((module) => module.ModerationPage),
  },
  { path: '**', redirectTo: '' },
];
