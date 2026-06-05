import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { AuthApiService } from '../api';
import { AuthStorageService } from '../services';
import { LoginRequest, LoginResponse, RegisterRequest, AuthUser } from '../models/auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly currentUserSubject = new BehaviorSubject<AuthUser | null>(null);
  private readonly isAuthenticatedSubject = new BehaviorSubject<boolean>(false);

  readonly currentUser$ = this.currentUserSubject.asObservable();
  readonly isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(
    private readonly authApi: AuthApiService,
    private readonly authStorage: AuthStorageService,
    private readonly router: Router
  ) {
    this.initializeFromStorage();
  }

  private initializeFromStorage(): void {
    const token = this.authStorage.getAccessToken();
    const user = this.authStorage.getUser();

    if (token) {
      this.isAuthenticatedSubject.next(true);
      this.currentUserSubject.next(user);
    }
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.authApi.login(request).pipe(
      tap(response => this.setSession(response))
    );
  }

  register(request: RegisterRequest): Observable<LoginResponse> {
    return this.authApi.register(request);
  }

  logout(redirectToLogin = true): void {
    this.authStorage.clear();
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);

    if (redirectToLogin) {
      void this.router.navigateByUrl('/login');
    }
  }

  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  currentUser(): AuthUser | null {
    return this.currentUserSubject.value;
  }

  getToken(): string | null {
    return this.authStorage.getAccessToken();
  }

  getUserFullName(): string {
    return this.currentUserSubject.value?.fullName ?? '';
  }

  getUserEmail(): string {
    const user = this.currentUserSubject.value;
    return user?.email ?? '';
  }

  hasRole(role: string): boolean {
    return this.currentUserSubject.value?.role === role;
  }

  private setSession(response: LoginResponse): void {
    this.authStorage.saveSession(response);
    this.currentUserSubject.next(this.authStorage.getUser());
    this.isAuthenticatedSubject.next(true);
  }
}
