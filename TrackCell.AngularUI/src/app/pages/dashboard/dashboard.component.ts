import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { Subscription } from 'rxjs';
import { OperationHistory } from '../../models/track-cell.models';
import { DashboardHubService } from '../../services/dashboard-hub.service';
import { ToastService } from '../../services/toast.service';
import { OperationHistoryService } from '../../services/operation-history.service';

interface OpColumn {
  opNumber: string;
  count: number;
  badgeColor: string;
  items: OperationHistory[];
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit, OnDestroy {
  private operationHistory = inject(OperationHistoryService);
  private hub = inject(DashboardHubService);
  private toast = inject(ToastService);

  columns = signal<OpColumn[]>([]);
  lastUpdated = signal<string>('-');

  private hubSub?: Subscription;

  ngOnInit(): void {
    this.fetchActiveItems();
    this.hub.start();
    this.hubSub = this.hub.onUpdate.subscribe(() => {
      this.fetchActiveItems();
      this.toast.show('Live update received from Redis', 'success');
    });
  }

  ngOnDestroy(): void {
    this.hubSub?.unsubscribe();
  }

  private fetchActiveItems(): void {
    this.operationHistory.getInProgress().subscribe({
      next: items => {
        this.columns.set(this.groupByOp(items));
        this.lastUpdated.set(new Date().toLocaleTimeString());
      },
      error: e => console.error('Failed to fetch items:', e)
    });
  }

  private groupByOp(items: OperationHistory[]): OpColumn[] {
    const map: Record<string, OperationHistory[]> = {};
    for (const it of items) {
      (map[it.opNumber] ??= []).push(it);
    }
    return Object.entries(map).map(([opNumber, ops]) => {
      let badgeColor = 'var(--success-color)';
      const count = ops.length;
      if (count >= 3 && count <= 5) badgeColor = '#f59e0b';
      else if (count > 5) badgeColor = 'var(--danger-color)';
      return { opNumber, count, badgeColor, items: ops };
    });
  }

  trackColumn(_: number, c: OpColumn): string { return c.opNumber; }
  trackItem(_: number, i: OperationHistory): string { return `${i.part}-${i.serial}-${i.opNumber}`; }
}
