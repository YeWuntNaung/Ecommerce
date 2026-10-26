import { isPlatformBrowser } from '@angular/common';
import { Component, inject, PLATFORM_ID } from '@angular/core';
import { AuthStore } from '@application/auth/auth.store';
import { AppShell } from '@presentation/layout/app-shell';

@Component({
  selector: 'app-root',
  imports: [AppShell],
  template: `<app-shell />`,
})
export class App {
  private readonly auth = inject(AuthStore);
  private readonly platformId = inject(PLATFORM_ID);

  constructor() {
    // On the server there is no cookie to read, so skip the session probe and
    // keep prerendering cheap and deterministic.
    if (isPlatformBrowser(this.platformId)) {
      void this.auth.ensureSession();
    }
  }
}
