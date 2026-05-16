import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { User } from '../models/track-cell.models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private base = environment.apiBase + environment.userPath;

  getByRole(role: string): Observable<User[]> {
    return this.http.get<User[]>(`${this.base}/byRole/${encodeURIComponent(role)}`);
  }

  getByBadge(badge: string): Observable<User> {
    return this.http.get<User>(`${this.base}/byBadge/${encodeURIComponent(badge)}`);
  }
}
