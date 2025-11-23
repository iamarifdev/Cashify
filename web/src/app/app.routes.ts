import { Routes } from '@angular/router';

import { AuthGuard } from './core/auth/auth.guard';
import { HasBusinessGuard, OnboardingGuard } from './core/business/business.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'onboarding',
    canActivate: [AuthGuard, OnboardingGuard],
    loadComponent: () => import('./features/onboarding/onboarding.component').then(m => m.OnboardingComponent)
  },
  {
    path: '',
    canActivate: [AuthGuard, HasBusinessGuard],
    loadComponent: () => import('./features/shell/shell.component').then(m => m.ShellComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'transactions',
        loadComponent: () => import('./features/transactions/transactions.component').then(m => m.TransactionsComponent)
      },
      {
        path: 'activity',
        loadComponent: () => import('./features/activity/activity.component').then(m => m.ActivityComponent)
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
