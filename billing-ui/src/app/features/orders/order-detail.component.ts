import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { Order, PaymentMethod, MfsProvider, RecordPaymentRequest } from '../../core/models/order.model';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './order-detail.component.html'
})
export class OrderDetailComponent implements OnInit {
  order = signal<Order | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  actionBusy = signal(false);

  paymentMethod: PaymentMethod = 'Cash';
  mfsProvider: MfsProvider = 'Bkash';
  transactionReference = '';
  amountPaid = 0;

  private orderId!: number;

  constructor(private route: ActivatedRoute, private orderService: OrderService) {}

  ngOnInit(): void {
    this.orderId = Number(this.route.snapshot.paramMap.get('id'));
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.orderService.getById(this.orderId).subscribe({
      next: (order) => {
        this.order.set(order);
        this.amountPaid = order.totalAmount;
        this.loading.set(false);
      },
      error: () => {
        this.error.set(`Order #${this.orderId} was not found.`);
        this.loading.set(false);
      }
    });
  }

  confirm(): void {
    this.actionBusy.set(true);
    this.orderService.confirm(this.orderId).subscribe({
      next: (order) => {
        this.order.set(order);
        this.actionBusy.set(false);
      },
      error: (err) => {
        this.actionBusy.set(false);
        this.error.set(err?.error?.detail ?? 'Could not confirm the order.');
      }
    });
  }

  recordPayment(): void {
    this.error.set(null);
    const request: RecordPaymentRequest = {
      method: this.paymentMethod,
      amountPaid: this.amountPaid,
      ...(this.paymentMethod === 'Mfs' ? { mfsProvider: this.mfsProvider } : {}),
      ...(this.paymentMethod !== 'Cash' ? { transactionReference: this.transactionReference } : {})
    };

    this.actionBusy.set(true);
    this.orderService.recordPayment(this.orderId, request).subscribe({
      next: (order) => {
        this.order.set(order);
        this.actionBusy.set(false);
      },
      error: (err) => {
        this.actionBusy.set(false);
        this.error.set(err?.error?.detail ?? 'Could not record the payment.');
      }
    });
  }

  downloadInvoice(): void {
    this.orderService.downloadInvoice(this.orderId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${this.order()?.orderNumber ?? 'invoice'}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => this.error.set('Could not download the invoice yet.')
    });
  }

  badgeClass(status: string): string {
    return `badge badge-${status.toLowerCase()}`;
  }
}
