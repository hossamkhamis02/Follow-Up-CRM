import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { AppShellComponent } from './layout/app-shell/app-shell.component';

export const routes: Routes = [
  {
    path: 'identity',
    loadChildren: () => import('./features/identity/identity.routes').then(m => m.IDENTITY_ROUTES),
  },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
      },
      {
        path: 'customers',
        loadChildren: () => import('./features/customers/customers.routes').then(m => m.CUSTOMERS_ROUTES),
      },
      {
        path: 'follow-ups',
        loadChildren: () => import('./features/follow-ups/follow-ups.routes').then(m => m.FOLLOW_UPS_ROUTES),
      },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];