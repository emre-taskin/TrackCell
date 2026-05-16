import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CompleteOperationRequest,
  OperationHistory,
  StartOperationHistoryRequest
} from '../models/track-cell.models';

@Injectable({ providedIn: 'root' })
export class OperationHistoryService {
  private http = inject(HttpClient);
  private base = environment.apiBase + environment.operationHistoryPath;

  getInProgress(): Observable<OperationHistory[]> {
    return this.http.get<OperationHistory[]>(`${this.base}/inprogress`);
  }

  start(req: StartOperationHistoryRequest): Observable<OperationHistory> {
    return this.http.post<OperationHistory>(`${this.base}/start`, req);
  }

  complete(req: CompleteOperationRequest): Observable<unknown> {
    return this.http.post(`${this.base}/complete`, req, { responseType: 'text' });
  }
}
