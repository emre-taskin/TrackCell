import { CommonModule } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  NgZone,
  OnDestroy,
  ViewChild,
  inject,
  signal
} from '@angular/core';
import * as THREE from 'three';
import { Subscription } from 'rxjs';
import { WorkItem } from '../../models/track-cell.models';
import { ConnectionStatus, DashboardHubService } from '../../services/dashboard-hub.service';
import { MasterDataService } from '../../services/master-data.service';
import { ToastService } from '../../services/toast.service';
import { OperationHistoryService } from '../../services/operation-history.service';

interface MachineParts {
  base: THREE.Mesh;
  baseMat: THREE.MeshStandardMaterial;
  column: THREE.Mesh;
  workpiece: THREE.Mesh;
  windowMat: THREE.MeshStandardMaterial;
  stackLight: THREE.Mesh;
  lightMat: THREE.MeshStandardMaterial;
  screen: THREE.Mesh;
  screenMat: THREE.MeshStandardMaterial;
  infoSprite: THREE.Sprite;
  infoMat: THREE.SpriteMaterial;
}

interface Machine {
  id: number;
  name: string;
  group: THREE.Group;
  parts: MachineParts;
  items: WorkItem[];
  count: number;
  state?: string;
}

interface Classification {
  state: 'IDLE' | 'ACTIVE' | 'PAUSED';
  color: number;
  hex: string;
}

const COLOR_ACTIVE = 0x10b981;
const COLOR_IDLE = 0xf59e0b;
const COLOR_PAUSED = 0xef4444;
const COLOR_ACTIVE_HEX = '#10b981';
const COLOR_IDLE_HEX = '#f59e0b';
const COLOR_PAUSED_HEX = '#ef4444';
const PAUSE_THRESHOLD_MS = 2 * 60 * 60 * 1000;

@Component({
  selector: 'app-factory-3d',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './factory-3d.component.html',
  styleUrl: './factory-3d.component.css'
})
export class Factory3dComponent implements AfterViewInit, OnDestroy {
  private workItems = inject(OperationHistoryService);
  private masterData = inject(MasterDataService);
  private hub = inject(DashboardHubService);
  private toast = inject(ToastService);
  private zone = inject(NgZone);

  @ViewChild('factoryRoot') factoryRoot!: ElementRef<HTMLDivElement>;
  @ViewChild('factoryCanvas') factoryCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('hoverPanel') hoverPanel!: ElementRef<HTMLDivElement>;

  lastUpdated = signal('-');
  connText = signal('Connecting...');
  connColor = signal('#f59e0b');

  private scene!: THREE.Scene;
  private camera!: THREE.PerspectiveCamera;
  private renderer!: THREE.WebGLRenderer;
  private machines: Machine[] = [];
  private operatorMap: Record<string, string> = {};

  private rafId = 0;
  private clock = new THREE.Clock();
  private intervalId?: ReturnType<typeof setInterval>;
  private hubUpdateSub?: Subscription;
  private hubStatusSub?: Subscription;

  // camera controls
  private target = new THREE.Vector3(0, 2, 0);
  private isDragging = false;
  private lastX = 0;
  private lastY = 0;
  private yaw = 0;
  private pitch = -0.5;
  private distance = 60;

  // hit testing
  private raycaster = new THREE.Raycaster();
  private mouse = new THREE.Vector2();

  ngAfterViewInit(): void {
    this.zone.runOutsideAngular(() => {
      this.initScene();
      this.buildMachines();
      this.attachInteractions();
      this.animate();
    });
    this.fetchOperators().then(() => this.refresh());
    this.hub.start();
    this.hubUpdateSub = this.hub.onUpdate.subscribe(async () => {
      await this.refresh();
      this.toast.show('Live update received', 'success');
    });
    this.hubStatusSub = this.hub.onStatus.subscribe(s => this.applyStatus(s));

    this.intervalId = setInterval(() => {
      this.machines.forEach(m => {
        if (m.items.length > 0) {
          const r = this.makeInfoTexture(m);
          if (m.parts.infoMat.map) m.parts.infoMat.map.dispose();
          m.parts.infoMat.map = r.texture;
          m.parts.infoMat.needsUpdate = true;
        }
      });
    }, 1000);
  }

