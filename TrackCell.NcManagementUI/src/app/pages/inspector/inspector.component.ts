import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CreateInspectionResultRequest, NonConformance, OperationDefinition, PartDefinition, PartImage, PartSerial } from '../../models/nc.models';
import { InspectionResultService } from '../../services/inspection-result.service';
import { NcManagementService } from '../../services/nc-management.service';
import { SerialService } from '../../services/serial.service';
import { ToastService } from '../../services/toast.service';
import { forkJoin } from 'rxjs';

export interface RecordedFinding {
  zoneId: number;
  zoneName: string;
  ncId: number;
  ncCode: string;
  notes: string;
}

@Component({
  selector: 'app-inspector',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inspector.component.html',
  styleUrl: './inspector.component.css'
})
export class InspectorComponent {
  private serialService = inject(SerialService);
  private ncService = inject(NcManagementService);
  private resultService = inject(InspectionResultService);
  private toast = inject(ToastService);
  private router = inject(Router);

  serial = signal('');
  isLooking = signal(false);
  isSubmitting = signal(false);

  resolvedSerial = signal<PartSerial | null>(null);
  resolvedPart = signal<PartDefinition | null>(null);
  operations = signal<OperationDefinition[]>([]);
  selectedOperationId = signal<number | null>(null);

  partImages = signal<PartImage[]>([]);
  selectedImage = signal<PartImage | null>(null);
  selectedZoneId = signal<number | null>(null);
  ncList = signal<NonConformance[]>([]);

  findings = signal<RecordedFinding[]>([]);
  isReviewing = signal(false);
  currentNotes = signal('');

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

        // Load images for the part
        this.loadPartImages(result.partDefinition.id);
        this.loadNonConformances();
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

  loadPartImages(partId: number): void {
    this.ncService.getImagesForPart(partId).subscribe({
      next: (imgs) => {
        this.partImages.set(imgs);
        if (imgs.length > 0) {
          this.selectedImage.set(imgs[0]);
        }
      },
      error: () => this.toast.show('Failed to load part images', 'error')
    });
  }

  loadNonConformances(): void {
    this.ncService.getNonConformances().subscribe({
      next: (list) => this.ncList.set(list),
      error: () => this.toast.show('Failed to load non-conformance types', 'error')
    });
  }

  selectImage(img: PartImage): void {
    this.selectedImage.set(img);
    this.selectedZoneId.set(null);
  }

  onImageLoad(img: HTMLImageElement, overlay: HTMLDivElement): void {
    // Sync overlay size with the actual displayed image size
    overlay.style.width = img.clientWidth + 'px';
    overlay.style.height = img.clientHeight + 'px';
  }

  selectZone(zoneId: number): void {
    this.selectedZoneId.set(zoneId);
    // Load existing notes for this zone if any
    const existing = this.findings().find(f => f.zoneId === zoneId);
    this.currentNotes.set(existing?.notes || '');
  }

  getSelectedZone(): any {
    const img = this.selectedImage();
    if (!img) return null;
    return img.zones.find(z => z.id === this.selectedZoneId());
  }

  getAvailableNCs(): NonConformance[] {
    const zone = this.getSelectedZone();
    if (!zone || !zone.nonConformanceIds) return [];
    
    // Filter the global NC list by the IDs allowed for this zone
    return this.ncList().filter(nc => zone.nonConformanceIds.includes(nc.id));
  }

  hasFinding(zoneId: number): boolean {
    return this.findings().some(f => f.zoneId === zoneId);
  }

  isNcSelected(ncId: number): boolean {
    const zoneId = this.selectedZoneId();
    if (!zoneId) return false;
    return this.findings().some(f => f.zoneId === zoneId && f.ncId === ncId);
  }

  toggleFinding(nc: NonConformance): void {
    const zone = this.getSelectedZone();
    if (!zone) return;

    const existingIdx = this.findings().findIndex(f => f.zoneId === zone.id && f.ncId === nc.id);
    if (existingIdx > -1) {
      // Remove
      this.findings.update(list => list.filter((_, i) => i !== existingIdx));
    } else {
      // Add
      this.findings.update(list => [...list, {
        zoneId: zone.id,
        zoneName: zone.name,
        ncId: nc.id,
        ncCode: nc.code,
        notes: this.currentNotes()
      }]);
    }
  }

  onNotesChange(notes: string): void {
    this.currentNotes.set(notes);
    const zoneId = this.selectedZoneId();
    if (!zoneId) return;

    // Update notes for ALL findings in this zone
    this.findings.update(list => list.map(f =>
      f.zoneId === zoneId ? { ...f, notes } : f
    ));
  }

  removeFinding(index: number): void {
    this.findings.update(list => list.filter((_, i) => i !== index));
  }

  submitInspection(): void {
    if (this.isSubmitting()) return;

    const findings = this.findings();
    const partImage = this.selectedImage();
    const serial = this.resolvedSerial();

    if (!partImage || !serial) {
      this.toast.show('Missing image or serial data', 'error');
      return;
    }

    if (findings.length === 0) {
      this.toast.show('Marking part as OK (no findings)', 'success');
      // For now we just reset, but we could record a "Conforming" result if needed.
      this.reset();
      return;
    }

    this.isSubmitting.set(true);

    const requests = findings.map(f => {
      const req: CreateInspectionResultRequest = {
        partImageId: partImage.id,
        imageZoneId: f.zoneId,
        nonConformanceId: f.ncId,
        partSerialId: serial.id,
        serialNumber: serial.serialNumber,
        notes: f.notes
      };
      return this.resultService.create(req);
    });

    forkJoin(requests).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.toast.show(`Successfully recorded ${findings.length} findings`, 'success');
        this.reset();
        this.router.navigate(['/inspection-results']);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.toast.show('Failed to submit some results', 'error');
        console.error('Submission error:', err);
      }
    });
  }

  imageSrc(img: PartImage | null): string {
    if (!img) return '';
    return this.ncService.toAbsoluteUrl(img.imageUrl);
  }

  reset(): void {
    this.serial.set('');
    this.resolvedSerial.set(null);
    this.resolvedPart.set(null);
    this.operations.set([]);
    this.selectedOperationId.set(null);
    this.partImages.set([]);
    this.selectedImage.set(null);
    this.selectedZoneId.set(null);
    this.findings.set([]);
    this.isReviewing.set(false);
    this.currentNotes.set('');
  }
}
