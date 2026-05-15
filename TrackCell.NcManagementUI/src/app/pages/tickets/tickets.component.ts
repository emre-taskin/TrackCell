import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';

export type TicketState = 'Open' | 'InProgress' | 'Resolved' | 'Closed';

const STATES: TicketState[] = ['Open', 'InProgress', 'Resolved', 'Closed'];

@Component({
  selector: 'app-tickets',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tickets.component.html',
  styleUrl: './tickets.component.css'
})
export class TicketsComponent {
  readonly states = STATES;
  selectedState = signal<TicketState>('Open');

  selectState(s: TicketState): void {
    this.selectedState.set(s);
  }
}
