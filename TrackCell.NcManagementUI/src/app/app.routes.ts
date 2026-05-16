import { Routes } from '@angular/router';
import { permissionGuard } from './guards/permission.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'dashboard'
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [permissionGuard('view:dashboard')],
    data: { title: 'Reporting Dashboard' }
  },
  {
    path: 'part-editor',
    loadComponent: () =>
      import('./pages/part-editor/part-editor.component').then(m => m.PartEditorComponent),
    canActivate: [permissionGuard('view:part-editor')],
    data: { title: 'Part Editor' }
  },
  {
    path: 'inspector',
    loadComponent: () =>
      import('./pages/inspector/inspector.component').then(m => m.InspectorComponent),
    canActivate: [permissionGuard('view:inspector')],
    data: { title: 'Inspector' }
  },
  {
    path: 'tickets',
    loadComponent: () =>
      import('./pages/tickets/tickets.component').then(m => m.TicketsComponent),
    canActivate: [permissionGuard('view:tickets')],
    data: { title: 'Tickets' }
  },
  {
    path: 'admin',
    loadComponent: () =>
      import('./pages/admin/admin.component').then(m => m.AdminComponent),
    canActivate: [permissionGuard('view:admin')],
    data: { title: 'Admin' }
  },
  {
    path: 'admin/users',
    loadComponent: () =>
      import('./pages/users/users.component').then(m => m.UsersComponent),
    canActivate: [permissionGuard('manage:users')],
    data: { title: 'User Management' }
  },
  {
    path: 'admin/roles',
    loadComponent: () =>
      import('./pages/roles/roles.component').then(m => m.RolesComponent),
    canActivate: [permissionGuard('manage:roles')],
    data: { title: 'Roles & Permissions' }
  },
  { path: '**', redirectTo: 'dashboard' }
];
