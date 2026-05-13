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
    return this.http.get<Operator[]>(`${this.base}/getOperators`);
  }

  getOperatorByBadge(badge: string): Observable<Operator> {
    return this.http.get<Operator>(`${this.base}/getOperatorByBadge/${encodeURIComponent(badge)}`);
  }

  getParts(): Observable<PartDefinition[]> {
    return this.http.get<PartDefinition[]>(`${this.base}/getParts`);
  }

  getOperations(): Observable<OperationDefinition[]> {
    return this.http.get<OperationDefinition[]>(`${this.base}/getOperations`);
  }

  getOperationsByPart(partNumber: string): Observable<OperationDefinition[]> {
    const url = `${this.base}/getOperationsByPart?partNumber=${encodeURIComponent(partNumber)}`;
    return this.http.get<OperationDefinition[]>(url);
  }

  getSerialHistory(serial: string): Observable<SerialHistory> {
    return this.http.get<SerialHistory>(`${this.base}/getSerialHistory/${encodeURIComponent(serial)}`);
  }
}
