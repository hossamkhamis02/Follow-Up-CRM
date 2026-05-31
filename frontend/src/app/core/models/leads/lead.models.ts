export type LeadStatus = 'new' | 'qualified' | 'contacted' | 'converted' | 'lost';

export interface LeadDto {
  id: string;
  fullName: string;
  email?: string | null;
  phone?: string | null;
  company?: string | null;
  source?: string | null;
  status: LeadStatus;
  createdAt: string;
}

export interface CreateLeadRequest {
  fullName: string;
  email?: string | null;
  phone?: string | null;
  company?: string | null;
  source?: string | null;
  status?: LeadStatus;
}

export interface UpdateLeadRequest extends CreateLeadRequest {}
