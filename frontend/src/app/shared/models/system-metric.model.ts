export interface SystemMetric {
  id: number;
  metricName: string;
  value: number;
  unit: string;
  timestamp: Date;
  source: string;
  tags?: string;
}

export interface CreateMetric {
  metricName: string;
  value: number;
  unit: string;
  source: string;
  tags?: string;
}

export interface MetricQuery {
  metricName?: string;
  source?: string;
  startDate?: Date;
  endDate?: Date;
  pageNumber?: number;
  pageSize?: number;
}
