import { InjectionToken } from '@angular/core';

/**
 * Origin used for API calls.
 *
 * In the browser this is an empty string so requests stay same-origin: the dev
 * server proxies `/api` to the backend, and in production a reverse proxy serves
 * the SPA and the API from one origin (which keeps the auth cookie simple).
 *
 * The server renderer has no origin of its own, so `app.config.server.ts`
 * overrides this with an absolute URL.
 */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');
