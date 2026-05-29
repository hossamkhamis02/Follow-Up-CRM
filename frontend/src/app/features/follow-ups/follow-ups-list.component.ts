import { Component } from '@angular/core';

@Component({
  selector: 'app-follow-ups-list',
  standalone: true,
  imports: [],
  template: `
    <div class="follow-ups-placeholder">
      <h2>Follow-ups</h2>
      <p>Follow-up management will be implemented here.</p>
    </div>
  `,
  styles: [`
    .follow-ups-placeholder {
      padding: 24px;
    }
  `],
})
export class FollowUpsListComponent {}