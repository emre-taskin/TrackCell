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
    data: { title: 'Inspection' }
  },
  {
    path: 'nc-heatmap-report',
    loadComponent: () =>
      import('./pages/nc-heatmap-report/nc-heatmap-report.component').then(m => m.NcHeatmapReportComponent),
    canActivate: [permissionGuard('view:nc-heatmap-report')],
    data: { title: 'NC Heatmap Report' }
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
  {
    path: 'admin/parts',
    loadComponent: () =>
      import('./pages/parts-manager/parts-manager').then(m => m.PartsManagerComponent),
    canActivate: [permissionGuard('manage:parts')],
    data: { title: 'Part Manager' }
  },
  {
    path: 'admin/ncs',
    loadComponent: () =>
      import('./pages/ncs-manager/ncs-manager').then(m => m.NcsManagerComponent),
    canActivate: [permissionGuard('manage:ncs')],
    data: { title: 'Non-Conformances Manager' }
  },
  {
    path: 'admin/operations',
    loadComponent: () =>
      import('./pages/operations-manager/operations-manager').then(m => m.OperationsManagerComponent),
    canActivate: [permissionGuard('manage:operations')],
    data: { title: 'Operations Manager' }
  },
  {
    path: 'admin/serials',
    loadComponent: () =>
      import('./pages/serials-manager/serials-manager').then(m => m.SerialsManagerComponent),
    canActivate: [permissionGuard('manage:serials')],
    data: { title: 'Serials Manager' }
  },
  { path: '**', redirectTo: 'dashboard' }
];
