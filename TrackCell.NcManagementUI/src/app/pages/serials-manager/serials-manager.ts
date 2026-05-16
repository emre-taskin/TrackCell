import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PartDefinition, PartSerial } from '../../models/nc.models';
import { PartService } from '../../services/part.service';
import { SerialService } from '../../services/serial.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-serials-manager',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './serials-manager.html',
  styleUrl: './serials-manager.css'
})
export class SerialsManagerComponent implements OnInit {
  private partService = inject(PartService);
  private serialService = inject(SerialService);
  private toast = inject(ToastService);

  parts = signal<PartDefinition[]>([]);
  selectedPartId = signal<number | null>(null);
  serials = signal<PartSerial[]>([]);
  
  newSerialNumber = '';
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
    this.loadSerials(partId);
  }

  loadSerials(partId: number): void {
    this.serialService.getSerialsByPart(partId).subscribe({
      next: (data) => this.serials.set(data),
      error: () => this.toast.show('Failed to load serials', 'error')
    });
  }

  addSerial(): void {
    const partId = this.selectedPartId();
    if (partId === null) {
      this.toast.show('Please select a part first', 'error');
      return;
    }

    if (!this.newSerialNumber.trim()) {
      this.toast.show('Serial number is required', 'error');
      return;
    }

    this.isSubmitting.set(true);
    this.serialService.addSerial({
      partDefinitionId: partId,
      serialNumber: this.newSerialNumber.trim()
    }).subscribe({
      next: (newSerial) => {
        this.serials.update(list => [...list, newSerial].sort((a, b) => a.serialNumber.localeCompare(b.serialNumber)));
        this.toast.show('Serial created successfully', 'success');
        this.newSerialNumber = '';
        this.isSubmitting.set(false);
      },
      error: (err) => {
        const msg = err.error || 'Failed to create serial';
        this.toast.show(typeof msg === 'string' ? msg : 'Failed to create serial', 'error');
        this.isSubmitting.set(false);
      }
    });
  }
}
