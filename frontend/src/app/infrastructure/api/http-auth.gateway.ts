import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '@core/config/api-base-url';
import { AuthGateway } from '@domain/identity/ports/auth.gateway';
import { CurrentUser } from '@domain/identity/models';

@Injectable()
export class HttpAuthGateway extends AuthGateway {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  override login(username: string, password: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/api/v1/auth/login`, { username, password });
  }

  override logout(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/api/v1/auth/logout`, {});
  }

  override me(): Observable<CurrentUser> {
    return this.http.get<CurrentUser>(`${this.baseUrl}/api/v1/auth/me`);
  }
}
