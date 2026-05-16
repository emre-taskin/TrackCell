import { Injectable, computed, signal } from '@angular/core';
import {
  ADMIN_ROLE_ID,
  DEFAULT_ROLES,
  DEFAULT_USER_ROLE_ID,
  Permission,
  Role,
  User
} from '../models/auth.models';

const STORAGE_KEY = 'trackcell.nc.auth.v1';

interface AuthStorage {
  users: User[];
  roles: Role[];
  currentUserId: string | null;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _users = signal<User[]>([]);
  private readonly _roles = signal<Role[]>([]);
  private readonly _currentUserId = signal<string | null>(null);

  readonly users = this._users.asReadonly();
  readonly roles = this._roles.asReadonly();

  readonly currentUser = computed<User | null>(() => {
    const id = this._currentUserId();
    if (!id) return null;
    return this._users().find(u => u.id === id) ?? null;
  });

  readonly currentRoles = computed<Role[]>(() => {
    const user = this.currentUser();
    if (!user) return [];
    return this._roles().filter(r => user.roleIds.includes(r.id));
  });

  readonly currentPermissions = computed<Set<Permission>>(() => {
    const set = new Set<Permission>();
    for (const role of this.currentRoles()) {
      for (const p of role.permissions) set.add(p);
    }
    return set;
  });

  constructor() {
    this.load();
  }

  hasPermission(permission: Permission): boolean {
    return this.currentPermissions().has(permission);
  }

  hasAnyPermission(permissions: Permission[]): boolean {
    const set = this.currentPermissions();
    return permissions.some(p => set.has(p));
  }

  isAdmin(): boolean {
    const user = this.currentUser();
    return !!user && user.roleIds.includes(ADMIN_ROLE_ID);
  }

  // --- User management ---

  createUser(input: { username: string; displayName: string; email: string; roleIds: string[] }): User {
    const user: User = {
      id: `user-${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 6)}`,
      username: input.username.trim(),
      displayName: input.displayName.trim() || input.username.trim(),
      email: input.email.trim(),
      roleIds: input.roleIds.length ? [...input.roleIds] : [DEFAULT_USER_ROLE_ID],
      createdAt: new Date().toISOString()
    };
    this._users.update(list => [...list, user]);
    this.persist();
    return user;
  }

  updateUser(id: string, patch: Partial<Omit<User, 'id' | 'createdAt'>>): void {
    this._users.update(list =>
      list.map(u => (u.id === id ? { ...u, ...patch } : u))
    );
    this.persist();
  }

  deleteUser(id: string): void {
    if (id === this._currentUserId()) {
      throw new Error('Cannot delete the currently active user.');
    }
    this._users.update(list => list.filter(u => u.id !== id));
    this.persist();
  }

  setCurrentUser(id: string): void {
    if (!this._users().some(u => u.id === id)) return;
    this._currentUserId.set(id);
    this.persist();
  }

  // --- Role management ---

  createRole(input: { name: string; description: string; permissions: Permission[] }): Role {
    const role: Role = {
      id: `role-${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 6)}`,
      name: input.name.trim(),
      description: input.description.trim(),
      permissions: [...input.permissions]
    };
    this._roles.update(list => [...list, role]);
    this.persist();
    return role;
  }

  updateRole(id: string, patch: Partial<Omit<Role, 'id' | 'builtIn'>>): void {
    this._roles.update(list =>
      list.map(r => {
        if (r.id !== id) return r;
        // Built-in roles: only description can be edited freely; name/permissions are locked.
        if (r.builtIn) {
          return { ...r, description: patch.description ?? r.description };
        }
        return { ...r, ...patch };
      })
    );
    this.persist();
  }

  deleteRole(id: string): void {
    const role = this._roles().find(r => r.id === id);
    if (!role || role.builtIn) {
      throw new Error('Built-in roles cannot be deleted.');
    }
    this._roles.update(list => list.filter(r => r.id !== id));
    this._users.update(list =>
      list.map(u => ({ ...u, roleIds: u.roleIds.filter(rid => rid !== id) }))
    );
    this.persist();
  }

  // --- Persistence ---

  private load(): void {
    let parsed: AuthStorage | null = null;
    try {
      const raw = typeof localStorage !== 'undefined' ? localStorage.getItem(STORAGE_KEY) : null;
      if (raw) parsed = JSON.parse(raw) as AuthStorage;
    } catch {
      parsed = null;
    }

    if (!parsed || !parsed.users?.length) {
      // First-time launch: bootstrap with a default user assigned the basic "user" role.
      const defaultUser: User = {
        id: 'user-default',
        username: 'user',
        displayName: 'Default User',
        email: '',
        roleIds: [DEFAULT_USER_ROLE_ID],
        createdAt: new Date().toISOString()
      };
      this._roles.set([...DEFAULT_ROLES]);
      this._users.set([defaultUser]);
      this._currentUserId.set(defaultUser.id);
      this.persist();
      return;
    }

    // Merge stored roles with defaults so built-ins always exist and have up-to-date permissions.
    const storedRoles = parsed.roles ?? [];
    const mergedRoles: Role[] = [];
    
    for (const def of DEFAULT_ROLES) {
      const stored = storedRoles.find(r => r.id === def.id);
      if (stored) {
        mergedRoles.push({
          ...stored,
          permissions: [...def.permissions] // Force update built-in permissions
        });
      } else {
        mergedRoles.push(def);
      }
    }
    
    for (const r of storedRoles) {
      if (!mergedRoles.some(mr => mr.id === r.id)) {
        mergedRoles.push(r);
      }
    }

    this._roles.set(mergedRoles);

    // Auto-grant admin to the default user for development
    for (const u of parsed.users) {
      if (u.id === 'user-default' && !u.roleIds.includes(ADMIN_ROLE_ID)) {
        u.roleIds.push(ADMIN_ROLE_ID);
      }
    }

    this._users.set(parsed.users);
    const currentId = parsed.currentUserId && parsed.users.some(u => u.id === parsed!.currentUserId)
      ? parsed.currentUserId
      : parsed.users[0]?.id ?? null;
    this._currentUserId.set(currentId);
  }

  private persist(): void {
    if (typeof localStorage === 'undefined') return;
    const data: AuthStorage = {
      users: this._users(),
      roles: this._roles(),
      currentUserId: this._currentUserId()
    };
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(data));
    } catch {
      // ignore quota / privacy errors
    }
  }
}
