import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ImageZone,
  InspectionResult,
  NonConformance,
  PartDefinition,
  PartImage
} from '../../models/nc.models';
import { InspectionResultService } from '../../services/inspection-result.service';
import { NcManagementService } from '../../services/nc-management.service';
import { PartService } from '../../services/part.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-inspection-results',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inspection-results.component.html',
  styleUrl: './inspection-results.component.css'
})
export class InspectionResultsComponent implements OnInit {
  private partService = inject(PartService);
  private nc = inject(NcManagementService);
  private inspections = inject(InspectionResultService);
  private toast = inject(ToastService);

  parts = signal<PartDefinition[]>([]);
  ncList = signal<NonConformance[]>([]);

  selectedPartId = signal<number | null>(null);
  images = signal<PartImage[]>([]);
  selectedImage = signal<PartImage | null>(null);

  results = signal<InspectionResult[]>([]);
  loading = signal(false);

  filterZoneId = signal<number | null>(null);
  filterNcId = signal<number | null>(null);
  searchText = signal('');

  ncById = computed(() => {
    const m = new Map<number, NonConformance>();
    for (const n of this.ncList()) m.set(n.id, n);
    return m;
  });

  zoneById = computed(() => {
    const m = new Map<number, ImageZone>();
    for (const z of this.selectedImage()?.zones ?? []) m.set(z.id, z);
    return m;
  });

  filteredResults = computed(() => {
    const z = this.filterZoneId();
    const n = this.filterNcId();
    const q = this.searchText().trim().toLowerCase();
    return this.results().filter(r => {
      if (z !== null && r.imageZoneId !== z) return false;
      if (n !== null && r.nonConformanceId !== n) return false;
      if (q) {
        const serial = (r.serialNumber ?? '').toLowerCase();
        const notes = (r.notes ?? '').toLowerCase();
        if (!serial.includes(q) && !notes.includes(q)) return false;
      }
      return true;
    });
  });

  summary = computed(() => {
    const filtered = this.filteredResults();
    const byNc = new Map<number, number>();
    const byZone = new Map<number, number>();
    for (const r of filtered) {
      byNc.set(r.nonConformanceId, (byNc.get(r.nonConformanceId) ?? 0) + 1);
      byZone.set(r.imageZoneId, (byZone.get(r.imageZoneId) ?? 0) + 1);
    }
    return {
      total: filtered.length,
      uniqueZones: byZone.size,
      uniqueNcs: byNc.size
    };
  });

  ngOnInit(): void {
    this.partService.getParts().subscribe({
      next: p => this.parts.set(p),
      error: () => this.toast.show('Failed to load parts', 'error')
    });
    this.nc.getNonConformances().subscribe({
      next: n => this.ncList.set(n),
      error: () => this.toast.show('Failed to load non-conformances', 'error')
    });
  }

  onPartIdChange(value: number | null): void {
    this.selectedPartId.set(value);
    this.selectedImage.set(null);
    this.images.set([]);
    this.results.set([]);
    this.filterZoneId.set(null);
    if (!value) return;
    this.nc.getImagesForPart(value).subscribe({
      next: imgs => {
        this.images.set(imgs);
        if (imgs.length) this.selectImage(imgs[0]);
      },
      error: () => this.toast.show('Failed to load images', 'error')
    });
  }

  selectImage(img: PartImage | undefined | null): void {
    if (!img) return;
    this.selectedImage.set(img);
    this.filterZoneId.set(null);
    this.loadResults();
  }

  onImageIdChange(id: number | null): void {
    if (id == null) return;
    const img = this.images().find(i => i.id === id);
    this.selectImage(img);
  }

  refresh(): void {
    this.loadResults();
  }

  clearFilters(): void {
    this.filterZoneId.set(null);
    this.filterNcId.set(null);
    this.searchText.set('');
  }

  private loadResults(): void {
    const img = this.selectedImage();
    if (!img) {
      this.results.set([]);
      return;
    }
    this.loading.set(true);
    this.inspections.list(img.id).subscribe({
      next: r => {
        this.results.set(r);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toast.show('Failed to load inspection results', 'error');
      }
    });
  }

  zoneName(zoneId: number): string {
    return this.zoneById().get(zoneId)?.name ?? `Zone #${zoneId}`;
  }

  ncCode(ncId: number): string {
    return this.ncById().get(ncId)?.code ?? `#${ncId}`;
  }

  ncDescription(ncId: number): string {
    return this.ncById().get(ncId)?.description ?? '';
  }

  formatDate(iso: string): string {
    const d = new Date(iso);
    if (isNaN(d.getTime())) return iso;
    return d.toLocaleString();
  }

  trackResult = (_: number, r: InspectionResult) => r.id;
  trackZone = (_: number, z: ImageZone) => z.id;
  trackNc = (_: number, n: NonConformance) => n.id;
}
