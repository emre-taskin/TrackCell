import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NonConformance } from '../../models/nc.models';
import { NcManagementService } from '../../services/nc-management.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-ncs-manager',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ncs-manager.html',
  styleUrl: './ncs-manager.css'
})
export class NcsManagerComponent implements OnInit {
  private ncService = inject(NcManagementService);
  private toast = inject(ToastService);

  ncs = signal<NonConformance[]>([]);
  
  newNcCode = '';
  newNcDescription = '';
  isSubmitting = signal(false);

  ngOnInit(): void {
    this.loadNcs();
  }

  loadNcs(): void {
    this.ncService.getNonConformances().subscribe({
      next: (data) => this.ncs.set(data),
      error: () => this.toast.show('Failed to load NCs', 'error')
    });
  }

  addNc(): void {
    if (!this.newNcCode.trim() || !this.newNcDescription.trim()) {
      this.toast.show('Both Code and Description are required', 'error');
      return;
    }

    this.isSubmitting.set(true);
    this.ncService.createNonConformance({
      code: this.newNcCode.trim(),
      description: this.newNcDescription.trim()
    }).subscribe({
      next: (newNc) => {
        this.ncs.update(list => [...list, newNc].sort((a, b) => a.code.localeCompare(b.code)));
        this.toast.show('NC created successfully', 'success');
        this.newNcCode = '';
        this.newNcDescription = '';
        this.isSubmitting.set(false);
      },
      error: (err) => {
        const msg = err.error || 'Failed to create NC';
        this.toast.show(typeof msg === 'string' ? msg : 'Failed to create NC', 'error');
        this.isSubmitting.set(false);
      }
    });
  }
}
