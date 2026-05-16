import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Permission } from '../models/auth.models';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

export function permissionGuard(...required: Permission[]): CanActivateFn {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const toast = inject(ToastService);

    const granted = required.every(p => auth.hasPermission(p));
    if (granted) return true;

    toast.show('You do not have permission to access that page.', 'error');
    return router.parseUrl('/dashboard');
  };
}
