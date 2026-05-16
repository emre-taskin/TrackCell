import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CreateInspectionResultRequest,
  HeatmapResponse,
  InspectionResult
} from '../models/nc.models';

@Injectable({ providedIn: 'root' })
export class InspectionResultService {
  private http = inject(HttpClient);
  private base = environment.apiBase + environment.inspectionResultsPath;

  list(partImageId: number): Observable<InspectionResult[]> {
    return this.http.get<InspectionResult[]>(`${this.base}?partImageId=${partImageId}`);
  }

  getHeatmap(partImageId: number, nonConformanceId: number | null = null): Observable<HeatmapResponse> {
    let params = new HttpParams().set('partImageId', String(partImageId));
    if (nonConformanceId != null) {
      params = params.set('nonConformanceId', String(nonConformanceId));
    }
    return this.http.get<HeatmapResponse>(`${this.base}/heatmap`, { params });
  }

  create(body: CreateInspectionResultRequest): Observable<InspectionResult> {
    return this.http.post<InspectionResult>(this.base, body);
  }
}
