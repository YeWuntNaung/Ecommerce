import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  // The landing page is static, so it can be prerendered at build time.
  { path: '', renderMode: RenderMode.Prerender },
  // Everything else depends on route params, authentication, or live API data.
  { path: '**', renderMode: RenderMode.Client },
];
