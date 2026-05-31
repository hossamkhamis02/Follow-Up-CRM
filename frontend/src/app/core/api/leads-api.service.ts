import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { CreateLeadRequest, LeadDto, UpdateLeadRequest } from '../models/leads';
import { ApiResponse, PagedRequest, PagedResult } from '../models/shared';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class LeadsApiService {
  private readonly endpoint = 'v1/leads';

  constructor(private readonly api: ApiService) {}

  getLeads(request: PagedRequest): Observable<PagedResult<LeadDto>> {
    return this.api
      .get<ApiResponse<PagedResult<LeadDto>>>(this.endpoint, { params: request })
      .pipe(map(response => response.data));
  }

  getLead(id: string): Observable<LeadDto> {
    return this.api
      .get<ApiResponse<LeadDto>>(`${this.endpoint}/${id}`)
      .pipe(map(response => response.data));
  }

  createLead(request: CreateLeadRequest): Observable<string> {
    return this.api
      .post<ApiResponse<string>>(this.endpoint, request)
      .pipe(map(response => response.data));
  }

  updateLead(id: string, request: UpdateLeadRequest): Observable<string> {
    return this.api
      .put<ApiResponse<string>>(`${this.endpoint}/${id}`, request)
      .pipe(map(response => response.data));
  }

  deleteLead(id: string): Observable<string> {
    return this.api
      .delete<ApiResponse<string>>(`${this.endpoint}/${id}`)
      .pipe(map(response => response.data));
  }
}
