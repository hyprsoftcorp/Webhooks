import { SuccessfulWebhook } from "./successful-webhook";

export interface FailedWebhook extends SuccessfulWebhook {
  webhookUri: string;
  error: string;
}
