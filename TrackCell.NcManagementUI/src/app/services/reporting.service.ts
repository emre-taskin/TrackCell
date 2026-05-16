import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface DashboardSummary {
  openNcsToday: number;
  ncRate7d: string;
  activeStreaks: number;
  openTickets: number;
  streaks: { partName: string, zoneId: number, count: number }[];
  trend: { date: string, count: number }[];
}

@Injectable({ providedIn: 'root' })
export class ReportingService {
  private http = inject(HttpClient);
  private base = environment.apiBase + '/Reporting';

  getDashboardSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.base}/dashboard-summary`);
  }
}
