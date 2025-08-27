import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // 1. Authentication Errors - Handle at system level
      if (error.status === 401) {
        notificationService.showError('Session expired. Please login again.');
        authService.logout();
        router.navigate(['/login']);
        return throwError(() => error);
      }

      // 2. Network/Server Errors - Always show to user
      if (error.status === 0) {
        notificationService.showError('Unable to connect to server. Please check your connection.');
        return throwError(() => error);
      }

      // 3. Server Errors - Always show to user
      if (error.status >= 500) {
        const errorMessage = error.error?.message || 'Server error occurred. Please try again later.';
        notificationService.showError(errorMessage);
        return throwError(() => error);
      }

      // 4. Permission Errors - System level handling
      if (error.status === 403) {
        notificationService.showError('Access denied. You do not have permission to perform this action.');
        return throwError(() => error);
      }

      // 5. Rate Limiting
      if (error.status === 429) {
        notificationService.showWarning('Too many requests. Please wait before trying again.');
        return throwError(() => error);
      }

      // 6. Client Errors (400, 404, 422) - Let components handle
      // These often represent business logic (no data found, validation errors)
      // Components should decide whether to show notifications or handle gracefully
      if (error.status >= 400 && error.status < 500) {
        // Don't show notification here - let components handle
        return throwError(() => error);
      }

      // 7. Fallback for any other errors
      const errorMessage = error.error?.message || 'An unexpected error occurred';
      notificationService.showError(errorMessage);
      return throwError(() => error);
    })
  );
};
