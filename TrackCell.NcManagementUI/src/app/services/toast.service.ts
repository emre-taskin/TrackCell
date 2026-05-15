import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error';

export interface Toast {
  id: number;
  message: string;
  type: ToastType;
  leaving: boolean;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  readonly toasts = signal<Toast[]>([]);

  show(message: string, type: ToastType = 'success'): void {
    const toast: Toast = { id: this.nextId++, message, type, leaving: false };
    this.toasts.update(list => [...list, toast]);
    setTimeout(() => this.dismiss(toast.id), 3000);
  }

  private dismiss(id: number): void {
    this.toasts.update(list => list.map(t => (t.id === id ? { ...t, leaving: true } : t)));
    setTimeout(() => {
      this.toasts.update(list => list.filter(t => t.id !== id));
    }, 300);
  }
}
