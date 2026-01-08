export interface DashboardAnalytics {
  summary: DashboardSummary;
  responseTimeTrend: TimeSeriesData[];
  throughputTrend: TimeSeriesData[];
  serverMetrics: ServerMetrics[];
  metricDistribution: MetricDistribution[];
}

export interface DashboardSummary {
  totalServers: number;
  activeAlerts: number;
  averageResponseTime: number;
  systemAvailability: number;
  totalRequests: number;
  totalErrors: number;
}

export interface TimeSeriesData {
  timestamp: string;
  value: number;
  label?: string;
}

export interface ServerMetrics {
  serverName: string;
  cpuUsage: number;
  memoryUsage: number;
  responseTime: number;
  requestCount: number;
  status: 'Healthy' | 'Warning' | 'Critical';
}

export interface MetricDistribution {
  metricName: string;
  value: number;
  category: string;
}
