import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AppConfigService } from '../config/app-config.service';
import { ApiResponse, PagedResponse, PagedRequest } from '../../shared/models/api-response.models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private _baseUrl: string;

  constructor(private http: HttpClient, private config: AppConfigService) {
    this._baseUrl = config.apiUrl;
  }

  get<T>(endpoint: string): Observable<ApiResponse<T>> {
    return this.http.get<ApiResponse<T>>(`${this._baseUrl}/${endpoint}`);
  }

  getRaw<T>(endpoint: string): Observable<T> {
    return this.http.get<T>(`${this._baseUrl}/${endpoint}`);
  }

  post<T>(endpoint: string, body: unknown): Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(`${this._baseUrl}/${endpoint}`, body);
  }

  put<T>(endpoint: string, body: unknown): Observable<ApiResponse<T>> {
    return this.http.put<ApiResponse<T>>(`${this._baseUrl}/${endpoint}`, body);
  }

  patch<T>(endpoint: string, body: unknown): Observable<ApiResponse<T>> {
    return this.http.patch<ApiResponse<T>>(`${this._baseUrl}/${endpoint}`, body);
  }

  delete<T>(endpoint: string): Observable<ApiResponse<T>> {
    return this.http.delete<ApiResponse<T>>(`${this._baseUrl}/${endpoint}`);
  }

  getPaged<T>(endpoint: string, request: PagedRequest): Observable<PagedResponse<T>> {
    const params = this.buildPagedParams(request);
    return this.http.get<PagedResponse<T>>(`${this._baseUrl}/${endpoint}`, { params });
  }

  private buildPagedParams(request: PagedRequest): Record<string, string | number> {
    return {
      pageNumber: request.pageNumber,
      pageSize: request.pageSize,
      ...(request.searchTerm ? { searchTerm: request.searchTerm } : {}),
      ...(request.sortBy ? { sortBy: request.sortBy } : {}),
      ...(request.sortDirection ? { sortDirection: request.sortDirection } : {}),
    };
  }
}