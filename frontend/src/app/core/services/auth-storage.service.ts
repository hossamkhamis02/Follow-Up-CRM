import { Injectable } from '@angular/core';
import { AuthUser, LoginResponse } from '../models/auth';
import { TOKEN_KEY, USER_KEY } from '../constants';

@Injectable({ providedIn: 'root' })
export class AuthStorageService {
  getAccessToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getUser(): AuthUser | null {
    const value = localStorage.getItem(USER_KEY);

    if (!value) {
      return null;
    }

    try {
      return JSON.parse(value) as AuthUser;
    } catch {
      this.clear();
      return null;
    }
  }

  saveSession(response: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, response.token);
    localStorage.setItem(USER_KEY, JSON.stringify(this.toAuthUser(response)));
  }

  clear(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }

  private toAuthUser(response: LoginResponse): AuthUser {
    return {
      id: response.userId,
      email: response.email,
      fullName: response.fullName,
      role: response.role,
    };
  }
}
