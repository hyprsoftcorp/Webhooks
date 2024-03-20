export type AuditSummary = {
  eventName: string;
  webhookUri?: string;
  auditType: string;
  error?: string;
  count: number;
}
