import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { OperationDefinition } from '../models/nc.models';

@Injectable({ providedIn: 'root' })
export class OperationService {
  private http = inject(HttpClient);
  private base = environment.apiBase + '/operation';

  getOperationsByPart(partDefinitionId: number): Observable<OperationDefinition[]> {
    return this.http.get<OperationDefinition[]>(`${this.base}/byPart/${partDefinitionId}`);
  }

  addOperation(dto: { partDefinitionId: number; opNumber: string; description: string }): Observable<OperationDefinition> {
    return this.http.post<OperationDefinition>(`${this.base}/add`, dto);
  }
}
