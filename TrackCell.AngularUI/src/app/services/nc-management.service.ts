import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { NonConformance, PartImage, SaveZonesRequest } from '../models/track-cell.models';

@Injectable({ providedIn: 'root' })
export class NcManagementService {
  private http = inject(HttpClient);
  private ncBase = environment.apiBase + environment.nonConformancesPath;
  private imgBase = environment.apiBase + environment.partImagesPath;

  /** Absolute URL builder for image src (server returns relative `/uploads/...`). */
  toAbsoluteUrl(relativeOrAbsolute: string): string {
    if (!relativeOrAbsolute) return '';
    if (/^https?:\/\//i.test(relativeOrAbsolute)) return relativeOrAbsolute;
    return environment.apiBase + relativeOrAbsolute;
  }

  getNonConformances(): Observable<NonConformance[]> {
    return this.http.get<NonConformance[]>(this.ncBase);
  }

  getImagesForPart(partDefinitionId: number): Observable<PartImage[]> {
    return this.http.get<PartImage[]>(`${this.imgBase}?partDefinitionId=${partDefinitionId}`);
  }

  uploadImage(partDefinitionId: number, name: string, file: File): Observable<PartImage> {
    const form = new FormData();
    form.append('partDefinitionId', String(partDefinitionId));
    form.append('name', name);
    form.append('file', file);
    return this.http.post<PartImage>(this.imgBase, form);
  }

  deleteImage(id: number): Observable<void> {
    return this.http.delete<void>(`${this.imgBase}/${id}`);
  }

  saveZones(id: number, body: SaveZonesRequest): Observable<PartImage> {
    return this.http.put<PartImage>(`${this.imgBase}/${id}/zones`, body);
  }
}
