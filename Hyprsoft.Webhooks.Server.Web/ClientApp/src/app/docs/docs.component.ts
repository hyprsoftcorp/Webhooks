import { Component, OnInit } from '@angular/core';
import { BuildInfo } from '../interfaces/build-info';
import { WebhooksService } from '../services/webhooks.service';
import { HighlightModule } from 'ngx-highlightjs';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-docs',
  templateUrl: './docs.component.html',
  styleUrls: ['./docs.component.css'],
  imports: [HighlightModule, DatePipe],
  standalone: true
})
export class DocsComponent implements OnInit {

  public buildInfo: BuildInfo = { buildDateTimeUtc: new Date(), version: 'Unknown' };

  public code = ` // Webhooks REST client
  var client = new WebhooksClient(new WebhooksHttpClientOptions { ServerBaseUri = new Uri("https://webhooks.hyprsoft.com/") });

  // Subscribe
  var webhookUri = new Uri("https://office.hyprsoft.com/webhooks/v1/ping");
  await client.SubscribeAsync<PingWebhookEvent>(webhookUri);

  // Publish
  await client.PublishAsync(new PingWebhookEvent());

  // Unsubscribe
  await client.UnsubscribeAsync<PingWebhookEvent>(webhookUri);
`;

  constructor(private webhooksService: WebhooksService) {
  }

  ngOnInit(): void {
    this.webhooksService.getBuildInfo().subscribe(data => this.buildInfo = data);
  }
}
