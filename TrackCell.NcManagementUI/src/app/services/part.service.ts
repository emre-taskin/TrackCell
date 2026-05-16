import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PartDefinition } from '../models/nc.models';

@Injectable({ providedIn: 'root' })
export class PartService {
  private http = inject(HttpClient);
  private base = environment.apiBase + '/part';

  getParts(): Observable<PartDefinition[]> {
    return this.http.get<PartDefinition[]>(this.base);
  }

  addPart(dto: { partNumber: string; description: string }): Observable<PartDefinition> {
    return this.http.post<PartDefinition>(`${this.base}/addPart`, dto);
  }
}