  ngOnDestroy(): void {
    cancelAnimationFrame(this.rafId);
    if (this.intervalId) clearInterval(this.intervalId);
    this.hubUpdateSub?.unsubscribe();
    this.hubStatusSub?.unsubscribe();
    window.removeEventListener('mouseup', this.onWindowMouseUp);
    window.removeEventListener('mousemove', this.onWindowMouseMove);
    window.removeEventListener('resize', this.onResize);
    this.renderer?.dispose();
  }

  // ---------- status indicator ----------
  private applyStatus(s: ConnectionStatus): void {
    this.zone.run(() => {
      switch (s) {
        case 'connecting':
          this.connColor.set('#f59e0b');
          this.connText.set('Connecting...');
          break;
        case 'connected':
          this.connColor.set('#10b981');
          this.connText.set('Live');
          break;
        case 'reconnecting':
          this.connColor.set('#f59e0b');
          this.connText.set('Reconnecting...');
          break;
        case 'disconnected':
          this.connColor.set('#ef4444');
          this.connText.set('Disconnected');
          break;
      }
    });
  }

  // ---------- scene ----------
  private initScene(): void {
    const root = this.factoryRoot.nativeElement;
    const canvas = this.factoryCanvas.nativeElement;

    this.scene = new THREE.Scene();
    this.scene.background = new THREE.Color(0x0b1020);
    this.scene.fog = new THREE.Fog(0x0b1020, 60, 160);

    this.camera = new THREE.PerspectiveCamera(50, root.clientWidth / root.clientHeight, 0.1, 500);

    this.renderer = new THREE.WebGLRenderer({ canvas, antialias: true });
    this.renderer.setPixelRatio(window.devicePixelRatio);
    this.renderer.setSize(root.clientWidth, root.clientHeight, false);
    this.renderer.shadowMap.enabled = true;
    this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;

    const ambient = new THREE.AmbientLight(0xffffff, 0.4);
    this.scene.add(ambient);
    const dir = new THREE.DirectionalLight(0xffffff, 0.95);
    dir.position.set(25, 50, 20);
    dir.castShadow = true;
    dir.shadow.mapSize.set(2048, 2048);
    dir.shadow.camera.left = -50; dir.shadow.camera.right = 50;
    dir.shadow.camera.top = 50; dir.shadow.camera.bottom = -50;
    this.scene.add(dir);
    const hemi = new THREE.HemisphereLight(0x88aaff, 0x202030, 0.45);
    this.scene.add(hemi);

    // Floor
    const floorMat = new THREE.MeshStandardMaterial({ color: 0x1a223d, roughness: 0.9, metalness: 0.05 });
    const floor = new THREE.Mesh(new THREE.PlaneGeometry(110, 80), floorMat);
    floor.rotation.x = -Math.PI / 2;
    floor.receiveShadow = true;
    this.scene.add(floor);

    // Aisle lanes
    const aisleMat = new THREE.MeshStandardMaterial({ color: 0xf5c542, roughness: 0.7 });
    const aisle1 = new THREE.Mesh(new THREE.PlaneGeometry(90, 0.25), aisleMat);
    aisle1.rotation.x = -Math.PI / 2; aisle1.position.set(0, 0.02, -3);
    this.scene.add(aisle1);
    const aisle2 = new THREE.Mesh(new THREE.PlaneGeometry(90, 0.25), aisleMat);
    aisle2.rotation.x = -Math.PI / 2; aisle2.position.set(0, 0.02, 3);
    this.scene.add(aisle2);

    // Grid
    const grid = new THREE.GridHelper(110, 55, 0x3a4a78, 0x202a4a);
    grid.position.y = 0.01;
    this.scene.add(grid);

    // Walls
    const wallMat = new THREE.MeshStandardMaterial({ color: 0x131a30, roughness: 0.9, transparent: true, opacity: 0.55 });
    const wallBack = new THREE.Mesh(new THREE.PlaneGeometry(110, 14), wallMat);
    wallBack.position.set(0, 7, -40); this.scene.add(wallBack);
    const wallLeft = new THREE.Mesh(new THREE.PlaneGeometry(80, 14), wallMat);
    wallLeft.rotation.y = Math.PI / 2; wallLeft.position.set(-55, 7, 0); this.scene.add(wallLeft);
    const wallRight = new THREE.Mesh(new THREE.PlaneGeometry(80, 14), wallMat);
    wallRight.rotation.y = -Math.PI / 2; wallRight.position.set(55, 7, 0); this.scene.add(wallRight);

    this.applyCamera();
  }

