export interface PartDefinition {
  id: number;
  partNumber: string;
  description: string;
}

export interface PartSerial {
  id: number;
  partDefinitionId: number;
  serialNumber: string;
}

export interface OperationDefinition {
  id: number;
  opNumber: string;
  description: string;
}

export interface SerialLookupResult {
  partSerial: PartSerial;
  partDefinition: PartDefinition;
  operations: OperationDefinition[];
}

export interface NonConformance {
  id: number;
  code: string;
  description: string;
}

export interface ImageZone {
  id: number;
  name: string;
  /** Normalized 0..1 coordinates relative to the displayed image. */
  x: number;
  y: number;
  width: number;
  height: number;
  nonConformanceIds: number[];
}

export interface PartImage {
  id: number;
  partDefinitionId: number;
  name: string;
  imageUrl: string;
  uploadedAt: string;
  zones: ImageZone[];
}

export interface InspectionResult {
  id: number;
  partImageId: number;
  imageZoneId: number;
  nonConformanceId: number;
  serialNumber: string | null;
  notes: string | null;
  inspectedAt: string;
}

export interface CreateInspectionResultRequest {
  partImageId: number;
  imageZoneId: number;
  nonConformanceId: number;
  partSerialId?: number;
  serialNumber?: string;
  notes?: string;
}

export interface HeatmapZone {
  zoneId: number;
  name: string;
  x: number;
  y: number;
  width: number;
  height: number;
  count: number;
  countsByNonConformance: Record<number, number>;
}

export interface HeatmapResponse {
  partImageId: number;
  nonConformanceId: number | null;
  maxCount: number;
  totalCount: number;
  zones: HeatmapZone[];
}

export interface SaveZonesRequest {
  zones: {
    name: string;
    x: number;
    y: number;
    width: number;
    height: number;
    nonConformanceIds: number[];
  }[];
}

export interface PartSerial {
  id: number;
  partDefinitionId: number;
  serialNumber: string;
}
