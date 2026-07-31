export interface Customer {
  id: number;
  name: string;
  phone: string;
  email?: string;
  address?: string;
}

export interface CreateCustomerRequest {
  name: string;
  phone: string;
  email?: string;
  address?: string;
}
