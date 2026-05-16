import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PartDefinition } from '../../models/nc.models';
import { PartService } from '../../services/part.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-parts-manager',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './parts-manager.html',
  styleUrl: './parts-manager.css'
})
export class PartsManagerComponent implements OnInit {
  private partService = inject(PartService);
  private toast = inject(ToastService);

  parts = signal<PartDefinition[]>([]);
  
  newPartNumber = '';
  newPartDescription = '';
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

  addPart(): void {
    if (!this.newPartNumber.trim()) {
      this.toast.show('Part number is required', 'error');
      return;
    }

    this.isSubmitting.set(true);
    this.partService.addPart({
      partNumber: this.newPartNumber.trim(),
      description: this.newPartDescription.trim()
    }).subscribe({
      next: (newPart) => {
        this.parts.update(list => [...list, newPart].sort((a, b) => a.partNumber.localeCompare(b.partNumber)));
        this.toast.show('Part created successfully', 'success');
        this.newPartNumber = '';
        this.newPartDescription = '';
        this.isSubmitting.set(false);
      },
      error: (err) => {
        const msg = err.error || 'Failed to create part';
        this.toast.show(typeof msg === 'string' ? msg : 'Failed to create part', 'error');
        this.isSubmitting.set(false);
      }
    });
  }
}
