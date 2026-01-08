import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Anomaly } from '../../shared/models/anomaly.model';

@Injectable({
  providedIn: 'root'
})
export class AnomalyService {
  private readonly apiUrl = `${environment.apiUrl}/anomalies`;

  constructor(private http: HttpClient) {}

  getAllAnomalies(): Observable<Anomaly[]> {
    return this.http.get<Anomaly[]>(this.apiUrl);
  }

  getUnresolvedAnomalies(): Observable<Anomaly[]> {
    return this.http.get<Anomaly[]>(`${this.apiUrl}/unresolved`);
  }

  detectAnomalies(): Observable<any> {
    return this.http.post(`${this.apiUrl}/detect`, {});
  }

  resolveAnomaly(id: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/resolve`, {});
  }
}
