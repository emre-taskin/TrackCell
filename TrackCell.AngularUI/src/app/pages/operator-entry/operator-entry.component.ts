import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild,
  inject,
  signal
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import {
  OperationDefinition,
  PartDefinition,
  SerialHistory,
  User,
  WorkItem
} from '../../models/track-cell.models';
import { DashboardHubService } from '../../services/dashboard-hub.service';
import { MasterDataService } from '../../services/master-data.service';
import { ToastService } from '../../services/toast.service';
import { UserService } from '../../services/user.service';
import { OperationHistoryService } from '../../services/operation-history.service';

const REQUIRED_ROLE = 'Operator';

type StepState = 'idle' | 'active' | 'done';

interface PendingCompletion {
  partSerialId: number;
  opNumber: string;
}

@Component({
  selector: 'app-operator-entry',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './operator-entry.component.html',
  styleUrl: './operator-entry.component.css'
})
export class OperatorEntryComponent implements OnInit, AfterViewInit, OnDestroy {
  private masterData = inject(MasterDataService);
  private users = inject(UserService);
  private workItems = inject(OperationHistoryService);
  private hub = inject(DashboardHubService);
  private toast = inject(ToastService);

  @ViewChild('badgeInput') badgeInput?: ElementRef<HTMLInputElement>;
  @ViewChild('serialInput') serialInput?: ElementRef<HTMLInputElement>;
  @ViewChild('partSelect') partSelect?: ElementRef<HTMLSelectElement>;
  @ViewChild('opSelect') opSelect?: ElementRef<HTMLSelectElement>;
  @ViewChild('confirmBadge') confirmBadge?: ElementRef<HTMLInputElement>;
  @ViewChild('completionDialog') completionDialog?: ElementRef<HTMLDialogElement>;

  // Form fields
  badge = '';
  serial = '';
  part = '';
  opNumber = '';

  // State
  operator = signal<User | null>(null);
  serialHistory = signal<SerialHistory | null>(null);
  partSerialId = signal<number>(0);
  allParts = signal<PartDefinition[]>([]);
  allOps = signal<OperationDefinition[]>([]);

  // Field info messages
  operatorInfo = signal<{ text: string; color: string }>({ text: '', color: 'var(--text-secondary)' });
  serialInfo = signal<{ text: string; color: string }>({ text: '', color: 'var(--text-secondary)' });
  partInfo = signal<{ text: string; color: string }>({ text: '', color: 'var(--text-secondary)' });

  // Step state
  step2 = signal<StepState>('idle');
  step3 = signal<StepState>('idle');
  step4 = signal<StepState>('idle');

  // Field enabled flags
  serialEnabled = signal(false);
  partEnabled = signal(false);
  opEnabled = signal(false);
  startEnabled = signal(false);

  // Suggested op markers
  partOptions = signal<{ value: string; label: string }[]>([]);
  opOptions = signal<{ value: string; label: string; disabled: boolean }[]>([]);

  // Active items
  activeItems = signal<WorkItem[]>([]);

  // Completion modal
  pendingCompletion: PendingCompletion | null = null;
  confirmBadgeValue = '';

  private hubSub?: Subscription;

