import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  // Root redirect to dashboard after login
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full'
  },
  
  // Auth routes (guest only)
  {
    path: 'login',
    loadComponent: () => import('./features/auth/components/login/login.component').then(m => m.LoginComponent)
    // canActivate: [guestGuard] - temporarily removed for testing
  },
  
  // Protected routes
  {
    path: 'balances',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./features/balances/components/balance-summary/balance-summary.component').then(m => m.BalanceSummaryComponent)
      },
      {
        path: 'list',
        loadComponent: () => import('./features/balances/components/balance-list/balance-list.component').then(m => m.BalanceListComponent)
      },
      {
        path: 'upload',
        loadComponent: () => import('./features/balances/components/balance-upload/balance-upload.component').then(m => m.BalanceUploadComponent),
        canActivate: [adminGuard]
      }
    ]
  },
  
  // Wildcard route
  {
    path: '**',
    redirectTo: '/balances'
  }
];
