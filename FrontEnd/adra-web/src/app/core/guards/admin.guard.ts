import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const notificationService = inject(NotificationService);

  return authService.currentUser$.pipe(
    map(user => {
      if (user && authService.isAdmin()) {
        return true;
      }
      
      notificationService.showError('Admin access required');
      router.navigate(['/balances']);
      return false;
    })
  );
};
