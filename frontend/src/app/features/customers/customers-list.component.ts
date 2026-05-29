import { Component } from '@angular/core';

@Component({
  selector: 'app-customers-list',
  standalone: true,
  imports: [],
  template: `
    <div class="customers-placeholder">
      <h2>Customers</h2>
      <p>Customer management will be implemented here.</p>
    </div>
  `,
  styles: [`
    .customers-placeholder {
      padding: 24px;
    }
  `],
})
export class CustomersListComponent {}