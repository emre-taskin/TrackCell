import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { OperationDefinition, PartDefinition } from '../../models/nc.models';
import { PartService } from '../../services/part.service';
import { OperationService } from '../../services/operation.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-operations-manager',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './operations-manager.html',
  styleUrl: './operations-manager.css'
})
export class OperationsManagerComponent implements OnInit {
  private partService = inject(PartService);
  private operationService = inject(OperationService);
  private toast = inject(ToastService);

  parts = signal<PartDefinition[]>([]);
  selectedPartId = signal<number | null>(null);
  operations = signal<OperationDefinition[]>([]);
  
  newOpNumber = '';
  newOpDescription = '';
  isSubmitting = signal(false);

  ngOnInit(): void {
    this.loadParts();
  }

  loadParts(): void {
    this.partService.getParts().subscribe({
      next: (data) => this.parts.set(data),
      error: () => this.toast.show('Failed to load parts', 'error')
    });
  }

  selectPart(partId: number): void {
    this.selectedPartId.set(partId);
    this.loadOperations(partId);
  }

  loadOperations(partId: number): void {
    this.operationService.getOperationsByPart(partId).subscribe({
      next: (data) => this.operations.set(data),
      error: () => this.toast.show('Failed to load operations', 'error')
    });
  }

  addOperation(): void {
    const partId = this.selectedPartId();
    if (partId === null) {
      this.toast.show('Please select a part first', 'error');
      return;
    }

    if (!this.newOpNumber.trim()) {
      this.toast.show('Operation number is required', 'error');
      return;
    }

    this.isSubmitting.set(true);
    this.operationService.addOperation({
      partDefinitionId: partId,
      opNumber: this.newOpNumber.trim(),
      description: this.newOpDescription.trim()
    }).subscribe({
      next: (newOp) => {
        this.operations.update(list => [...list, newOp].sort((a, b) => a.opNumber.localeCompare(b.opNumber)));
        this.toast.show('Operation created successfully', 'success');
        this.newOpNumber = '';
        this.newOpDescription = '';
        this.isSubmitting.set(false);
      },
      error: (err) => {
        const msg = err.error || 'Failed to create operation';
        this.toast.show(typeof msg === 'string' ? msg : 'Failed to create operation', 'error');
        this.isSubmitting.set(false);
      }
    });
  }
}
