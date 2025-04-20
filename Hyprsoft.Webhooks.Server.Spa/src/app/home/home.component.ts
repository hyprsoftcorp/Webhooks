import { Component, OnInit, VERSION } from '@angular/core';
import { DatePipe } from '@angular/common';
import { BuildInfo } from '../interfaces/build-info';
import { WebhooksService } from '../services/webhooks.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  imports: [ DatePipe ],
  standalone: true
})
export class HomeComponent implements OnInit {

  public buildInfo: BuildInfo = { buildDateTimeUtc: new Date(), version: '0.0.0', dotnetFrameworkVersion: '.NET Core', angularVersion: VERSION.full };

  constructor(private webhooksService: WebhooksService) {
  }

  ngOnInit(): void {
    this.webhooksService.getBuildInfo().subscribe(data => this.buildInfo = { ...this.buildInfo, ...data });
  }

}
