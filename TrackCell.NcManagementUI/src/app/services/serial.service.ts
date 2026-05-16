import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PartSerial, SerialLookupResult } from '../models/nc.models';

@Injectable({ providedIn: 'root' })
export class SerialService {
  private http = inject(HttpClient);
  private base = environment.apiBase + '/serial';

  getSerialsByPart(partDefinitionId: number): Observable<PartSerial[]> {
    return this.http.get<PartSerial[]>(`${this.base}/byPart/${partDefinitionId}`);
  }

  addSerial(dto: { partDefinitionId: number; serialNumber: string }): Observable<PartSerial> {
    return this.http.post<PartSerial>(`${this.base}/add`, dto);
  }

  lookupSerial(serialNumber: string): Observable<SerialLookupResult> {
    return this.http.get<SerialLookupResult>(`${this.base}/lookup/${encodeURIComponent(serialNumber)}`);
  }
}
