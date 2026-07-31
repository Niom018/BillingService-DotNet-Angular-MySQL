import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Customer, CreateCustomerRequest } from '../models/customer.model';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private base = `${environment.apiUrl}/Customers`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Customer[]> {
    return this.http.get<Customer[]>(this.base);
  }

  create(request: CreateCustomerRequest): Observable<Customer> {
    return this.http.post<Customer>(this.base, request);
  }
}