  private classify(machine: Machine): Classification {
    if (machine.items.length === 0) {
      return { state: 'IDLE', color: COLOR_IDLE, hex: COLOR_IDLE_HEX };
    }
    const oldest = machine.items.reduce((a, b) =>
      new Date(a.createdAt) < new Date(b.createdAt) ? a : b
    );
    const ageMs = Date.now() - new Date(oldest.createdAt).getTime();
    if (ageMs > PAUSE_THRESHOLD_MS) {
      return { state: 'PAUSED', color: COLOR_PAUSED, hex: COLOR_PAUSED_HEX };
    }
    return { state: 'ACTIVE', color: COLOR_ACTIVE, hex: COLOR_ACTIVE_HEX };
  }

  private formatElapsed(ms: number): string {
    if (!isFinite(ms) || ms < 0) ms = 0;
    const totalSec = Math.floor(ms / 1000);
    const h = Math.floor(totalSec / 3600);
    const m = Math.floor((totalSec % 3600) / 60);
    const s = totalSec % 60;
    if (h > 0) return `${h}h ${String(m).padStart(2, '0')}m`;
    return `${String(m).padStart(2, '0')}m ${String(s).padStart(2, '0')}s`;
  }

  private makeLabelTexture(text: string, sub?: string): THREE.CanvasTexture {
    const canvasEl = document.createElement('canvas');
    canvasEl.width = 320; canvasEl.height = 96;
    const ctx = canvasEl.getContext('2d')!;
    ctx.fillStyle = 'rgba(15,23,42,0.92)';
    ctx.fillRect(0, 0, 320, 96);
    ctx.strokeStyle = '#3a4a78';
    ctx.lineWidth = 2;
    ctx.strokeRect(2, 2, 316, 92);
    ctx.font = 'bold 30px Outfit, Arial, sans-serif';
    ctx.fillStyle = '#ffffff';
    ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
    ctx.fillText(text, 160, sub ? 34 : 48);
    if (sub) {
      ctx.font = '20px Outfit, Arial, sans-serif';
      ctx.fillStyle = '#9ca3af';
      ctx.fillText(sub, 160, 66);
    }
    const tex = new THREE.CanvasTexture(canvasEl);
    tex.needsUpdate = true;
    return tex;
  }

