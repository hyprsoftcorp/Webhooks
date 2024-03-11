import { SuccessfulWebhook } from "./successful-webhook";
import { FailedWebhook } from "./failed-webhook";

export interface HealthSummary {
  serverStartDateUtc: Date;
  publishIntervalMinutes: number;
  uptime: string;
  successfulWebhooks: SuccessfulWebhook[];
  failedWebhooks: FailedWebhook[];
}
