import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DashboardAnalytics, TimeSeriesData, ServerMetrics } from '../../shared/models/dashboard-analytics.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly apiUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  getDashboardAnalytics(hours: number = 24): Observable<DashboardAnalytics> {
    return this.http.get<DashboardAnalytics>(`${this.apiUrl}/analytics?hours=${hours}`);
  }

  getMetricTrend(metricName: string, hours: number = 24): Observable<TimeSeriesData[]> {
    return this.http.get<TimeSeriesData[]>(`${this.apiUrl}/metrics/${metricName}/trend?hours=${hours}`);
  }

  getServerHealth(): Observable<ServerMetrics[]> {
    return this.http.get<ServerMetrics[]>(`${this.apiUrl}/servers/health`);
  }
}
