import { provideHttpClient, withFetch } from '@angular/common/http';
import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideClientHydration } from '@angular/platform-browser';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { API_BASE_URL } from '@core/config/api-base-url';
import { infrastructureProviders } from '@core/providers';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(
      routes,
      withInMemoryScrolling({ scrollPositionRestoration: 'top' }),
    ),
    provideClientHydration(),
    provideHttpClient(withFetch()),
    // Empty means same-origin. The dev proxy and the production reverse proxy
    // both route `/api` to the .NET API.
    { provide: API_BASE_URL, useValue: '' },
    ...infrastructureProviders,
  ],
};