  private makeInfoTexture(machine: Machine): { texture: THREE.CanvasTexture; color: number; hex: string; state: string } {
    const W = 512, H = 256;
    const canvasEl = document.createElement('canvas');
    canvasEl.width = W; canvasEl.height = H;
    const ctx = canvasEl.getContext('2d')!;

    const cls = this.classify(machine);
    const color = cls.hex;
    const label = cls.state;

    ctx.fillStyle = 'rgba(15,23,42,0.95)';
    ctx.fillRect(0, 0, W, H);
    ctx.strokeStyle = color;
    ctx.lineWidth = 4;
    ctx.strokeRect(2, 2, W - 4, H - 4);

    ctx.fillStyle = color;
    ctx.fillRect(2, 2, W - 4, 44);
    ctx.font = 'bold 24px Outfit, Arial, sans-serif';
    ctx.fillStyle = '#0b1020';
    ctx.textBaseline = 'middle';
    ctx.textAlign = 'left';
    ctx.fillText(machine.name, 16, 24);
    ctx.textAlign = 'right';
    ctx.fillText(label, W - 16, 24);

    ctx.textAlign = 'left';
    ctx.textBaseline = 'top';

    if (machine.items.length === 0) {
      ctx.fillStyle = '#9ca3af';
      ctx.font = '500 22px Outfit, Arial, sans-serif';
      ctx.fillText('No active operation', 18, 80);
      ctx.font = '500 18px Outfit, Arial, sans-serif';
      ctx.fillText('Awaiting work', 18, 120);
    } else {
      const item = machine.items.reduce((a, b) =>
        new Date(a.createdAt) < new Date(b.createdAt) ? a : b
      );
      const opName = this.operatorMap[item.badgeNumber] || item.badgeNumber;

      ctx.fillStyle = '#9ca3af';
      ctx.font = '500 14px Outfit, Arial, sans-serif';
      ctx.fillText('OPERATOR', 18, 58);
      ctx.fillStyle = '#ffffff';
      ctx.font = 'bold 22px Outfit, Arial, sans-serif';
      ctx.fillText(opName, 18, 78);

      ctx.fillStyle = '#9ca3af';
      ctx.font = '500 14px Outfit, Arial, sans-serif';
      ctx.fillText('PART', 18, 112);
      ctx.fillStyle = '#fbbf24';
      ctx.font = 'bold 22px Outfit, Arial, sans-serif';
      ctx.fillText(item.part, 18, 132);

      ctx.fillStyle = '#9ca3af';
      ctx.font = '500 14px Outfit, Arial, sans-serif';
      ctx.fillText('OP', 290, 112);
      ctx.fillStyle = '#ffffff';
      ctx.font = 'bold 22px Outfit, Arial, sans-serif';
      ctx.fillText(item.opNumber, 290, 132);

      ctx.fillStyle = '#9ca3af';
      ctx.font = '500 14px Outfit, Arial, sans-serif';
      ctx.fillText('SERIAL', 18, 166);
      ctx.fillStyle = '#ffffff';
      ctx.font = 'bold 22px Outfit, Arial, sans-serif';
      ctx.fillText(item.serial, 18, 186);

      const ageMs = Date.now() - new Date(item.createdAt).getTime();
      ctx.fillStyle = '#9ca3af';
      ctx.font = '500 14px Outfit, Arial, sans-serif';
      ctx.fillText('ELAPSED', 290, 166);
      ctx.fillStyle = color;
      ctx.font = 'bold 26px Outfit, Arial, sans-serif';
      ctx.fillText(this.formatElapsed(ageMs), 290, 186);

      if (machine.items.length > 1) {
        ctx.fillStyle = '#9ca3af';
        ctx.font = '500 13px Outfit, Arial, sans-serif';
        ctx.textAlign = 'right';
        ctx.fillText(`+${machine.items.length - 1} queued`, W - 16, 226);
        ctx.textAlign = 'left';
      }
    }

    const tex = new THREE.CanvasTexture(canvasEl);
    tex.needsUpdate = true;
    return { texture: tex, color: cls.color, hex: color, state: label };
  }

