import { Observable } from 'rxjs';
import { CurrentUser } from '../models';

export abstract class AuthGateway {
  abstract login(username: string, password: string): Observable<void>;

  abstract logout(): Observable<void>;

  abstract me(): Observable<CurrentUser>;
}
