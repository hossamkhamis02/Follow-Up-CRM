import { Routes } from '@angular/router';

export const IDENTITY_ROUTES: Routes = [
  { path: 'login', loadComponent: () => import('./login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./register.component').then(m => m.RegisterComponent) },
];