import { Component, OnInit, OnDestroy } from '@angular/core';
import { HealthSummary } from '../interfaces/health-summary';
import { WebhooksService } from '../services/webhooks.service';
import { interval, Subscription } from 'rxjs';

@Component({
  selector: 'app-health',
  templateUrl: './health.component.html',
  styleUrls: ['./health.component.css']
})
export class HealthComponent implements OnInit, OnDestroy {

  public summary: HealthSummary = { serverStartDateUtc: new Date(), uptime: 'Unknown', successfulWebhooks: [], failedWebhooks: [], publishIntervalMinutes: 60 };
  public successfulWebhooksCount = 0;
  public failedWebhooksCount = 0;
  public pageRefreshSeconds = 60;
  private timer = new Subscription();

  constructor(private webhooksService: WebhooksService) { }

  ngOnInit() {
    this.getHealthSummary();
    this.timer = interval(this.pageRefreshSeconds * 1000).subscribe(() => this.getHealthSummary());
  }

  ngOnDestroy() {
    this.timer.unsubscribe();
  }

  private getHealthSummary() {
    this.webhooksService.getHeathSummary().subscribe(data => {
      this.summary = data;
      this.successfulWebhooksCount = data.successfulWebhooks.reduce((x, y) => x + y.count, 0);
      this.failedWebhooksCount = data.failedWebhooks.reduce((x, y) => x + y.count, 0);
    });
  }
}
