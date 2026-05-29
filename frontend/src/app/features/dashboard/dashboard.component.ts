import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [],
  template: `
    <div class="dashboard-placeholder">
      <h2>Dashboard</h2>
      <p>Dashboard content will be implemented here.</p>
    </div>
  `,
  styles: [`
    .dashboard-placeholder {
      padding: 24px;
    }
  `],
})
export class DashboardComponent {}