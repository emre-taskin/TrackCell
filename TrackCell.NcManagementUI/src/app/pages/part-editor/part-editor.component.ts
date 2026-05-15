import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  ElementRef,
  OnInit,
  ViewChild,
  computed,
  inject,
  signal
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  NonConformance,
  PartDefinition,
  PartImage
} from '../../models/nc.models';
import { MasterDataService } from '../../services/master-data.service';
import { NcManagementService } from '../../services/nc-management.service';
import { ToastService } from '../../services/toast.service';

interface DraftZone {
  id: number | null;
  name: string;
  x: number;
  y: number;
  width: number;
  height: number;
  nonConformanceIds: number[];
}

type DragMode = 'create' | 'move' | 'resize' | null;

@Component({
  selector: 'app-part-editor',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './part-editor.component.html',
  styleUrl: './part-editor.component.css'
})
export class PartEditorComponent implements OnInit {
  private master = inject(MasterDataService);
  private nc = inject(NcManagementService);
  private toast = inject(ToastService);

  @ViewChild('imageEl') imageEl?: ElementRef<HTMLImageElement>;
  @ViewChild('overlayEl') overlayEl?: ElementRef<HTMLDivElement>;
  @ViewChild('fileInput') fileInput?: ElementRef<HTMLInputElement>;

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

  zones = signal<DraftZone[]>([]);
  selectedZoneIdx = signal<number | null>(null);
  dirty = signal(false);
  uploading = signal(false);
  saving = signal(false);

  newImageName = '';

  private dragMode: DragMode = null;
  private dragStartX = 0;
  private dragStartY = 0;
  private zoneStart: DraftZone | null = null;

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

  onPartIdChange(value: number | null): void {
    this.selectedPartId.set(value);
    this.onPartChange();
  }

  onPartChange(): void {
    const id = this.selectedPartId();
    this.selectedImage.set(null);
    this.zones.set([]);
    this.selectedZoneIdx.set(null);
    this.dirty.set(false);
    this.images.set([]);
    if (!id) return;
    this.nc.getImagesForPart(id).subscribe({
      next: imgs => {
        this.images.set(imgs);
        if (imgs.length) this.selectImage(imgs[0]);
      },
      error: () => this.toast.show('Failed to load images', 'error')
    });
  }

  selectImage(img: PartImage): void {
    if (this.dirty() && !confirm('Discard unsaved zone changes?')) return;
    this.selectedImage.set(img);
    this.zones.set(
      img.zones.map(z => ({
        id: z.id,
        name: z.name,
        x: z.x,
        y: z.y,
        width: z.width,
        height: z.height,
        nonConformanceIds: [...z.nonConformanceIds]
      }))
    );
    this.selectedZoneIdx.set(null);
    this.dirty.set(false);
  }

  imageSrc(img: PartImage | null): string {
    return img ? this.nc.toAbsoluteUrl(img.imageUrl) : '';
  }

  onFilePicked(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    const partId = this.selectedPartId();
    if (!partId) {
      this.toast.show('Select a part first', 'error');
      input.value = '';
      return;
    }
    const name = (this.newImageName || file.name).trim();
    this.uploading.set(true);
    this.nc.uploadImage(partId, name, file).subscribe({
      next: created => {
        this.uploading.set(false);
        this.images.update(list => [created, ...list]);
        this.selectImage(created);
        this.newImageName = '';
        if (this.fileInput) this.fileInput.nativeElement.value = '';
        this.toast.show('Image uploaded');
      },
      error: (e: HttpErrorResponse) => {
        this.uploading.set(false);
        const msg = typeof e.error === 'string' ? e.error : 'Upload failed';
        this.toast.show(msg, 'error');
      }
    });
  }

  deleteImage(img: PartImage, ev: Event): void {
    ev.stopPropagation();
    if (!confirm(`Delete image "${img.name}"? This removes its zones too.`)) return;
    this.nc.deleteImage(img.id).subscribe({
      next: () => {
        this.images.update(list => list.filter(i => i.id !== img.id));
        if (this.selectedImage()?.id === img.id) {
          const next = this.images()[0] ?? null;
          if (next) this.selectImage(next);
          else {
            this.selectedImage.set(null);
            this.zones.set([]);
            this.dirty.set(false);
          }
        }
        this.toast.show('Image deleted');
      },
      error: () => this.toast.show('Delete failed', 'error')
    });
  }

  onOverlayMouseDown(ev: MouseEvent): void {
    if (!this.selectedImage()) return;
    const rect = this.overlayEl!.nativeElement.getBoundingClientRect();
    const x = (ev.clientX - rect.left) / rect.width;
    const y = (ev.clientY - rect.top) / rect.height;

    const target = ev.target as HTMLElement;
    const handle = target.dataset['handle'];
    const zoneIdxAttr = target.dataset['zoneIdx'];

    if (handle && zoneIdxAttr !== undefined) {
      const idx = Number(zoneIdxAttr);
      this.selectedZoneIdx.set(idx);
      this.dragMode = handle === 'move' ? 'move' : 'resize';
      this.dragStartX = x;
      this.dragStartY = y;
      this.zoneStart = { ...this.zones()[idx], nonConformanceIds: [...this.zones()[idx].nonConformanceIds] };
      ev.preventDefault();
      return;
    }

    this.dragMode = 'create';
    this.dragStartX = x;
    this.dragStartY = y;
    const newZone: DraftZone = {
      id: null,
      name: `Zone ${this.zones().length + 1}`,
      x,
      y,
      width: 0,
      height: 0,
      nonConformanceIds: []
    };
    this.zones.update(list => [...list, newZone]);
    this.selectedZoneIdx.set(this.zones().length - 1);
    ev.preventDefault();
  }