  private buildMachine(id: number, name: string): Machine {
    const g = new THREE.Group();
    const W = 5.0, H = 5.0, D = 4.5;

    const padMat = new THREE.MeshStandardMaterial({ color: 0x374151, roughness: 0.85 });
    const pad = new THREE.Mesh(new THREE.BoxGeometry(W + 0.6, 0.15, D + 0.6), padMat);
    pad.position.y = 0.075;
    pad.receiveShadow = true;
    g.add(pad);

    const bodyMat = new THREE.MeshStandardMaterial({ color: 0xe5e7eb, roughness: 0.55, metalness: 0.35 });
    const body = new THREE.Mesh(new THREE.BoxGeometry(W, H, D), bodyMat);
    body.position.set(0, 0.15 + H / 2, 0);
    body.castShadow = true; body.receiveShadow = true;
    g.add(body);

    const accentMat = new THREE.MeshStandardMaterial({ color: 0x1f2937, roughness: 0.6, metalness: 0.4 });
    const accent = new THREE.Mesh(new THREE.BoxGeometry(W + 0.02, 0.7, D + 0.02), accentMat);
    accent.position.set(0, 0.15 + H - 0.55, 0);
    g.add(accent);

    const doorMat = new THREE.MeshStandardMaterial({ color: 0xcbd5e1, roughness: 0.5, metalness: 0.4 });
    const door = new THREE.Mesh(new THREE.BoxGeometry(W * 0.7, H * 0.6, 0.08), doorMat);
    door.position.set(0, 0.15 + H * 0.45, D / 2 + 0.04);
    g.add(door);

    const windowMat = new THREE.MeshStandardMaterial({
      color: 0x1e3a5f, roughness: 0.15, metalness: 0.5,
      transparent: true, opacity: 0.55, emissive: 0x000000, emissiveIntensity: 0
    });
    const windowPane = new THREE.Mesh(new THREE.PlaneGeometry(W * 0.55, H * 0.32), windowMat);
    windowPane.position.set(0, 0.15 + H * 0.55, D / 2 + 0.09);
    g.add(windowPane);

    const frameMat = new THREE.MeshStandardMaterial({ color: 0x111827, roughness: 0.45, metalness: 0.6 });
    const frameTop = new THREE.Mesh(new THREE.BoxGeometry(W * 0.58, 0.08, 0.06), frameMat);
    frameTop.position.set(0, 0.15 + H * 0.55 + H * 0.16 + 0.04, D / 2 + 0.09);
    g.add(frameTop);
    const frameBot = new THREE.Mesh(new THREE.BoxGeometry(W * 0.58, 0.08, 0.06), frameMat);
    frameBot.position.set(0, 0.15 + H * 0.55 - H * 0.16 - 0.04, D / 2 + 0.09);
    g.add(frameBot);
    const frameL = new THREE.Mesh(new THREE.BoxGeometry(0.08, H * 0.32, 0.06), frameMat);
    frameL.position.set(-W * 0.275, 0.15 + H * 0.55, D / 2 + 0.09);
    g.add(frameL);
    const frameR = new THREE.Mesh(new THREE.BoxGeometry(0.08, H * 0.32, 0.06), frameMat);
    frameR.position.set(W * 0.275, 0.15 + H * 0.55, D / 2 + 0.09);
    g.add(frameR);

    const splitMat = new THREE.MeshBasicMaterial({ color: 0x374151 });
    const split = new THREE.Mesh(new THREE.PlaneGeometry(0.04, H * 0.6), splitMat);
    split.position.set(0, 0.15 + H * 0.45, D / 2 + 0.085);
    g.add(split);

    const handleMat = new THREE.MeshStandardMaterial({ color: 0x111827, metalness: 0.7, roughness: 0.3 });
    const handle = new THREE.Mesh(new THREE.BoxGeometry(0.5, 0.12, 0.12), handleMat);
    handle.position.set(W * 0.18, 0.15 + H * 0.3, D / 2 + 0.12);
    g.add(handle);

    const wpMat = new THREE.MeshStandardMaterial({ color: 0xfbbf24, roughness: 0.45, metalness: 0.6, emissive: 0xfbbf24, emissiveIntensity: 0.15 });
    const workpiece = new THREE.Mesh(new THREE.CylinderGeometry(0.45, 0.5, 0.9, 16), wpMat);
    workpiece.position.set(0, 0.15 + H * 0.45, D / 2 - 0.5);
    workpiece.visible = false;
    g.add(workpiece);

    const pendMat = new THREE.MeshStandardMaterial({ color: 0x111827, roughness: 0.4, metalness: 0.5 });
    const pendantArm = new THREE.Mesh(new THREE.BoxGeometry(0.15, 0.15, 0.8), pendMat);
    pendantArm.position.set(W / 2 + 0.45, 0.15 + H * 0.6, D / 2 - 0.25);
    g.add(pendantArm);
    const pendant = new THREE.Mesh(new THREE.BoxGeometry(0.9, 1.1, 0.25), pendMat);
    pendant.position.set(W / 2 + 0.85, 0.15 + H * 0.6, D / 2 - 0.25);
    pendant.castShadow = true;
    g.add(pendant);
    const screenMat = new THREE.MeshStandardMaterial({ color: 0x10b981, emissive: 0x10b981, emissiveIntensity: 0.7 });
    const screen = new THREE.Mesh(new THREE.PlaneGeometry(0.7, 0.55), screenMat);
    screen.position.set(W / 2 + 0.985, 0.15 + H * 0.65, D / 2 - 0.25);
    screen.rotation.y = Math.PI / 2;
    g.add(screen);

    const auxMat = new THREE.MeshStandardMaterial({ color: 0xb8bcc4, roughness: 0.6, metalness: 0.3 });
    const aux = new THREE.Mesh(new THREE.BoxGeometry(1.2, 1.6, 1.0), auxMat);
    aux.position.set(-W / 2 - 0.65, 0.15 + 0.8, -D / 2 + 0.5);
    aux.castShadow = true;
    g.add(aux);

    const stackMastMat = new THREE.MeshStandardMaterial({ color: 0x4b5563, roughness: 0.5, metalness: 0.4 });
    const mast = new THREE.Mesh(new THREE.CylinderGeometry(0.07, 0.07, 0.6, 12), stackMastMat);
    mast.position.set(W / 2 - 0.5, 0.15 + H + 0.3, -D / 2 + 0.5);
    g.add(mast);

    const lightMat = new THREE.MeshStandardMaterial({
      color: 0x10b981, emissive: 0x10b981, emissiveIntensity: 1.0,
      transparent: true, opacity: 0.95
    });
    const stackLight = new THREE.Mesh(new THREE.CylinderGeometry(0.18, 0.18, 0.55, 16), lightMat);
    stackLight.position.set(W / 2 - 0.5, 0.15 + H + 0.85, -D / 2 + 0.5);
    g.add(stackLight);
    const stackTop = new THREE.Mesh(new THREE.SphereGeometry(0.18, 16, 8, 0, Math.PI * 2, 0, Math.PI / 2), lightMat);
    stackTop.position.set(W / 2 - 0.5, 0.15 + H + 1.13, -D / 2 + 0.5);
    g.add(stackTop);

    const ventMat = new THREE.MeshStandardMaterial({ color: 0x1f2937, roughness: 0.7 });
    const vent = new THREE.Mesh(new THREE.PlaneGeometry(W * 0.5, H * 0.25), ventMat);
    vent.position.set(0, 0.15 + H * 0.35, -D / 2 - 0.01);
    vent.rotation.y = Math.PI;
    g.add(vent);

    const brandTex = this.makeLabelTexture('HNK', 'NET75');
    const brandMat = new THREE.MeshBasicMaterial({ map: brandTex, transparent: true });
    const brand = new THREE.Mesh(new THREE.PlaneGeometry(2.2, 0.75), brandMat);
    brand.position.set(0, 0.15 + H - 0.55, D / 2 + 0.06);
    g.add(brand);

    const infoMat = new THREE.SpriteMaterial({ transparent: true, depthTest: false });
    const infoSprite = new THREE.Sprite(infoMat);
    infoSprite.position.set(0, 0.15 + H + 2.6, 0);
    infoSprite.scale.set(6.0, 3.0, 1);
    infoSprite.renderOrder = 999;
    g.add(infoSprite);

    return {
      id, name, group: g,
      parts: {
        base: body, baseMat: bodyMat,
        column: door,
        workpiece, windowMat,
        stackLight, lightMat,
        screen, screenMat,
        infoSprite, infoMat
      },
      items: [],
      count: 0
    };
  }

