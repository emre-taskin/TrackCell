import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ReportingService } from '../../services/reporting.service';

interface MetricCard {
  label: string;
  value: string | number;
  hint: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private reporting = inject(ReportingService);

  metrics = signal<MetricCard[]>([
    { label: 'Open NCs (today)', value: '—', hint: 'Total findings created today' },
    { label: 'NC rate (7d)', value: '—', hint: 'NCs per inspected operation' },
    { label: 'Active streaks', value: '—', hint: 'Zones approaching ticket threshold' },
    { label: 'Open tickets', value: '—', hint: 'Awaiting investigation or fix' }
  ]);

  streaks = signal<{ partName: string, zoneId: number, count: number }[]>([]);
  trend = signal<{ date: string, count: number }[]>([]);

  ngOnInit(): void {
    this.reporting.getDashboardSummary().subscribe({
      next: (summary) => {
        this.metrics.set([
          { label: 'Open NCs (today)', value: summary.openNcsToday, hint: 'Total findings created today' },
          { label: 'NC rate (7d)', value: summary.ncRate7d, hint: 'NCs per inspected operation' },
          { label: 'Active streaks', value: summary.activeStreaks, hint: 'Zones approaching ticket threshold' },
          { label: 'Open tickets', value: summary.openTickets, hint: 'Awaiting investigation or fix' }
        ]);
        this.streaks.set(summary.streaks);
        this.trend.set(summary.trend);
      }
    });
  }
}
