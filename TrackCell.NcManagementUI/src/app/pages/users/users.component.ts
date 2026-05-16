import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Role, User } from '../../models/auth.models';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

interface UserDraft {
  username: string;
  displayName: string;
  email: string;
  roleIds: string[];
}

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent {
  private auth = inject(AuthService);
  private toast = inject(ToastService);

  readonly users = this.auth.users;
  readonly roles = this.auth.roles;
  readonly currentUser = this.auth.currentUser;

  readonly rolesById = computed(() => {
    const m = new Map<string, Role>();
    for (const r of this.roles()) m.set(r.id, r);
    return m;
  });

  readonly editingUserId = signal<string | null>(null);
  readonly showCreateForm = signal(false);

  readonly draft = signal<UserDraft>(this.blankDraft());

  startCreate(): void {
    this.editingUserId.set(null);
    this.draft.set(this.blankDraft());
    this.showCreateForm.set(true);
  }

  startEdit(user: User): void {
    this.editingUserId.set(user.id);
    this.draft.set({
      username: user.username,
      displayName: user.displayName,
      email: user.email,
      roleIds: [...user.roleIds]
    });
    this.showCreateForm.set(true);
  }

  cancelEdit(): void {
    this.editingUserId.set(null);
    this.showCreateForm.set(false);
    this.draft.set(this.blankDraft());
  }

  toggleRole(roleId: string, ev: Event): void {
    const checked = (ev.target as HTMLInputElement).checked;
    this.draft.update(d => {
      const set = new Set(d.roleIds);
      if (checked) set.add(roleId);
      else set.delete(roleId);
      return { ...d, roleIds: Array.from(set) };
    });
  }

  isRoleSelected(roleId: string): boolean {
    return this.draft().roleIds.includes(roleId);
  }

  save(): void {
    const d = this.draft();
    if (!d.username.trim()) {
      this.toast.show('Username is required.', 'error');
      return;
    }
    if (!d.roleIds.length) {
      this.toast.show('Assign at least one role.', 'error');
      return;
    }

    const editingId = this.editingUserId();
    if (editingId) {
      this.auth.updateUser(editingId, {
        username: d.username.trim(),
        displayName: d.displayName.trim() || d.username.trim(),
        email: d.email.trim(),
        roleIds: d.roleIds
      });
      this.toast.show('User updated');
    } else {
      const duplicate = this.users().some(
        u => u.username.toLowerCase() === d.username.trim().toLowerCase()
      );
      if (duplicate) {
        this.toast.show('A user with that username already exists.', 'error');
        return;
      }
      this.auth.createUser(d);
      this.toast.show('User created');
    }
    this.cancelEdit();
  }

  deleteUser(user: User): void {
    if (user.id === this.currentUser()?.id) {
      this.toast.show('You cannot delete the user you are signed in as.', 'error');
      return;
    }
    if (!confirm(`Delete user "${user.displayName}"?`)) return;
    try {
      this.auth.deleteUser(user.id);
      this.toast.show('User deleted');
    } catch (e: unknown) {
      this.toast.show(e instanceof Error ? e.message : 'Delete failed', 'error');
    }
  }

  switchTo(user: User): void {
    this.auth.setCurrentUser(user.id);
    this.toast.show(`Signed in as ${user.displayName}`);
  }

  roleNames(user: User): string {
    const m = this.rolesById();
    return user.roleIds
      .map(id => m.get(id)?.name ?? '?')
      .join(', ') || '—';
  }

  trackUser = (_: number, u: User) => u.id;
  trackRole = (_: number, r: Role) => r.id;

  private blankDraft(): UserDraft {
    return { username: '', displayName: '', email: '', roleIds: [] };
  }
}
