import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { KpiResult } from '../../shared/models/kpi-result.model';

@Injectable({
  providedIn: 'root'
})
export class KpiService {
  private readonly apiUrl = `${environment.apiUrl}/kpis`;

  constructor(private http: HttpClient) {}

  getAllKpis(): Observable<KpiResult[]> {
    return this.http.get<KpiResult[]>(this.apiUrl);
  }

  getKpisByDateRange(startDate: Date, endDate: Date): Observable<KpiResult[]> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<KpiResult[]>(`${this.apiUrl}/date-range`, { params });
  }

  calculateKpis(): Observable<any> {
    return this.http.post(`${this.apiUrl}/calculate`, {});
  }
}
