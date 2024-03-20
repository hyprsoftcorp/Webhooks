export type Subscription = {
  subscriptionId: number;
  eventName: string;
  createdUtc: Date;
  webhookUri: string;
  filterExpression: string;
  filter: string;
  isActive: boolean;
}
