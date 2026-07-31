import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomerService } from '../../core/services/customer.service';
import { AuthService } from '../../core/services/auth.service';
import { Customer, CreateCustomerRequest } from '../../core/models/customer.model';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customers.component.html',
  styleUrl: './customers.component.css'
})
export class CustomersComponent implements OnInit {
  customers = signal<Customer[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  showForm = signal(false);

  form: CreateCustomerRequest = { name: '', phone: '', email: '', address: '' };

  constructor(private customerService: CustomerService, public auth: AuthService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.customerService.getAll().subscribe({
      next: (customers) => {
        this.customers.set(customers);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load customers.');
        this.loading.set(false);
      }
    });
  }

  submit(): void {
    this.error.set(null);
    this.customerService.create(this.form).subscribe({
      next: () => {
        this.form = { name: '', phone: '', email: '', address: '' };
        this.showForm.set(false);
        this.load();
      },
      error: (err) => this.error.set(err?.error?.detail ?? 'Could not create customer.')
    });
  }
}
