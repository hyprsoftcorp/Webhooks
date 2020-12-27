import { Component, OnInit } from '@angular/core';
import { BuildInfo } from '../interfaces/build-info';
import { WebhooksService } from '../services/webhooks.service';

@Component({
  selector: 'app-docs',
  templateUrl: './docs.component.html',
  styleUrls: ['./docs.component.css']
})
export class DocsComponent implements OnInit {

  buildInfo: BuildInfo = { buildDateTimeUtc: new Date(), version: 'Unknown' };

  code = `// Webhooks REST client
var client = new WebhooksClient(new WebhooksHttpClientOptions { ServerBaseUri = new Uri("https://webhooks.hyprsoft.com/") });

// Subscribe
var webhookUri = new Uri("http://webhooks.hyprsoft.com/webhooks/v1/samplecreated");
await client.SubscribeAsync&lt;SampleCreatedWebhookEvent&gt;(webhookUri, x => x.SampleType == 2);

// Publish
await client.PublishAsync(new SampleCreatedWebhookEvent { SampleId = 1, SampleType = 2, UserId = 3, ReferenceId = 4 });

// Unsubscribe
await client.UnsubscribeAsync&lt;SampleCreatedWebhookEvent&gt;(webhookUri);`;

  constructor(private webhooksService: WebhooksService) {
  }

  ngOnInit(): void {
    this.webhooksService.getBuildInfo().subscribe(data => this.buildInfo = data);
  }
}
