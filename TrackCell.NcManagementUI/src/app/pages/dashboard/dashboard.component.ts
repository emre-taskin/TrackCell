import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface MetricCard {
  label: string;
  value: string;
  hint: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  metrics: MetricCard[] = [
    { label: 'Open NCs (today)', value: '—', hint: 'Total findings created today' },
    { label: 'NC rate (7d)', value: '—', hint: 'NCs per inspected operation' },
    { label: 'Active streaks', value: '—', hint: 'Zones approaching ticket threshold' },
    { label: 'Open tickets', value: '—', hint: 'Awaiting investigation or fix' }
  ];
}
