export interface Product {
  id: number;
  sku: string;
  name: string;
  description?: string;
  unitPrice: number;
  stockQuantity: number;
}

export interface CreateProductRequest {
  sku: string;
  name: string;
  description?: string;
  unitPrice: number;
  stockQuantity: number;
}
