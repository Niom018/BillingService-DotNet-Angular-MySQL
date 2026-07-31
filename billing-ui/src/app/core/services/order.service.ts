import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateOrderRequest, Order, RecordPaymentRequest } from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private base = `${environment.apiUrl}/Orders`;

  constructor(private http: HttpClient) {}

  create(request: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.base, request);
  }

  getById(id: number): Observable<Order> {
    return this.http.get<Order>(`${this.base}/${id}`);
  }

  confirm(id: number): Observable<Order> {
    return this.http.post<Order>(`${this.base}/${id}/confirm`, {});
  }

  recordPayment(orderId: number, request: RecordPaymentRequest): Observable<Order> {
    return this.http.post<Order>(`${environment.apiUrl}/orders/${orderId}/Payments`, request);
  }

  downloadInvoice(id: number): Observable<Blob> {
    return this.http.get(`${this.base}/${id}/invoice`, { responseType: 'blob' });
  }
}
