export enum AnomalySeverity {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export interface Anomaly {
  id: number;
  metricType: string;
  serverName: string;
  actualValue: number;
  expectedValue: number;
  deviation: number;
  severity: string;
  detectedAt: Date;
  isResolved: boolean;
  resolvedAt?: Date;
  description: string;
}
