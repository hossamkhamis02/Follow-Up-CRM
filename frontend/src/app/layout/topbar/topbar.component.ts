import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';
import { MatSidenav } from '@angular/material/sidenav';
import { AuthService } from '../../core/auth/auth.service';
import { APP_NAME } from '../../core/constants';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatBadgeModule,
    MatDividerModule,
  ],
  template: `
    <mat-toolbar class="app-topbar" color="primary">
      @if (isHandset) {
        <button mat-icon-button (click)="sidenav?.toggle()" aria-label="Toggle sidebar">
          <mat-icon>menu</mat-icon>
        </button>
      }

      <span class="topbar-title">{{ appName }}</span>

      <div class="topbar-spacer"></div>

      <button mat-icon-button aria-label="Notifications" class="topbar-action">
        <mat-icon matBadge="3" matBadgeColor="accent">notifications</mat-icon>
      </button>

      @if (authService.isAuthenticated()) {
        <button mat-icon-button [matMenuTriggerFor]="userMenu" aria-label="User menu">
          <mat-icon>account_circle</mat-icon>
        </button>

        <mat-menu #userMenu="matMenu">
          <span mat-menu-item class="topbar-user-info">
            {{ authService.getUserFullName() }}
          </span>
          <span mat-menu-item class="topbar-user-email">
            {{ authService.getUserEmail() }}
          </span>
          <mat-divider></mat-divider>
          <button mat-menu-item routerLink="/settings">
            <mat-icon>settings</mat-icon>
            <span>Settings</span>
          </button>
          <button mat-menu-item (click)="authService.logout()">
            <mat-icon>logout</mat-icon>
            <span>Sign out</span>
          </button>
        </mat-menu>
      } @else {
        <button mat-button routerLink="/login">
          <mat-icon>login</mat-icon>
          <span>Sign in</span>
        </button>
      }
    </mat-toolbar>
  `,
  styles: [`
    .app-topbar {
      position: sticky;
      top: 0;
      z-index: 100;
      height: 56px;
    }

    .topbar-title {
      font-weight: 500;
      font-size: 16px;
    }

    .topbar-spacer {
      flex: 1 1 auto;
    }

    .topbar-action {
      margin-right: 8px;
    }

    .topbar-user-info {
      font-weight: 600;
      cursor: default;
      pointer-events: none;
    }

    .topbar-user-email {
      font-size: 12px;
      opacity: 0.7;
      cursor: default;
      pointer-events: none;
    }
  `],
})
export class TopbarComponent {
  @Input() sidenav!: MatSidenav | null;
  @Input() isHandset: boolean | null = false;

  appName = APP_NAME;

  constructor(public authService: AuthService) {}
}