  private setMachineStatus(m: Machine): void {
    m.count = m.items.length;
    const cls = this.classify(m);
    const color = cls.color;

    m.parts.lightMat.color.setHex(color);
    m.parts.lightMat.emissive.setHex(color);
    m.parts.screenMat.color.setHex(color);
    m.parts.screenMat.emissive.setHex(color);

    if (cls.state === 'ACTIVE') {
      m.parts.windowMat.emissive.setHex(0xfbbf24);
      m.parts.windowMat.emissiveIntensity = 0.4;
    } else if (cls.state === 'PAUSED') {
      m.parts.windowMat.emissive.setHex(0xef4444);
      m.parts.windowMat.emissiveIntensity = 0.25;
    } else {
      m.parts.windowMat.emissive.setHex(0x000000);
      m.parts.windowMat.emissiveIntensity = 0;
    }

    if (cls.state !== 'IDLE') {
      m.parts.baseMat.emissive.setHex(color);
      m.parts.baseMat.emissiveIntensity = 0.04;
    } else {
      m.parts.baseMat.emissive.setHex(0x000000);
      m.parts.baseMat.emissiveIntensity = 0;
    }

    m.parts.workpiece.visible = cls.state === 'ACTIVE';

    const result = this.makeInfoTexture(m);
    if (m.parts.infoMat.map) m.parts.infoMat.map.dispose();
    m.parts.infoMat.map = result.texture;
    m.parts.infoMat.needsUpdate = true;

    m.state = cls.state;
  }

