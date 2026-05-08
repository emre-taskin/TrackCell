import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./pages/operator-entry/operator-entry.component').then(m => m.OperatorEntryComponent),
    data: { title: 'Operator Entry' }
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent),
    data: { title: 'Kaizen Kanban Dashboard', fullscreen: true }
  },
  {
    path: 'factory-3d',
    loadComponent: () =>
      import('./pages/factory-3d/factory-3d.component').then(m => m.Factory3dComponent),
    data: { title: '3D Factory Layout', fullscreen: true }
  },
  { path: '**', redirectTo: '' }
];
