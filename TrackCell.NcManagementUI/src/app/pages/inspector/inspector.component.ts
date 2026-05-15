import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-inspector',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inspector.component.html',
  styleUrl: './inspector.component.css'
})
export class InspectorComponent {
  serial = signal('');
  resolvedSerial = signal<string | null>(null);

  lookup(): void {
    const s = this.serial().trim();
    if (!s) return;
    this.resolvedSerial.set(s);
  }

  reset(): void {
    this.serial.set('');
    this.resolvedSerial.set(null);
  }
}
