import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CompleteOperationRequest,
  StartWorkItemRequest,
  WorkItem
} from '../models/track-cell.models';

@Injectable({ providedIn: 'root' })
export class WorkItemsService {
  private http = inject(HttpClient);
  private base = environment.apiBase + environment.workItemsPath;

  getActive(): Observable<WorkItem[]> {
    return this.http.get<WorkItem[]>(`${this.base}/active`);
  }

  start(req: StartWorkItemRequest): Observable<WorkItem> {
    return this.http.post<WorkItem>(`${this.base}/start`, req);
  }

  complete(req: CompleteOperationRequest): Observable<unknown> {
    return this.http.post(`${this.base}/complete`, req, { responseType: 'text' });
  }
}
