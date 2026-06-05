import { Routes } from '@angular/router';
import { guestGuard } from '../../core/guards';

export const IDENTITY_ROUTES: Routes = [
  { path: 'login', redirectTo: '/login', pathMatch: 'full' },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./register.component').then(m => m.RegisterComponent),
  },
];
