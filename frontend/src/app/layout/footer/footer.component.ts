import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <footer class="app-footer">
      <span>&copy; {{ currentYear }} FollowUp CRM. All rights reserved.</span>
    </footer>
  `,
  styles: [`
    .app-footer {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 16px;
      font-size: 12px;
      opacity: 0.6;
    }
  `],
})
export class FooterComponent {
  currentYear = new Date().getFullYear();
}