import { Component, OnInit, VERSION } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BuildInfo } from '../interfaces/build-info';
import { WebhooksService } from '../services/webhooks.service';
import { Highlight } from 'ngx-highlightjs';
import { HighlightLineNumbers } from 'ngx-highlightjs/line-numbers';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-docs',
  templateUrl: './docs.component.html',
  styleUrls: ['./docs.component.css'],
  imports: [Highlight, HighlightLineNumbers, DatePipe, RouterLink],
  standalone: true
})
export class DocsComponent implements OnInit {

  public buildInfo: BuildInfo = { buildDateTimeUtc: new Date(), version: '0.0.0', dotnetFrameworkVersion: '.NET Core', angularVersion: VERSION.full };

  public code = `using Hyprsoft.Webhooks.Client;

  // Webhooks REST client
  var client = new WebhooksClient(options => options.ServerBaseUri = new Uri("https://webhooks.hyprsoft.com/"));

  // Subscribe (a REST controller is needed to host the webhook callback endpoints)
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
