import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  OperationDefinition,
  PartDefinition,
  SerialHistory
} from '../models/track-cell.models';

@Injectable({ providedIn: 'root' })
export class MasterDataService {
  private http = inject(HttpClient);
  private base = environment.apiBase + environment.masterDataPath;

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
