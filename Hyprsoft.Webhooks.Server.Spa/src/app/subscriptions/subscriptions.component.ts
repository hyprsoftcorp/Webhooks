import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Subscription } from '../interfaces/subscription';
import { WebhooksService } from '../services/webhooks.service';

@Component({
  selector: 'app-subscriptions',
  templateUrl: './subscriptions.component.html',
  styleUrls: ['./subscriptions.component.css'],
  imports: [ CommonModule ],
  standalone: true
})
export class SubscriptionsComponent implements OnInit {

  public subscriptions: Subscription[] = [];

  constructor(private webhooksService: WebhooksService) { }

  ngOnInit(): void {
    this.webhooksService.getSubscriptions().subscribe(data => this.subscriptions = data);
  }
}
