import { Component } from '@angular/core';
import { AsyncPipe, DatePipe } from '@angular/common';
import { Observable } from 'rxjs';
import { CustomersApiService } from '../../core/api';
import { PagedResult } from '../../core/models/shared';
import { CustomerDto } from '../../core/models/customers';

@Component({
  selector: 'app-customers-list',
  standalone: true,
  imports: [AsyncPipe, DatePipe],
  template: `
    <section class="customers-page">
      <header>
        <h2>Customers</h2>
      </header>

      @if (customers$ | async; as result) {
        <div class="customers-grid">
          @for (customer of result.items; track customer.id) {
            <article class="customer-row">
              <div>
                <strong>{{ customer.name }}</strong>
                <span>{{ customer.email }}</span>
              </div>
              <span>{{ customer.company || 'No company' }}</span>
              <span>{{ customer.createdAt | date: 'mediumDate' }}</span>
            </article>
          } @empty {
            <p>No customers found.</p>
          }
        </div>
      }
    </section>
  `,
  styles: [`
    .customers-page {
      padding: 24px;
    }
    .customers-grid {
      display: grid;
      gap: 8px;
    }
    .customer-row {
      align-items: center;
      border: 1px solid #d7dde5;
      border-radius: 8px;
      display: grid;
      gap: 16px;
      grid-template-columns: minmax(220px, 1fr) minmax(160px, 240px) 140px;
      padding: 12px 16px;
    }
    .customer-row div {
      display: grid;
      gap: 4px;
    }
    .customer-row span {
      color: #536070;
    }
  `],
})
export class CustomersListComponent {
  readonly customers$: Observable<PagedResult<CustomerDto>>;

  constructor(customersApi: CustomersApiService) {
    this.customers$ = customersApi.getCustomers({
      page: 1,
      pageSize: 25,
      sortBy: 'name',
      sortDirection: 'asc',
    });
  }
}
