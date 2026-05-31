export interface CustomerDto {
  id: string;
  name: string;
  email: string;
  phone?: string | null;
  company?: string | null;
  createdAt: string;
}

export interface CreateCustomerRequest {
  name: string;
  email: string;
  phone?: string | null;
  company?: string | null;
}

export interface UpdateCustomerRequest extends CreateCustomerRequest {}
