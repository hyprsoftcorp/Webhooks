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

  public summary: HealthSummary = { serverStartDateUtc: new Date(), uptime: 'Unknown', audits: [], publishIntervalMinutes: 60 };
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

  public badgeClassFromAuditType(auditType: string): string {
    switch (auditType.toLowerCase()) {
      case "publish": return "badge bg-success";
      case "subscribe": return "badge bg-primary";
      case "unsubscribe": return "badge bg-secondary";
    }
    return "badge bg-info";
  }

  private getHealthSummary() {
    this.webhooksService.getHeathSummary().subscribe(data => {
      this.summary = data;
    });
  }
}
