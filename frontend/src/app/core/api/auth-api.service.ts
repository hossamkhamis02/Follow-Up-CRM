import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiResponse } from '../models/shared';
import { LoginRequest, LoginResponse, RegisterRequest } from '../models/auth';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly endpoint = 'auth';

  constructor(private readonly api: ApiService) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.api
      .post<ApiResponse<LoginResponse>>(`${this.endpoint}/login`, request)
      .pipe(map(response => response.data));
  }

  register(request: RegisterRequest): Observable<LoginResponse> {
    return this.api
      .post<ApiResponse<LoginResponse>>(`${this.endpoint}/register`, request)
      .pipe(map(response => response.data));
  }

  me(): Observable<LoginResponse> {
    return this.api
      .get<ApiResponse<LoginResponse>>(`${this.endpoint}/me`)
      .pipe(map(response => response.data));
  }
}
