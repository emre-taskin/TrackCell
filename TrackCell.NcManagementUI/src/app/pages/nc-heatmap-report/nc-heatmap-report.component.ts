import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  HeatmapResponse,
  HeatmapZone,
  NonConformance,
  PartDefinition,
  PartImage
} from '../../models/nc.models';
import { InspectionResultService } from '../../services/inspection-result.service';
import { MasterDataService } from '../../services/master-data.service';
import { NcManagementService } from '../../services/nc-management.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-nc-heatmap-report',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './nc-heatmap-report.component.html',
  styleUrl: './nc-heatmap-report.component.css'
})
export class NcHeatmapReportComponent implements OnInit {
  private master = inject(MasterDataService);
  private nc = inject(NcManagementService);
  private inspections = inject(InspectionResultService);
  private toast = inject(ToastService);

  parts = signal<PartDefinition[]>([]);
  ncList = signal<NonConformance[]>([]);
  ncById = computed(() => {
    const m = new Map<number, NonConformance>();
    for (const n of this.ncList()) m.set(n.id, n);
    return m;
  });

  selectedPartId = signal<number | null>(null);
  images = signal<PartImage[]>([]);
  selectedImage = signal<PartImage | null>(null);
  selectedNcId = signal<number | null>(null);

  heatmap = signal<HeatmapResponse | null>(null);
  loading = signal(false);
  hoveredZoneId = signal<number | null>(null);

  ngOnInit(): void {
    this.master.getParts().subscribe({
      next: p => this.parts.set(p),
      error: () => this.toast.show('Failed to load parts', 'error')
    });
    this.nc.getNonConformances().subscribe({
      next: n => this.ncList.set(n),
      error: () => this.toast.show('Failed to load non-conformances', 'error')
    });
  }

  imageSrc(img: PartImage | null): string {
    return img ? this.nc.toAbsoluteUrl(img.imageUrl) : '';
  }

  onPartIdChange(value: number | null): void {
    this.selectedPartId.set(value);
    this.selectedImage.set(null);
    this.images.set([]);
    this.heatmap.set(null);
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
    this.loadHeatmap();
  }

  onImageIdChange(id: number | null): void {
    if (id == null) return;
    const img = this.images().find(i => i.id === id);
    this.selectImage(img);
  }

  onNcChange(value: number | null): void {
    this.selectedNcId.set(value);
    this.loadHeatmap();
  }

  private loadHeatmap(): void {
    const img = this.selectedImage();
    if (!img) {
      this.heatmap.set(null);
      return;
    }
    this.loading.set(true);
    this.inspections.getHeatmap(img.id, this.selectedNcId()).subscribe({
      next: h => {
        this.heatmap.set(h);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toast.show('Failed to load heatmap', 'error');
      }
    });
  }

  /** 0..1 intensity used for coloring. */
  intensity(zone: HeatmapZone): number {
    const max = this.heatmap()?.maxCount ?? 0;
    if (max <= 0) return 0;
    return zone.count / max;
  }

  /** Heatmap fill color: green -> yellow -> red as count grows. */
  zoneFill(zone: HeatmapZone): string {
    if (zone.count === 0) return 'rgba(148, 163, 184, 0.18)';
    const t = this.intensity(zone);
    let r: number, g: number, b: number;
    if (t < 0.5) {
      const k = t / 0.5;
      r = Math.round(16 + (245 - 16) * k);
      g = Math.round(185 + (158 - 185) * k);
      b = Math.round(129 + (11 - 129) * k);
    } else {
      const k = (t - 0.5) / 0.5;
      r = Math.round(245 + (239 - 245) * k);
      g = Math.round(158 + (68 - 158) * k);
      b = Math.round(11 + (68 - 11) * k);
    }
    const alpha = 0.35 + 0.45 * t;
    return `rgba(${r}, ${g}, ${b}, ${alpha})`;
  }

  zoneBorder(zone: HeatmapZone): string {
    if (zone.count === 0) return 'rgba(148, 163, 184, 0.5)';
    const t = this.intensity(zone);
    if (t < 0.5) return '#10b981';
    if (t < 0.85) return '#f59e0b';
    return '#ef4444';
  }

  topNcsForZone(zone: HeatmapZone, limit = 3): { ncId: number; code: string; description: string; count: number }[] {
    const entries = Object.entries(zone.countsByNonConformance ?? {})
      .map(([k, v]) => ({ ncId: Number(k), count: v as number }))
      .sort((a, b) => b.count - a.count)
      .slice(0, limit);
    const m = this.ncById();
    return entries.map(e => ({
      ncId: e.ncId,
      code: m.get(e.ncId)?.code ?? `#${e.ncId}`,
      description: m.get(e.ncId)?.description ?? '',
      count: e.count
    }));
  }

  selectedNcLabel(): string {
    const id = this.selectedNcId();
    if (id == null) return 'All non-conformances';
    const nc = this.ncById().get(id);
    return nc ? `${nc.code} — ${nc.description}` : 'Selected NC';
  }

  trackImage = (_: number, i: PartImage) => i.id;
  trackZone = (_: number, z: HeatmapZone) => z.zoneId;
  trackNc = (_: number, n: NonConformance) => n.id;
}
