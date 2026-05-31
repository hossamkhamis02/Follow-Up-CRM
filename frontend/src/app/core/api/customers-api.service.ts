import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { CreateCustomerRequest, CustomerDto, UpdateCustomerRequest } from '../models/customers';
import { ApiResponse, PagedRequest, PagedResult } from '../models/shared';
import { ApiService } from './api.service';

interface BackendPagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

@Injectable({ providedIn: 'root' })
export class CustomersApiService {
  private readonly endpoint = 'v1/customers';

  constructor(private readonly api: ApiService) {}

  getCustomers(request: PagedRequest): Observable<PagedResult<CustomerDto>> {
    return this.api
      .get<ApiResponse<BackendPagedResult<CustomerDto>>>(this.endpoint, { params: request })
      .pipe(map(response => this.mapPagedResult(response.data)));
  }

  getCustomer(id: string): Observable<CustomerDto> {
    return this.api
      .get<ApiResponse<CustomerDto>>(`${this.endpoint}/${id}`)
      .pipe(map(response => response.data));
  }

  createCustomer(request: CreateCustomerRequest): Observable<string> {
    return this.api
      .post<ApiResponse<string>>(this.endpoint, request)
      .pipe(map(response => response.data));
  }

  updateCustomer(id: string, request: UpdateCustomerRequest): Observable<string> {
    return this.api
      .put<ApiResponse<string>>(`${this.endpoint}/${id}`, request)
      .pipe(map(response => response.data));
  }

  deleteCustomer(id: string): Observable<string> {
    return this.api
      .delete<ApiResponse<string>>(`${this.endpoint}/${id}`)
      .pipe(map(response => response.data));
  }

  private mapPagedResult<T>(result: BackendPagedResult<T>): PagedResult<T> {
    return {
      items: result.data,
      totalCount: result.totalCount,
      page: result.page,
      pageSize: result.pageSize,
      totalPages: result.totalPages,
      hasNextPage: result.hasNextPage,
      hasPreviousPage: result.hasPreviousPage,
    };
  }
}
