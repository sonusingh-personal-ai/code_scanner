export interface ModelDto {
  id: number;
  name: string;
  value: string;
  position: number;
}

export interface OfficeMemberDto {
  id: number;
  type: number; // 1: VisualBy, 2: TestedBy, 3: ProcEng, 4: ProgDisNo
  name: string;
}

export interface LineDto {
  id: number;
  name: string;
  sequence: number;
}

export interface PrinterModelValueDto {
  id: number;
  modelId: number;
  name: string;
  value: string;
}

export interface PrinterModelDto {
  id: number;
  name: string;
  modelValues: PrinterModelValueDto[];
}

export interface ShiftStatsDto {
  totalTested: number;
  passCount: number;
  failCount: number;
  yieldPercentage: number;
  currentShift: string;
  date: string;
}

export interface DashboardInitDto {
  models: ModelDto[];
  visualByList: OfficeMemberDto[];
  testedByList: OfficeMemberDto[];
  processEngList: OfficeMemberDto[];
  progDisplayNoList: OfficeMemberDto[];
  lines: LineDto[];
  printerModels: PrinterModelDto[];
  availablePorts: string[];
  availablePrinters: string[];
  shiftStats: ShiftStatsDto;
}

export interface TelemetryParamDto {
  parameter: string;
  display: string;
  actual: string;
  status: string;
  isPass: boolean;
}

export interface TestResultDto {
  responseId: number;
  barcode: string;
  isOverallPass: boolean;
  header: string;
  footer: string;
  displayProgNo: string;
  controlProgNo: string;
  systemRating: string;
  modelName: string;
  parameters: TelemetryParamDto[];
  formattedQrCode: string;
  message: string;
  timestamp: string;
}

export interface SaveResponseRequestDto {
  barcode: string;
  qcStatus?: number;
  visualBy?: number;
  testedBy?: number;
  productionLine?: number;
  processEngg?: number;
  serialCardNo?: string;
  model?: string;
  conProgNo?: string;
  disProgNo?: string;
  systemRating?: string;
  printerModelId?: number;
  line?: number;
  rawTelemetry?: string;
}
