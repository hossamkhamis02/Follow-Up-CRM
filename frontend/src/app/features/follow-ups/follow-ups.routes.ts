import { Routes } from '@angular/router';

export const FOLLOW_UPS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./follow-ups-list.component').then(m => m.FollowUpsListComponent),
  },
];