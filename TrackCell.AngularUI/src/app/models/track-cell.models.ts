export interface User {
  id: number;
  displayName: string;
  role: string;
  badgeNumber: string | null;
}

export interface PartDefinition {
  id: number;
  partNumber: string;
  description: string;
}

export interface OperationDefinition {
  id: number;
  opNumber: string;
  description: string;
}

export type OperationHistoryStatus = 'InProcess' | 'Completed';

export interface OperationHistory {
  id: string;
  badgeNumber: string;
  partSerialId: number;
  part: string;
  serial: string;
  opNumber: string;
  status: OperationHistoryStatus | number;
  createdAt: string;
}

export interface StartOperationHistoryRequest {
  badgeNumber: string;
  partSerialId: number;
  opNumber: string;
}

export interface CompleteOperationRequest {
  partSerialId: number;
  opNumber: string;
  badgeNumber: string;
}
