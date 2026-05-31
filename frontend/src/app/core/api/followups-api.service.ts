import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { CreateFollowUpRequest, FollowUpDto, UpdateFollowUpRequest } from '../models/followups';
import { ApiResponse, PagedRequest, PagedResult } from '../models/shared';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class FollowUpsApiService {
  private readonly endpoint = 'v1/followups';

  constructor(private readonly api: ApiService) {}

  getFollowUps(request: PagedRequest): Observable<PagedResult<FollowUpDto>> {
    return this.api
      .get<ApiResponse<PagedResult<FollowUpDto>>>(this.endpoint, { params: request })
      .pipe(map(response => response.data));
  }

  getFollowUp(id: string): Observable<FollowUpDto> {
    return this.api
      .get<ApiResponse<FollowUpDto>>(`${this.endpoint}/${id}`)
      .pipe(map(response => response.data));
  }

  createFollowUp(request: CreateFollowUpRequest): Observable<string> {
    return this.api
      .post<ApiResponse<string>>(this.endpoint, request)
      .pipe(map(response => response.data));
  }

  updateFollowUp(id: string, request: UpdateFollowUpRequest): Observable<string> {
    return this.api
      .put<ApiResponse<string>>(`${this.endpoint}/${id}`, request)
      .pipe(map(response => response.data));
  }

  deleteFollowUp(id: string): Observable<string> {
    return this.api
      .delete<ApiResponse<string>>(`${this.endpoint}/${id}`)
      .pipe(map(response => response.data));
  }
}
