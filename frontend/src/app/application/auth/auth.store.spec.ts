import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { CurrentUser } from '@domain/identity/models';
import { AuthGateway } from '@domain/identity/ports/auth.gateway';
import { Observable, of, throwError } from 'rxjs';
import { AuthStore } from './auth.store';

const adminUser: CurrentUser = {
  userId: 'user-1',
  username: 'admin',
  roles: ['Admin'],
};

class FakeAuthGateway extends AuthGateway {
  login = vi.fn((_username: string, _password: string): Observable<void> => of(void 0));
  logout = vi.fn((): Observable<void> => of(void 0));
  me = vi.fn((): Observable<CurrentUser> => of(adminUser));
}

describe('AuthStore', () => {
  let gateway: FakeAuthGateway;
  let store: InstanceType<typeof AuthStore>;

  beforeEach(() => {
    gateway = new FakeAuthGateway();

    TestBed.configureTestingModule({
      providers: [{ provide: AuthGateway, useValue: gateway }],
    });

    store = TestBed.inject(AuthStore);
  });

  it('resolves concurrent session probes only once', async () => {
    await Promise.all([store.ensureSession(), store.ensureSession()]);

    expect(store.isAuthenticated()).toBe(true);
    expect(store.isAdmin()).toBe(true);
    expect(gateway.me).toHaveBeenCalledTimes(1);
  });

  it('treats an unauthorized probe as anonymous', async () => {
    gateway.me.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 401 })));

    await store.ensureSession();

    expect(store.isAuthenticated()).toBe(false);
    expect(store.status()).toBe('anonymous');
  });

  it('surfaces the API message when sign in fails', async () => {
    gateway.login.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 401,
            error: { detail: 'Invalid username or password.' },
          }),
      ),
    );

    const signedIn = await store.login('vendor', 'wrong');

    expect(signedIn).toBe(false);
    expect(store.error()).toBe('Invalid username or password.');
    expect(store.isAuthenticated()).toBe(false);
  });

  it('loads the user after a successful sign in', async () => {
    const signedIn = await store.login('admin', 'Admin123!');

    expect(signedIn).toBe(true);
    expect(store.isAdmin()).toBe(true);
    expect(store.status()).toBe('authenticated');
  });

  it('clears the session on sign out', async () => {
    await store.ensureSession();
    await store.logout();

    expect(store.isAuthenticated()).toBe(false);
    expect(store.status()).toBe('anonymous');
    expect(gateway.logout).toHaveBeenCalledTimes(1);
  });
});
