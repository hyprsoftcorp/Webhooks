import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Subscription } from '../interfaces/subscription';
import { BuildInfo } from '../interfaces/build-info';
import { HealthSummary } from '../interfaces/health-summary';

@Injectable({
  providedIn: 'root'
})
export class WebhooksService {

  constructor(private httpClient: HttpClient) { }

  getBuildInfo(): Observable<BuildInfo> {
    return this.httpClient.get<BuildInfo>('/api/v1/buildinfo');
  }

  getSubscriptions(): Observable<Subscription[]> {
    return this.httpClient.get<Subscription[]>('/api/v1/subscriptions');
  }

  getHeathSummary(): Observable<HealthSummary> {
    return this.httpClient.get<HealthSummary>('/api/v1/health');
  }
}
