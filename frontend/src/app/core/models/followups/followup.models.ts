export type FollowUpStatus = 'pending' | 'completed' | 'cancelled';
export type FollowUpType = 'call' | 'email' | 'meeting' | 'task';

export interface FollowUpDto {
  id: string;
  title: string;
  notes?: string | null;
  dueAt: string;
  completedAt?: string | null;
  status: FollowUpStatus;
  type: FollowUpType;
  customerId?: string | null;
  leadId?: string | null;
  createdAt: string;
}

export interface CreateFollowUpRequest {
  title: string;
  notes?: string | null;
  dueAt: string;
  type: FollowUpType;
  customerId?: string | null;
  leadId?: string | null;
}

export interface UpdateFollowUpRequest extends CreateFollowUpRequest {
  status: FollowUpStatus;
  completedAt?: string | null;
}
