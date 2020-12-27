import { Component, OnInit } from '@angular/core';
import { BuildInfo } from '../interfaces/build-info';
import { WebhooksService } from '../services/webhooks.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  buildInfo: BuildInfo = { buildDateTimeUtc: new Date(), version: 'Unknown' };

  constructor(private webhooksService: WebhooksService) {
  }

  ngOnInit(): void {
    this.webhooksService.getBuildInfo().subscribe(data => this.buildInfo = data);
  }

}
