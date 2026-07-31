import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CustomerService } from '../../core/services/customer.service';
import { ProductService } from '../../core/services/product.service';
import { OrderService } from '../../core/services/order.service';
import { Customer } from '../../core/models/customer.model';
import { Product } from '../../core/models/product.model';
import { CreateOrderItemRequest } from '../../core/models/order.model';

@Component({
  selector: 'app-order-create',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './order-create.component.html'
})
export class OrderCreateComponent implements OnInit {
  customers = signal<Customer[]>([]);
  products = signal<Product[]>([]);
  error = signal<string | null>(null);
  submitting = signal(false);

  customerId: number | null = null;
  items: CreateOrderItemRequest[] = [{ productId: 0, quantity: 1 }];

  constructor(
    private customerService: CustomerService,
    private productService: ProductService,
    private orderService: OrderService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.customerService.getAll().subscribe((c) => this.customers.set(c));
    this.productService.getAll().subscribe((p) => this.products.set(p));
  }

  addItem(): void {
    this.items.push({ productId: 0, quantity: 1 });
  }

  removeItem(index: number): void {
    this.items.splice(index, 1);
  }

  productPrice(productId: number): number {
    return this.products().find((p) => p.id === productId)?.unitPrice ?? 0;
  }

  get estimatedTotal(): number {
    return this.items.reduce((sum, item) => sum + this.productPrice(item.productId) * (item.quantity || 0), 0);
  }

  submit(): void {
    this.error.set(null);

    if (!this.customerId) {
      this.error.set('Pick a customer first.');
      return;
    }
    if (this.items.some((i) => !i.productId || i.quantity < 1)) {
      this.error.set('Every line needs a product and a quantity of at least 1.');
      return;
    }

    this.submitting.set(true);
    this.orderService.create({ customerId: this.customerId, items: this.items }).subscribe({
      next: (order) => this.router.navigate(['/orders', order.id]),
      error: (err) => {
        this.submitting.set(false);
        this.error.set(err?.error?.detail ?? 'Could not create the order.');
      }
    });
  }
}
