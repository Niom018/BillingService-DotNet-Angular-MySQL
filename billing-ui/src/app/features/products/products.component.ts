import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../core/services/product.service';
import { AuthService } from '../../core/services/auth.service';
import { CreateProductRequest, Product } from '../../core/models/product.model';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css'
})
export class ProductsComponent implements OnInit {
  products = signal<Product[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  showForm = signal(false);

  form: CreateProductRequest = { sku: '', name: '', description: '', unitPrice: 0, stockQuantity: 0 };

  constructor(private productService: ProductService, public auth: AuthService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.productService.getAll().subscribe({
      next: (products) => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load products.');
        this.loading.set(false);
      }
    });
  }

  submit(): void {
    this.error.set(null);
    this.productService.create(this.form).subscribe({
      next: () => {
        this.form = { sku: '', name: '', description: '', unitPrice: 0, stockQuantity: 0 };
        this.showForm.set(false);
        this.load();
      },
      error: (err) => this.error.set(err?.error?.detail ?? 'Could not create product.')
    });
  }
}
