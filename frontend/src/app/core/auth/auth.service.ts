import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { ApiService } from '../services/api.service';
import { LoginRequest, RegisterRequest, AuthResponse, AuthUser } from './auth.models';
import { ApiResponse } from '../../shared/models/api-response.models';
import { TOKEN_KEY, REFRESH_TOKEN_KEY } from '../constants';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private _currentUser = new BehaviorSubject<AuthUser | null>(null);
  private _isAuthenticated = new BehaviorSubject<boolean>(false);

  currentUser$ = this._currentUser.asObservable();
  isAuthenticated$ = this._isAuthenticated.asObservable();

  constructor(private api: ApiService) {
    this.initializeFromStorage();
  }

  private initializeFromStorage(): void {
    const token = localStorage.getItem(TOKEN_KEY);
    if (token) {
      this._isAuthenticated.next(true);
    }
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('identity/login', request).pipe(
      map(response => response.data),
      tap(data => {
        if (data) {
          this.setSession(data);
        }
      })
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('identity/register', request).pipe(
      map(response => response.data)
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    this._currentUser.next(null);
    this._isAuthenticated.next(false);
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem(TOKEN_KEY);
  }

  getUserFullName(): string {
    const user = this._currentUser.value;
    return user ? `${user.firstName} ${user.lastName}` : '';
  }

  getUserEmail(): string {
    const user = this._currentUser.value;
    return user?.email ?? '';
  }

  hasRole(role: string): boolean {
    const user = this._currentUser.value;
    return user?.roles.includes(role) ?? false;
  }

  private setSession(response: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, response.token);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    this._currentUser.next(response.user);
    this._isAuthenticated.next(true);
  }
}