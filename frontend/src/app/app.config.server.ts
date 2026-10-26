import { mergeApplicationConfig, ApplicationConfig } from '@angular/core';
import { provideServerRendering, withRoutes } from '@angular/ssr';
import { API_BASE_URL } from './core/config/api-base-url';
import { appConfig } from './app.config';
import { serverRoutes } from './app.routes.server';

const serverConfig: ApplicationConfig = {
  providers: [
    provideServerRendering(withRoutes(serverRoutes)),
    // The server renderer has no request origin to resolve `/api` against.
    {
      provide: API_BASE_URL,
      useFactory: () => process.env['API_BASE_URL'] ?? 'http://localhost:5080',
    },
  ],
};

export const config = mergeApplicationConfig(appConfig, serverConfig);
