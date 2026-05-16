import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { OperationDefinition, PartDefinition, PartSerial } from '../../models/nc.models';
import { SerialService } from '../../services/serial.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-inspector',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inspector.component.html',
  styleUrl: './inspector.component.css'
})
export class InspectorComponent {
  private serialService = inject(SerialService);
  private toast = inject(ToastService);

  serial = signal('');
  isLooking = signal(false);

  resolvedSerial = signal<PartSerial | null>(null);
  resolvedPart = signal<PartDefinition | null>(null);
  operations = signal<OperationDefinition[]>([]);
  selectedOperationId = signal<number | null>(null);

  lookup(): void {
    const s = this.serial().trim();
    if (!s || this.isLooking()) return;

    this.isLooking.set(true);
    this.serialService.lookupSerial(s).subscribe({
      next: (result) => {
        this.resolvedSerial.set(result.partSerial);
        this.resolvedPart.set(result.partDefinition);
        this.operations.set(result.operations);
        this.selectedOperationId.set(null);
        this.isLooking.set(false);

        if (result.operations.length === 0) {
          this.toast.show('No operations defined for this part.', 'error');
        }
      },
      error: (err) => {
        this.isLooking.set(false);
        const msg = typeof err?.error === 'string' && err.error
          ? err.error
          : `Serial '${s}' was not found.`;
        this.toast.show(msg, 'error');
      }
    });
  }

  selectOperation(operationId: number): void {
    this.selectedOperationId.set(operationId);
  }

  reset(): void {
    this.serial.set('');
    this.resolvedSerial.set(null);
    this.resolvedPart.set(null);
    this.operations.set([]);
    this.selectedOperationId.set(null);
  }
}
