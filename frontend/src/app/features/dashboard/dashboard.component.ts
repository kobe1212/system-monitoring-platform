import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MetricService } from '../../core/services/metric.service';
import { KpiService } from '../../core/services/kpi.service';
import { AnomalyService } from '../../core/services/anomaly.service';
import { SystemMetric } from '../../shared/models/system-metric.model';
import { KpiResult } from '../../shared/models/kpi-result.model';
import { Anomaly } from '../../shared/models/anomaly.model';
import { ChartConfiguration, ChartOptions } from 'chart.js';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  recentMetrics: SystemMetric[] = [];
  kpis: KpiResult[] = [];
  unresolvedAnomalies: Anomaly[] = [];
  isLoading = true;
  errorMessage = '';

  // Analytics Data
  dashboardAnalytics: any = null;
  
  // Chart Data
  responseTimeChartData: ChartConfiguration<'line'>['data'] | null = null;
  cpuUsageChartData: ChartConfiguration<'line'>['data'] | null = null;
  serverComparisonChartData: ChartConfiguration<'bar'>['data'] | null = null;
  
  // Chart Options
  lineChartOptions: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'top'
      },
      tooltip: {
        mode: 'index',
        intersect: false
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        grid: {
          color: 'rgba(0, 0, 0, 0.05)'
        }
      },
      x: {
        grid: {
          display: false
        }
      }
    }
  };

  barChartOptions: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false
      }
    },
    scales: {
      y: {
        beginAtZero: true
      }
    }
  };

  constructor(
    private http: HttpClient,
    private metricService: MetricService,
    private kpiService: KpiService,
    private anomalyService: AnomalyService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    Promise.all([
      this.http.get(`${environment.apiUrl}/dashboard/analytics`).toPromise(),
      this.kpiService.getAllKpis().toPromise(),
      this.anomalyService.getUnresolvedAnomalies().toPromise()
    ])
      .then(([analytics, kpis, anomalies]) => {
        this.dashboardAnalytics = analytics;
        this.kpis = kpis || [];
        this.unresolvedAnomalies = anomalies || [];
        this.prepareChartData();
        this.isLoading = false;
      })
      .catch(error => {
        this.errorMessage = 'Failed to load dashboard data';
        console.error('Dashboard error:', error);
        this.isLoading = false;
      });
  }

  prepareChartData(): void {
    if (!this.dashboardAnalytics) return;

    // Response Time Chart (Last 24 Hours)
    const responseTimeData = this.dashboardAnalytics.last24Hours?.responseTime || [];
    this.responseTimeChartData = {
      labels: responseTimeData.map((d: any) => new Date(d.timestamp).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })),
      datasets: [
        {
          label: 'Response Time (ms)',
          data: responseTimeData.map((d: any) => d.value),
          borderColor: '#3b82f6',
          backgroundColor: 'rgba(59, 130, 246, 0.1)',
          tension: 0.4,
          fill: true
        }
      ]
    };

    // CPU Usage Chart (Last 24 Hours)
    const cpuData = this.dashboardAnalytics.last24Hours?.cpuUsage || [];
    this.cpuUsageChartData = {
      labels: cpuData.map((d: any) => new Date(d.timestamp).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })),
      datasets: [
        {
          label: 'CPU Usage (%)',
          data: cpuData.map((d: any) => d.value),
          borderColor: '#10b981',
          backgroundColor: 'rgba(16, 185, 129, 0.1)',
          tension: 0.4,
          fill: true
        }
      ]
    };

    // Server Comparison Chart
    const serverStats = this.dashboardAnalytics.serverStats || [];
    this.serverComparisonChartData = {
      labels: serverStats.map((s: any) => s.serverName),
      datasets: [
        {
          label: 'Avg Response Time (ms)',
          data: serverStats.map((s: any) => s.avgResponseTime),
          backgroundColor: [
            '#3b82f6',
            '#10b981',
            '#f59e0b',
            '#ef4444',
            '#8b5cf6'
          ]
        }
      ]
    };
  }

  getKpiStatusClass(status: string): string {
    switch (status) {
      case 'OnTrack': return 'status-success';
      case 'AtRisk': return 'status-warning';
      case 'Critical': return 'status-danger';
      default: return 'status-info';
    }
  }

  getSeverityClass(severity: string): string {
    switch (severity) {
      case 'Low': return 'severity-low';
      case 'Medium': return 'severity-medium';
      case 'High': return 'severity-high';
      case 'Critical': return 'severity-critical';
      default: return 'severity-info';
    }
  }

  refreshDashboard(): void {
    this.loadDashboardData();
  }
}
