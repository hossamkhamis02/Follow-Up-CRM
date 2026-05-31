import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AppConfigService } from '../config';

type QueryValue = string | number | boolean | null | undefined;

interface RequestOptions {
  params?: object;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl: string;

  constructor(
    private readonly http: HttpClient,
    config: AppConfigService
  ) {
    this.baseUrl = config.apiUrl.replace(/\/$/, '');
  }

  get<T>(endpoint: string, options?: RequestOptions): Observable<T> {
    return this.http.get<T>(this.buildUrl(endpoint), {
      params: this.buildParams(options?.params),
    });
  }

  post<T>(endpoint: string, body: unknown, options?: RequestOptions): Observable<T> {
    return this.http.post<T>(this.buildUrl(endpoint), body, {
      params: this.buildParams(options?.params),
    });
  }

  put<T>(endpoint: string, body: unknown, options?: RequestOptions): Observable<T> {
    return this.http.put<T>(this.buildUrl(endpoint), body, {
      params: this.buildParams(options?.params),
    });
  }

  patch<T>(endpoint: string, body: unknown, options?: RequestOptions): Observable<T> {
    return this.http.patch<T>(this.buildUrl(endpoint), body, {
      params: this.buildParams(options?.params),
    });
  }

  delete<T>(endpoint: string, options?: RequestOptions): Observable<T> {
    return this.http.delete<T>(this.buildUrl(endpoint), {
      params: this.buildParams(options?.params),
    });
  }

  private buildUrl(endpoint: string): string {
    if (/^https?:\/\//i.test(endpoint)) {
      return endpoint;
    }

    return `${this.baseUrl}/${endpoint.replace(/^\/+/, '')}`;
  }

  private buildParams(params?: object): HttpParams {
    let httpParams = new HttpParams();

    Object.entries(params ?? {}).forEach(([key, value]) => {
      const values = Array.isArray(value) ? value : [value as QueryValue];

      values.forEach(item => {
        if (item !== null && item !== undefined && item !== '') {
          httpParams = httpParams.append(key, String(item));
        }
      });
    });

    return httpParams;
  }
}
