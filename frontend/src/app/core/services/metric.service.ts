import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SystemMetric, CreateMetric, MetricQuery } from '../../shared/models/system-metric.model';

@Injectable({
  providedIn: 'root'
})
export class MetricService {
  private readonly apiUrl = `${environment.apiUrl}/metrics`;

  constructor(private http: HttpClient) {}

  createMetric(metric: CreateMetric): Observable<SystemMetric> {
    return this.http.post<SystemMetric>(this.apiUrl, metric);
  }

  getMetrics(query?: MetricQuery): Observable<SystemMetric[]> {
    let params = new HttpParams();

    if (query) {
      if (query.metricName) {
        params = params.set('metricName', query.metricName);
      }
      if (query.source) {
        params = params.set('source', query.source);
      }
      if (query.startDate) {
        params = params.set('startDate', query.startDate.toISOString());
      }
      if (query.endDate) {
        params = params.set('endDate', query.endDate.toISOString());
      }
      if (query.pageNumber) {
        params = params.set('pageNumber', query.pageNumber.toString());
      }
      if (query.pageSize) {
        params = params.set('pageSize', query.pageSize.toString());
      }
    }

    return this.http.get<SystemMetric[]>(this.apiUrl, { params });
  }

  getMetricById(id: number): Observable<SystemMetric> {
    return this.http.get<SystemMetric>(`${this.apiUrl}/${id}`);
  }

  getRecentMetrics(count: number = 100): Observable<SystemMetric[]> {
    const params = new HttpParams().set('count', count.toString());
    return this.http.get<SystemMetric[]>(`${this.apiUrl}/recent`, { params });
  }
}
