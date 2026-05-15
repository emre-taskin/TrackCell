import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'part-editor'
  },
  {
    path: 'part-editor',
    loadComponent: () =>
      import('./pages/part-editor/part-editor.component').then(m => m.PartEditorComponent),
    data: { title: 'Part Editor' }
  },
  {
    path: 'inspector',
    loadComponent: () =>
      import('./pages/inspector/inspector.component').then(m => m.InspectorComponent),
    data: { title: 'Inspector' }
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent),
    data: { title: 'Reporting Dashboard' }
  },
  {
    path: 'tickets',
    loadComponent: () =>
      import('./pages/tickets/tickets.component').then(m => m.TicketsComponent),
    data: { title: 'Tickets' }
  },
  { path: '**', redirectTo: 'part-editor' }
];
