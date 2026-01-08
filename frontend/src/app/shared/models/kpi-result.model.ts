export enum KpiStatus {
  BelowTarget = 0,
  OnTarget = 1,
  AboveTarget = 2,
  Critical = 3
}

export interface KpiResult {
  id: number;
  kpiName: string;
  calculatedValue: number;
  targetValue?: number;
  status: string;
  calculatedAt: Date;
  periodStart: Date;
  periodEnd: Date;
  description?: string;
  percentageOfTarget?: number;
  unit?: string;
}
