import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { OperationDefinition, PartDefinition } from '../models/nc.models';

@Injectable({ providedIn: 'root' })
export class MasterDataService {
  private http = inject(HttpClient);
  private base = environment.apiBase + environment.masterDataPath;

  getParts(): Observable<PartDefinition[]> {
    return this.http.get<PartDefinition[]>(`${this.base}/getParts`);
  }

  getOperationsByPart(partNumber: string): Observable<OperationDefinition[]> {
    const url = `${this.base}/getOperationsByPart?partNumber=${encodeURIComponent(partNumber)}`;
    return this.http.get<OperationDefinition[]>(url);
  }
}
