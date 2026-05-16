import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ALL_PERMISSIONS,
  PERMISSION_LABELS,
  Permission,
  Role
} from '../../models/auth.models';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

interface RoleDraft {
  name: string;
  description: string;
  permissions: Permission[];
}

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.css'
})
export class RolesComponent {
  private auth = inject(AuthService);
  private toast = inject(ToastService);

  readonly roles = this.auth.roles;
  readonly users = this.auth.users;
  readonly allPermissions = ALL_PERMISSIONS;
  readonly permissionLabel = (p: Permission) => PERMISSION_LABELS[p];

  readonly editingRoleId = signal<string | null>(null);
  readonly showForm = signal(false);
  readonly draft = signal<RoleDraft>(this.blank());

  startCreate(): void {
    this.editingRoleId.set(null);
    this.draft.set(this.blank());
    this.showForm.set(true);
  }

  startEdit(role: Role): void {
    this.editingRoleId.set(role.id);
    this.draft.set({
      name: role.name,
      description: role.description,
      permissions: [...role.permissions]
    });
    this.showForm.set(true);
  }

  cancel(): void {
    this.editingRoleId.set(null);
    this.showForm.set(false);
    this.draft.set(this.blank());
  }

  isPermissionSelected(p: Permission): boolean {
    return this.draft().permissions.includes(p);
  }

  togglePermission(p: Permission, ev: Event): void {
    const checked = (ev.target as HTMLInputElement).checked;
    this.draft.update(d => {
      const set = new Set(d.permissions);
      if (checked) set.add(p);
      else set.delete(p);
      return { ...d, permissions: Array.from(set) };
    });
  }

  isEditingBuiltIn(): boolean {
    const id = this.editingRoleId();
    if (!id) return false;
    return !!this.roles().find(r => r.id === id)?.builtIn;
  }

  save(): void {
    const d = this.draft();
    if (!d.name.trim()) {
      this.toast.show('Role name is required.', 'error');
      return;
    }
    const id = this.editingRoleId();
    if (id) {
      this.auth.updateRole(id, {
        name: d.name.trim(),
        description: d.description.trim(),
        permissions: d.permissions
      });
      this.toast.show('Role updated');
    } else {
      this.auth.createRole({
        name: d.name.trim(),
        description: d.description.trim(),
        permissions: d.permissions
      });
      this.toast.show('Role created');
    }
    this.cancel();
  }

  deleteRole(role: Role): void {
    if (role.builtIn) {
      this.toast.show('Built-in roles cannot be deleted.', 'error');
      return;
    }
    const assigned = this.users().filter(u => u.roleIds.includes(role.id)).length;
    const suffix = assigned ? ` It is assigned to ${assigned} user(s); they will lose that role.` : '';
    if (!confirm(`Delete role "${role.name}"?${suffix}`)) return;
    try {
      this.auth.deleteRole(role.id);
      this.toast.show('Role deleted');
    } catch (e: unknown) {
      this.toast.show(e instanceof Error ? e.message : 'Delete failed', 'error');
    }
  }

  userCountForRole(roleId: string): number {
    return this.users().filter(u => u.roleIds.includes(roleId)).length;
  }

  trackRole = (_: number, r: Role) => r.id;
  trackPerm = (_: number, p: Permission) => p;

  private blank(): RoleDraft {
    return { name: '', description: '', permissions: [] };
  }
}
