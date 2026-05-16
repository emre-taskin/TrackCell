import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from './services/auth.service';
import { ToastContainerComponent } from './shared/toast-container.component';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, ToastContainerComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private auth = inject(AuthService);

  protected readonly title = signal('TrackCell NC Management');
  protected readonly currentUser = this.auth.currentUser;
  protected readonly currentRoles = this.auth.currentRoles;

  protected readonly canViewDashboard = computed(() => this.auth.hasPermission('view:dashboard'));
  protected readonly canViewPartEditor = computed(() => this.auth.hasPermission('view:part-editor'));
  protected readonly canViewInspector = computed(() => this.auth.hasPermission('view:inspector'));
  protected readonly canViewTickets = computed(() => this.auth.hasPermission('view:tickets'));
  protected readonly canViewAdmin = computed(() => this.auth.hasPermission('view:admin'));
  protected readonly canManageUsers = computed(() => this.auth.hasPermission('manage:users'));
  protected readonly canManageRoles = computed(() => this.auth.hasPermission('manage:roles'));

  protected readonly currentRoleNames = computed(() =>
    this.currentRoles().map(r => r.name).join(', ') || 'No role'
  );

  constructor(private router: Router) {
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(() => {
      let r = this.router.routerState.snapshot.root;
      while (r.firstChild) r = r.firstChild;
      const t = r.data?.['title'];
      if (t) document.title = `${t} - TrackCell NC`;
    });
  }
}
