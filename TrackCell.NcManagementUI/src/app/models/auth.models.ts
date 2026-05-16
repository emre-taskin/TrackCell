export type Permission =
  | 'view:dashboard'
  | 'view:part-editor'
  | 'edit:part-editor'
  | 'view:inspector'
  | 'view:nc-heatmap-report'
  | 'view:tickets'
  | 'view:admin'
  | 'manage:users'
  | 'manage:roles'
  | 'manage:parts'
  | 'manage:ncs'
  | 'manage:operations'
  | 'manage:serials';

export const ALL_PERMISSIONS: Permission[] = [
  'view:dashboard',
  'view:part-editor',
  'edit:part-editor',
  'view:inspector',
  'view:nc-heatmap-report',
  'view:tickets',
  'view:admin',
  'manage:users',
  'manage:roles',
  'manage:parts',
  'manage:ncs',
  'manage:operations'
];

export const PERMISSION_LABELS: Record<Permission, string> = {
  'view:dashboard': 'View Dashboard',
  'view:part-editor': 'View Part Editor',
  'edit:part-editor': 'Edit Part Editor',
  'view:inspector': 'View Inspection',
  'view:nc-heatmap-report': 'View NC Heatmap Report',
  'view:tickets': 'View Tickets',
  'view:admin': 'Access Admin Area',
  'manage:users': 'Manage Users',
  'manage:roles': 'Manage Roles',
  'manage:parts': 'Manage Parts',
  'manage:ncs': 'Manage NCs',
  'manage:operations': 'Manage Operations'
};

export interface Role {
  id: string;
  name: string;
  description: string;
  permissions: Permission[];
  builtIn?: boolean;
}

export interface User {
  id: string;
  username: string;
  displayName: string;
  email: string;
  roleIds: string[];
  createdAt: string;
}

export const DEFAULT_ROLES: Role[] = [
  {
    id: 'role-admin',
    name: 'Admin',
    description: 'Full access to all features, including user and role management.',
    permissions: [...ALL_PERMISSIONS],
    builtIn: true
  },
  {
    id: 'role-user',
    name: 'User',
    description: 'Default role assigned on first launch. Can view dashboard and part editor (read-only).',
    permissions: ['view:dashboard', 'view:part-editor'],
    builtIn: true
  }
];

export const DEFAULT_USER_ROLE_ID = 'role-user';
export const ADMIN_ROLE_ID = 'role-admin';