  onOverlayMouseMove(ev: MouseEvent): void {
    if (!this.dragMode) return;
    const rect = this.overlayEl!.nativeElement.getBoundingClientRect();
    const x = clamp01((ev.clientX - rect.left) / rect.width);
    const y = clamp01((ev.clientY - rect.top) / rect.height);
    const idx = this.selectedZoneIdx();
    if (idx === null) return;

    if (this.dragMode === 'create') {
      const sx = this.dragStartX;
      const sy = this.dragStartY;
      const nx = Math.min(sx, x);
      const ny = Math.min(sy, y);
      const nw = Math.abs(x - sx);
      const nh = Math.abs(y - sy);
      this.zones.update(list => list.map((z, i) => i === idx ? { ...z, x: nx, y: ny, width: nw, height: nh } : z));
    } else if (this.dragMode === 'move' && this.zoneStart) {
      const dx = x - this.dragStartX;
      const dy = y - this.dragStartY;
      const z = this.zoneStart;
      const nx = clamp01(z.x + dx, 1 - z.width);
      const ny = clamp01(z.y + dy, 1 - z.height);
      this.zones.update(list => list.map((zz, i) => i === idx ? { ...zz, x: nx, y: ny } : zz));
    } else if (this.dragMode === 'resize' && this.zoneStart) {
      const z = this.zoneStart;
      const nw = clamp01(x - z.x, 1 - z.x);
      const nh = clamp01(y - z.y, 1 - z.y);
      this.zones.update(list => list.map((zz, i) => i === idx ? { ...zz, width: Math.max(0.01, nw), height: Math.max(0.01, nh) } : zz));
    }
  }

  onOverlayMouseUp(): void {
    if (this.dragMode === 'create') {
      const idx = this.selectedZoneIdx();
      if (idx !== null) {
        const z = this.zones()[idx];
        if (z.width < 0.01 || z.height < 0.01) {
          this.zones.update(list => list.filter((_, i) => i !== idx));
          this.selectedZoneIdx.set(null);
        } else {
          this.dirty.set(true);
        }
      }
    } else if (this.dragMode === 'move' || this.dragMode === 'resize') {
      this.dirty.set(true);
    }
    this.dragMode = null;
    this.zoneStart = null;
  }

  selectZone(idx: number, ev?: Event): void {
    ev?.stopPropagation();
    this.selectedZoneIdx.set(idx);
  }

  removeZone(idx: number): void {
    this.zones.update(list => list.filter((_, i) => i !== idx));
    this.selectedZoneIdx.set(null);
    this.dirty.set(true);
  }

  toggleNcOnSelected(ncId: number, ev: Event): void {
    const checked = (ev.target as HTMLInputElement).checked;
    const idx = this.selectedZoneIdx();
    if (idx === null) return;
    this.zones.update(list => list.map((z, i) => {
      if (i !== idx) return z;
      const set = new Set(z.nonConformanceIds);
      if (checked) set.add(ncId);
      else set.delete(ncId);
      return { ...z, nonConformanceIds: Array.from(set) };
    }));
    this.dirty.set(true);
  }

  onZoneNameInput(idx: number, ev: Event): void {
    const value = (ev.target as HTMLInputElement).value;
    this.zones.update(list => list.map((z, i) => i === idx ? { ...z, name: value } : z));
    this.dirty.set(true);
  }

  ncSelectedOnZone(ncId: number): boolean {
    const idx = this.selectedZoneIdx();
    if (idx === null) return false;
    return this.zones()[idx].nonConformanceIds.includes(ncId);
  }

  ncNamesForZone(z: DraftZone): string {
    if (!z.nonConformanceIds.length) return '— no NCs —';
    const m = this.ncById();
    return z.nonConformanceIds
      .map(id => m.get(id)?.description ?? `#${id}`)
      .join(', ');
  }

  saveZones(): void {
    const img = this.selectedImage();
    if (!img) return;
    const cleaned = this.zones().filter(z => z.width >= 0.01 && z.height >= 0.01);
    const body = {
      zones: cleaned.map(z => ({
        name: z.name.trim() || 'Zone',
        x: z.x,
        y: z.y,
        width: z.width,
        height: z.height,
        nonConformanceIds: z.nonConformanceIds
      }))
    };
    this.saving.set(true);
    this.nc.saveZones(img.id, body).subscribe({
      next: updated => {
        this.saving.set(false);
        this.images.update(list => list.map(i => i.id === updated.id ? updated : i));
        this.selectImage(updated);
        this.toast.show('Zones saved');
      },
      error: () => {
        this.saving.set(false);
        this.toast.show('Save failed', 'error');
      }
    });
  }

  trackZone = (_: number, z: DraftZone) => z.id ?? `new-${_}`;
  trackImage = (_: number, i: PartImage) => i.id;
  trackNc = (_: number, n: NonConformance) => n.id;
}

function clamp01(v: number, max: number = 1): number {
  if (v < 0) return 0;
  if (v > max) return max;
  return v;
}