  private buildMachines(): void {
    const machinesGroup = new THREE.Group();
    this.scene.add(machinesGroup);

    const ROWS = 2;
    const COLS = 6;
    const SPACING_X = 8.5;
    const ROW_Z = 14;

    for (let r = 0; r < ROWS; r++) {
      for (let c = 0; c < COLS; c++) {
        const idx = r * COLS + c;
        const id = `NET75-${String(idx + 1).padStart(2, '0')}`;
        const m = this.buildMachine(idx + 1, id);
        const x = -((COLS - 1) * SPACING_X) / 2 + c * SPACING_X;
        const z = r === 0 ? -ROW_Z : ROW_Z;
        m.group.position.set(x, 0, z);
        m.group.rotation.y = r === 0 ? 0 : Math.PI;
        machinesGroup.add(m.group);
        this.machines.push(m);
      }
    }

    this.machines.forEach(m => this.setMachineStatus(m));
  }

  // ---------- data ----------
  private async fetchOperators(): Promise<void> {
    try {
      const ops = await new Promise<{ badgeNumber: string; name: string }[]>((resolve, reject) => {
        this.masterData.getOperators().subscribe({ next: resolve, error: reject });
      });
      this.operatorMap = {};
      ops.forEach(o => { this.operatorMap[o.badgeNumber] = o.name; });
    } catch (e) {
      console.warn('operators fetch failed', e);
    }
  }

  private async fetchActiveItems(): Promise<WorkItem[]> {
    try {
      return await new Promise<WorkItem[]>((resolve, reject) => {
        this.workItems.getInProgress().subscribe({ next: resolve, error: reject });
      });
    } catch (e) {
      console.warn('active fetch failed', e);
      return [];
    }
  }

  private distributeItems(items: WorkItem[]): void {
    this.machines.forEach(m => { m.items = []; });

    const opMap: Record<string, WorkItem[]> = {};
    items.forEach(it => {
      (opMap[it.opNumber] ??= []).push(it);
    });

    const opNumbers = Object.keys(opMap).sort();
    opNumbers.forEach((op, i) => {
      const machineIdx = i % this.machines.length;
      opMap[op].forEach(it => this.machines[machineIdx].items.push(it));
    });

    this.machines.forEach(m => this.setMachineStatus(m));

    this.zone.run(() => this.lastUpdated.set(new Date().toLocaleTimeString()));
  }

  private async refresh(): Promise<void> {
    const items = await this.fetchActiveItems();
    this.distributeItems(items);
  }

  // ---------- camera ----------
  private applyCamera(): void {
    const cx = this.target.x + this.distance * Math.cos(this.pitch) * Math.sin(this.yaw);
    const cy = this.target.y + this.distance * Math.sin(-this.pitch);
    const cz = this.target.z + this.distance * Math.cos(this.pitch) * Math.cos(this.yaw);
    this.camera.position.set(cx, Math.max(2, cy), cz);
    this.camera.lookAt(this.target);
  }

  private onCanvasMouseDown = (e: MouseEvent) => {
    this.isDragging = true;
    this.lastX = e.clientX;
    this.lastY = e.clientY;
  };

  private onWindowMouseUp = () => { this.isDragging = false; };

  private onWindowMouseMove = (e: MouseEvent) => {
    if (!this.isDragging) return;
    const dx = e.clientX - this.lastX;
    const dy = e.clientY - this.lastY;
    this.lastX = e.clientX; this.lastY = e.clientY;
    this.yaw -= dx * 0.005;
    this.pitch = Math.max(-1.4, Math.min(-0.05, this.pitch + dy * 0.005));
    this.applyCamera();
  };

