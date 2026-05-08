export interface Operator {
  id: number;
  badgeNumber: string;
  name: string;
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

export interface SerialHistory {
  serialNumber: string;
  partNumber: string;
  partDescription: string;
  completedOps: string[];
  inProcessOps: string[];
}

export type WorkItemStatus = 'InProcess' | 'Completed';

export interface WorkItem {
  id: string;
  badgeNumber: string;
  part: string;
  serial: string;
  opNumber: string;
  status: WorkItemStatus | number;
  createdAt: string;
}

export interface StartWorkItemRequest {
  badgeNumber: string;
  part: string;
  serial: string;
  opNumber: string;
}

export interface CompleteOperationRequest {
  part: string;
  serial: string;
  opNumber: string;
  badgeNumber: string;
}
