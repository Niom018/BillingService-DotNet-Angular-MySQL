import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Product, CreateProductRequest } from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private base = `${environment.apiUrl}/Products`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.base);
  }

  create(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.base, request);
  }
}
