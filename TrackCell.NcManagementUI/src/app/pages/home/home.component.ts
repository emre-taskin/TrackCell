import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { Permission } from '../../models/auth.models';

interface HomeTile {
  route: string;
  title: string;
  description: string;
  icon: string;
  accent: string;
  permission: Permission;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  private auth = inject(AuthService);

  readonly currentUser = this.auth.currentUser;

  private readonly allTiles: HomeTile[] = [
    {
      route: '/dashboard',
      title: 'Dashboard',
      description: 'Live KPIs, NC trends, and operational health at a glance.',
      icon: '📊',
      accent: '59, 130, 246',
      permission: 'view:dashboard'
    },
    {
      route: '/part-editor',
      title: 'Part Editor',
      description: 'Define parts, operations, and inspection plans.',
      icon: '🧩',
      accent: '139, 92, 246',
      permission: 'view:part-editor'
    },
    {
      route: '/inspector',
      title: 'Inspector',
      description: 'Record findings and capture non-conformities at the line.',
      icon: '🔎',
      accent: '14, 165, 233',
      permission: 'view:inspector'
    },
    {
      route: '/nc-heatmap-report',
      title: 'NC Heatmap',
      description: 'Visualize defect concentration across cells and zones.',
      icon: '🔥',
      accent: '239, 68, 68',
      permission: 'view:nc-heatmap-report'
    },
    {
      route: '/tickets',
      title: 'Tickets',
      description: 'Triage, assign, and resolve quality tickets.',
      icon: '🎫',
      accent: '245, 158, 11',
      permission: 'view:tickets'
    },
    {
      route: '/admin',
      title: 'Admin',
      description: 'Installation overview and administrative shortcuts.',
      icon: '⚙️',
      accent: '148, 163, 184',
      permission: 'view:admin'
    },
    {
      route: '/admin/users',
      title: 'Users',
      description: 'Create accounts and assign roles.',
      icon: '👥',
      accent: '34, 197, 94',
      permission: 'manage:users'
    },
    {
      route: '/admin/roles',
      title: 'Roles',
      description: 'Define roles and the permissions they grant.',
      icon: '🛡️',
      accent: '168, 85, 247',
      permission: 'manage:roles'
    }
  ];

  readonly tiles = computed<HomeTile[]>(() =>
    this.allTiles.filter(t => this.auth.hasPermission(t.permission))
  );

  readonly greeting = computed(() => {
    const hour = new Date().getHours();
    if (hour < 5) return 'Working late';
    if (hour < 12) return 'Good morning';
    if (hour < 18) return 'Good afternoon';
    return 'Good evening';
  });
}
