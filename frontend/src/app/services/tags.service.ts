import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TagPercentageDto {
  name: string;
  count: number;
  percentage: number;
}

export interface PagedResult<T> {
  data: T[];
  totalRecords: number;
}

export interface PrimeNgRequestDto {
  first: number;
  rows: number;
  sortField?: string;
  sortOrder?: number;
}

@Injectable({
  providedIn: 'root'
})
export class TagsService {
  private apiUrl = 'http://localhost:8080/api/tags';

  constructor(private http: HttpClient) {}

  getTags(request: PrimeNgRequestDto): Observable<PagedResult<TagPercentageDto>> {
    let params = new HttpParams()
      .set('first', request.first.toString())
      .set('rows', request.rows.toString());

    if (request.sortField) {
      params = params.set('sortField', request.sortField);
    }

    if (request.sortOrder !== undefined) {
      params = params.set('sortOrder', request.sortOrder.toString());
    }

    return this.http.get<PagedResult<TagPercentageDto>>(this.apiUrl, { params });
  }
}