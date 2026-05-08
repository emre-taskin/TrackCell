import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  OperationDefinition,
  Operator,
  PartDefinition,
  SerialHistory
} from '../models/track-cell.models';

@Injectable({ providedIn: 'root' })
export class MasterDataService {
  private http = inject(HttpClient);
  private base = environment.apiBase + environment.masterDataPath;

  getOperators(): Observable<Operator[]> {
    return this.http.get<Operator[]>(`${this.base}/operators`);
  }

  getOperatorByBadge(badge: string): Observable<Operator> {
    return this.http.get<Operator>(`${this.base}/operators/${encodeURIComponent(badge)}`);
  }

  getParts(): Observable<PartDefinition[]> {
    return this.http.get<PartDefinition[]>(`${this.base}/parts`);
  }

  getOperations(): Observable<OperationDefinition[]> {
    return this.http.get<OperationDefinition[]>(`${this.base}/operations`);
  }

  getSerialHistory(serial: string): Observable<SerialHistory> {
    return this.http.get<SerialHistory>(`${this.base}/serial/${encodeURIComponent(serial)}`);
  }
}
