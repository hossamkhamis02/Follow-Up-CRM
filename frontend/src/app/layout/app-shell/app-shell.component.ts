import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { TopbarComponent } from '../topbar/topbar.component';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { MatSidenav } from '@angular/material/sidenav';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatListModule,
    TopbarComponent,
    SidebarComponent,
  ],
  template: `
    <mat-sidenav-container class="app-shell">
      <mat-sidenav
        #sidenav
        class="app-sidebar"
        [mode]="(isHandset$ | async) ? 'over' : 'side'"
        [opened]="!(isHandset$ | async)"
        [fixedInViewport]="(isHandset$ | async)"
        [fixedTopGap]="topbarHeight"
        [autoFocus]="false">
        <app-sidebar (navigate)="sidenav.close()"></app-sidebar>
      </mat-sidenav>

      <mat-sidenav-content class="app-content">
        <app-topbar
          [sidenav]="sidenav"
          [isHandset]="isHandset$ | async">
        </app-topbar>

        <main class="main-content">
          <router-outlet></router-outlet>
        </main>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .app-shell {
      height: 100vh;
    }

    .app-sidebar {
      width: 260px;
      border-right: 1px solid rgba(0, 0, 0, 0.12);
    }

    .app-content {
      display: flex;
      flex-direction: column;
      min-height: 100vh;
    }

    .main-content {
      flex: 1;
      padding: 24px;
      overflow-y: auto;
      background: rgba(0, 0, 0, 0.02);
    }

    @media (max-width: 768px) {
      .main-content {
        padding: 16px;
      }
    }
  `],
})
export class AppShellComponent {
  @ViewChild('sidenav') sidenav!: MatSidenav;

  topbarHeight = 56;

  isHandset$: Observable<boolean> = this.breakpointObserver
    .observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay(1)
    );

  constructor(private breakpointObserver: BreakpointObserver) {}
}