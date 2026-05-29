import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../core/auth/auth.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatListModule,
    MatIconModule,
    MatDividerModule,
  ],
  template: `
    <div class="sidebar-header">
      <mat-icon class="sidebar-logo">rocket_launch</mat-icon>
      <span class="sidebar-brand">FollowUp CRM</span>
    </div>

    <mat-divider></mat-divider>

    <mat-nav-list class="sidebar-nav">
      @for (item of mainNavItems; track item.route) {
        <a mat-list-item
           [routerLink]="item.route"
           routerLinkActive="active-link"
           (click)="onNavigate()">
          <mat-icon matListItemIcon>{{ item.icon }}</mat-icon>
          <span matListItemTitle>{{ item.label }}</span>
        </a>
      }
    </mat-nav-list>

    <mat-divider></mat-divider>

    <mat-nav-list class="sidebar-nav">
      @for (item of secondaryNavItems; track item.route) {
        <a mat-list-item
           [routerLink]="item.route"
           routerLinkActive="active-link"
           (click)="onNavigate()">
          <mat-icon matListItemIcon>{{ item.icon }}</mat-icon>
          <span matListItemTitle>{{ item.label }}</span>
        </a>
      }
    </mat-nav-list>

    <div class="sidebar-footer">
      <mat-divider></mat-divider>
      <div class="sidebar-user" *ngIf="authService.isAuthenticated()">
        <mat-icon class="sidebar-user-avatar">account_circle</mat-icon>
        <span class="sidebar-user-name">{{ authService.getUserFullName() }}</span>
      </div>
    </div>
  `,
  styles: [`
    .sidebar-header {
      display: flex;
      align-items: center;
      padding: 16px 20px;
      height: 56px;
      gap: 12px;
    }

    .sidebar-logo {
      font-size: 28px;
      width: 28px;
      height: 28px;
      color: var(--mat-sys-primary);
    }

    .sidebar-brand {
      font-size: 18px;
      font-weight: 600;
      letter-spacing: -0.3px;
    }

    .sidebar-nav {
      padding-top: 4px;
    }

    .active-link {
      background: rgba(0, 0, 0, 0.04);
    }

    .sidebar-footer {
      margin-top: auto;
    }

    .sidebar-user {
      display: flex;
      align-items: center;
      padding: 12px 20px;
      gap: 10px;
    }

    .sidebar-user-avatar {
      font-size: 32px;
      width: 32px;
      height: 32px;
      opacity: 0.7;
    }

    .sidebar-user-name {
      font-size: 13px;
      opacity: 0.8;
    }
  `],
})
export class SidebarComponent {
  @Output() navigate = new EventEmitter<void>();

  mainNavItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
    { label: 'Customers', icon: 'people', route: '/customers' },
    { label: 'Follow-ups', icon: 'event_note', route: '/follow-ups' },
  ];

  secondaryNavItems: NavItem[] = [
    { label: 'Settings', icon: 'settings', route: '/settings' },
  ];

  constructor(public authService: AuthService) {}

  onNavigate(): void {
    this.navigate.emit();
  }
}