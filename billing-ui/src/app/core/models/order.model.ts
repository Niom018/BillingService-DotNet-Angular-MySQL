export type OrderStatus = 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled';
export type PaymentMethod = 'Cash' | 'Card' | 'Mfs';
export type MfsProvider = 'Bkash' | 'Nagad' | 'Rocket' | 'Upay';

export interface CreateOrderItemRequest {
  productId: number;
  quantity: number;
}

export interface CreateOrderRequest {
  customerId: number;
  items: CreateOrderItemRequest[];
}

export interface OrderItem {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Order {
  id: number;
  orderNumber: string;
  customerName: string;
  orderDate: string;
  status: OrderStatus;
  items: OrderItem[];
  subtotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  paymentMethod?: string;
  paymentStatus?: string;
}

export interface RecordPaymentRequest {
  method: PaymentMethod;
  mfsProvider?: MfsProvider;
  transactionReference?: string;
  amountPaid: number;
}