  private onCanvasWheel = (e: WheelEvent) => {
    e.preventDefault();
    this.distance = Math.max(20, Math.min(120, this.distance + e.deltaY * 0.05));
    this.applyCamera();
  };

  private onCanvasMouseMoveHover = (e: MouseEvent) => {
    const canvas = this.factoryCanvas.nativeElement;
    const rect = canvas.getBoundingClientRect();
    this.mouse.x = ((e.clientX - rect.left) / rect.width) * 2 - 1;
    this.mouse.y = -((e.clientY - rect.top) / rect.height) * 2 + 1;

    this.raycaster.setFromCamera(this.mouse, this.camera);
    const targets: THREE.Object3D[] = [];
    this.machines.forEach(m => { targets.push(m.parts.base, m.parts.column); });
    const hits = this.raycaster.intersectObjects(targets, false);
    const panel = this.hoverPanel.nativeElement;
    if (hits.length > 0) {
      const obj = hits[0].object;
      let machine: Machine | null = null;
      for (const m of this.machines) {
        if (m.parts.base === obj || m.parts.column === obj) { machine = m; break; }
      }
      if (machine) {
        panel.style.display = 'block';
        panel.style.left = (e.clientX - rect.left + 12) + 'px';
        panel.style.top = (e.clientY - rect.top + 12) + 'px';
        let html = `<div style="font-weight:600;margin-bottom:4px;">${machine.name}</div>`;
        html += `<div style="opacity:0.7;font-size:0.8rem;margin-bottom:6px;">HNK NET75 Vertical Lathe</div>`;
        html += `<div>WIP: <strong>${machine.count}</strong></div>`;
        if (machine.items.length > 0) {
          html += `<div style="margin-top:6px;border-top:1px solid #2a334d;padding-top:6px;font-size:0.8rem;opacity:0.85;">`;
          machine.items.slice(0, 5).forEach(it => {
            html += `<div>Op ${it.opNumber} · ${it.part} · SN ${it.serial}</div>`;
          });
          if (machine.items.length > 5) html += `<div style="opacity:0.6;">+ ${machine.items.length - 5} more...</div>`;
          html += `</div>`;
        }
        panel.innerHTML = html;
        return;
      }
    }
    panel.style.display = 'none';
  };

  private onCanvasMouseLeave = () => {
    this.hoverPanel.nativeElement.style.display = 'none';
  };

  private onResize = () => {
    const root = this.factoryRoot.nativeElement;
    const w = root.clientWidth, h = root.clientHeight;
    this.camera.aspect = w / h;
    this.camera.updateProjectionMatrix();
    this.renderer.setSize(w, h, false);
  };

  private attachInteractions(): void {
    const canvas = this.factoryCanvas.nativeElement;
    canvas.addEventListener('mousedown', this.onCanvasMouseDown);
    canvas.addEventListener('wheel', this.onCanvasWheel, { passive: false });
    canvas.addEventListener('mousemove', this.onCanvasMouseMoveHover);
    canvas.addEventListener('mouseleave', this.onCanvasMouseLeave);
    window.addEventListener('mouseup', this.onWindowMouseUp);
    window.addEventListener('mousemove', this.onWindowMouseMove);
    window.addEventListener('resize', this.onResize);
    this.onResize();
  }

  private animate = () => {
    const t = this.clock.getElapsedTime();
    this.machines.forEach((m, i) => {
      if (m.count > 0) {
        m.parts.workpiece.rotation.y += 0.05;
        m.parts.lightMat.emissiveIntensity = 0.65 + 0.35 * Math.sin(t * 3 + i);
        m.parts.windowMat.emissiveIntensity = 0.3 + 0.15 * Math.sin(t * 4 + i);
      } else {
        m.parts.lightMat.emissiveIntensity = 0.3;
      }
    });
    this.renderer.render(this.scene, this.camera);
    this.rafId = requestAnimationFrame(this.animate);
  };
}
