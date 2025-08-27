export interface BalanceSummary {
  accountId: string;
  accountName: string;
  year: number;
  month: number;
  totalAmount: number;
  lastUpdatedAt: Date;
  recordCount: number;
  formattedAmount?: string;
  periodDisplay?: string;
}