  ngOnInit(): void {
    this.loadMasterData();
    this.fetchActiveItems();
    this.hub.start();
    this.hubSub = this.hub.onUpdate.subscribe(() => this.fetchActiveItems());
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.badgeInput?.nativeElement.focus(), 0);
  }

  ngOnDestroy(): void {
    this.hubSub?.unsubscribe();
  }

  // ---------- master data ----------
  private loadMasterData(): void {
    this.masterData.getParts().subscribe({
      next: parts => this.allParts.set(parts),
      error: e => console.warn('parts load failed', e)
    });
    this.masterData.getOperations().subscribe({
      next: ops => this.allOps.set(ops),
      error: e => console.warn('ops load failed', e)
    });
  }

  private fetchActiveItems(): void {
    this.workItems.getInProgress().subscribe({
      next: items => this.activeItems.set(items),
      error: e => console.warn('active fetch failed', e)
    });
  }

  // ---------- step 1: badge ----------
  onBadgeKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      event.preventDefault();
      const v = this.badge.trim();
      if (!v) return;
      this.lookupOperator(v);
    }
  }

  private lookupOperator(badge: string): void {
    this.operatorInfo.set({ text: 'Looking up badge...', color: 'var(--text-secondary)' });
    this.users.getByBadge(badge).subscribe({
      next: user => {
        if (user.role !== REQUIRED_ROLE) {
          this.operator.set(null);
          this.operatorInfo.set({
            text: `Only ${REQUIRED_ROLE}s can start work here (badge belongs to a ${user.role}).`,
            color: 'var(--danger-color)'
          });
          return;
        }
        this.operator.set(user);
        this.operatorInfo.set({ text: '', color: 'var(--success-color)' });
        this.step2.set('active');
        this.serialEnabled.set(true);
        setTimeout(() => this.serialInput?.nativeElement.focus(), 0);
      },
      error: (err: HttpErrorResponse) => {
        this.operator.set(null);
        if (err.status === 404) {
          this.operatorInfo.set({ text: `Unknown badge: ${badge}`, color: 'var(--danger-color)' });
        } else {
          this.operatorInfo.set({ text: 'Lookup failed (API down?)', color: 'var(--danger-color)' });
        }
      }
    });
  }

  // ---------- step 2: serial ----------
  onSerialKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      event.preventDefault();
      const v = this.serial.trim();
      if (!v) return;
      this.lookupSerial(v);
    }
  }

  private lookupSerial(serial: string): void {
    this.serialInfo.set({ text: 'Looking up serial...', color: 'var(--text-secondary)' });
    this.masterData.getSerialHistory(serial).subscribe({
      next: history => {
        this.serialHistory.set(history);
        const completedTxt = history.completedOps.length
          ? ` · Completed: ${history.completedOps.join(', ')}`
          : '';
        this.serialInfo.set({ text: `Found in history${completedTxt}`, color: 'var(--success-color)' });
        this.applySerialMatch(history);
      },
      error: (err: HttpErrorResponse) => {
        if (err.status === 404) {
          this.serialHistory.set(null);
          this.partSerialId.set(0);
          this.serialInfo.set({ text: 'Serial not found. Must be defined in Serials Manager.', color: 'var(--danger-color)' });
        } else {
          this.serialInfo.set({ text: 'Lookup failed.', color: 'var(--danger-color)' });
        }
      }
    });
  }

  private applySerialMatch(history: SerialHistory): void {
    const matchPart = this.allParts().find(p => p.partNumber === history.partNumber);
    const desc = matchPart ? matchPart.description : history.partDescription || '';
    const label = `${desc ? desc + ' ' : ''}(${history.partNumber})`;
    this.partSerialId.set(history.partSerialId);
    this.partOptions.set([{ value: history.partNumber, label }]);
    this.part = history.partNumber;
    this.partEnabled.set(false);
    this.partInfo.set({ text: 'Auto-filled from serial history', color: 'var(--text-secondary)' });
    this.step3.set('done');
    this.populateOpsForPart(history);
  }

  private enablePartManualSelect(): void {
    const opts = [{ value: '', label: 'Select Part...' }];
    this.allParts().forEach(p => opts.push({ value: p.partNumber, label: `${p.description} (${p.partNumber})` }));
    this.partOptions.set(opts);
    this.part = '';
    this.partEnabled.set(true);
    this.partInfo.set({ text: 'Select the part for this new serial', color: 'var(--text-secondary)' });
    this.step3.set('active');
    setTimeout(() => this.partSelect?.nativeElement.focus(), 0);
  }

  onPartChange(): void {
    if (!this.part) return;
    this.step3.set('done');
    this.populateOpsForPart(null);
  }

  private populateOpsForPart(history: SerialHistory | null): void {
    const completed = new Set(history?.completedOps ?? []);
    const inProcess = new Set(history?.inProcessOps ?? []);
    const opts: { value: string; label: string; disabled: boolean }[] = [
      { value: '', label: 'Select Operation...', disabled: false }
    ];
    let suggestedIdx = -1;
    this.allOps().forEach((o, i) => {
      let label = `${o.description} (${o.opNumber})`;
      let disabled = false;
      if (completed.has(o.opNumber)) {
        label = '✓ ' + label + ' — done';
        disabled = true;
      } else if (inProcess.has(o.opNumber)) {
        label = '⏳ ' + label + ' — in process';
      } else if (suggestedIdx === -1) {
        suggestedIdx = i;
        label = '▶ ' + label;
      }
      opts.push({ value: o.opNumber, label, disabled });
    });
    this.opOptions.set(opts);
    this.opEnabled.set(true);
    if (suggestedIdx >= 0) {
      this.opNumber = this.allOps()[suggestedIdx].opNumber;
      this.step4.set('done');
      this.startEnabled.set(true);
    } else {
      this.opNumber = '';
      this.step4.set('active');
      this.startEnabled.set(false);
    }
    setTimeout(() => this.opSelect?.nativeElement.focus(), 0);
  }

  onOpChange(): void {
    this.startEnabled.set(!!this.opNumber);
    if (this.opNumber) this.step4.set('done');
  }

  // ---------- start ----------
  onSubmit(event: Event): void {
    event.preventDefault();
    const op = this.operator();
    if (!op) {
      this.toast.show('Scan badge first', 'error');
      return;
    }
    if (!this.part || !this.serial.trim() || !this.opNumber) {
      this.toast.show('All fields are required', 'error');
      return;
    }
    this.workItems
      .start({
        badgeNumber: op.badgeNumber ?? '',
        partSerialId: this.partSerialId(),
        opNumber: this.opNumber
      })
      .subscribe({
        next: () => {
          this.toast.show('Operation started successfully!');
          this.reset();
        },
        error: err => {
          const msg = typeof err?.error === 'string' ? err.error : 'Failed to start operation';
          this.toast.show(`Error: ${msg}`, 'error');
        }
      });
  }

  private reset(): void {
    this.operator.set(null);
    this.serialHistory.set(null);
    this.partSerialId.set(0);
    this.badge = '';
    this.serial = '';
    this.part = '';
    this.opNumber = '';
    this.partOptions.set([{ value: '', label: 'Scan serial first...' }]);
    this.opOptions.set([{ value: '', label: 'Select part first...', disabled: false }]);
    this.serialEnabled.set(false);
    this.partEnabled.set(false);
    this.opEnabled.set(false);
    this.startEnabled.set(false);
    this.operatorInfo.set({ text: '', color: 'var(--text-secondary)' });
    this.serialInfo.set({ text: '', color: 'var(--text-secondary)' });
    this.partInfo.set({ text: '', color: 'var(--text-secondary)' });
    this.step2.set('idle');
    this.step3.set('idle');
    this.step4.set('idle');
    setTimeout(() => this.badgeInput?.nativeElement.focus(), 0);
  }

  // ---------- complete flow ----------
  promptCompletion(item: WorkItem): void {
    this.pendingCompletion = { partSerialId: item.partSerialId, opNumber: item.opNumber };
    this.confirmBadgeValue = this.operator()?.badgeNumber ?? '';
    this.completionDialog?.nativeElement.showModal();
    setTimeout(() => this.confirmBadge?.nativeElement.focus(), 0);
  }

  closeModal(): void {
    this.completionDialog?.nativeElement.close();
    this.pendingCompletion = null;
  }

  submitCompletion(): void {
    if (!this.pendingCompletion) return;
    const badge = this.confirmBadgeValue.trim();
    if (!badge) {
      this.toast.show('Badge number is required', 'error');
      return;
    }
    this.workItems
      .complete({
        partSerialId: this.pendingCompletion.partSerialId,
        opNumber: this.pendingCompletion.opNumber,
        badgeNumber: badge
      })
      .subscribe({
        next: () => {
          this.toast.show('Operation marked as completed');
          this.closeModal();
        },
        error: () => this.toast.show('Failed to complete operation', 'error')
      });
  }

  stepClass(state: StepState): string {
    if (state === 'active') return 'step-badge step-badge-active';
    if (state === 'done') return 'step-badge step-badge-done';
    return 'step-badge';
  }
}
