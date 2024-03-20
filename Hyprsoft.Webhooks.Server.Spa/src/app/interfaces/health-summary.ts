import { AuditSummary } from "./audit-summary";

export type HealthSummary = {
  serverStartDateUtc: Date;
  publishIntervalMinutes: number;
  uptime: string;
  audits: AuditSummary[];
}
