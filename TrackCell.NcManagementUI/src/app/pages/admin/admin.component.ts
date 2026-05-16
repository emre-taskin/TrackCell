import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent {
  private auth = inject(AuthService);

  readonly currentUser = this.auth.currentUser;
  readonly users = this.auth.users;
  readonly roles = this.auth.roles;

  readonly stats = computed(() => ({
    userCount: this.users().length,
    roleCount: this.roles().length,
    builtInRoleCount: this.roles().filter(r => r.builtIn).length,
    customRoleCount: this.roles().filter(r => !r.builtIn).length
  }));

  canManageUsers = computed(() => this.auth.hasPermission('manage:users'));
  canManageRoles = computed(() => this.auth.hasPermission('manage:roles'));
}
