import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatChipsModule } from '@angular/material/chips';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { BalanceService } from '../../../../core/services/balance.service';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { BalanceSummary } from '../../../../core/models/balance-summary.model';
import { CurrencyUtil } from '../../../../shared/utils/currency.util';

@Component({
  selector: 'app-balance-summary',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatGridListModule,
    MatChipsModule
  ],
  templateUrl: './balance-summary.component.html',
  styleUrls: ['./balance-summary.component.scss']
})
export class BalanceSummaryComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  
  summaries: BalanceSummary[] = [];
  isLoading = false;
  currentPeriod = '';
  isAdmin = false;

  constructor(
    private balanceService: BalanceService,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {
    // Set current period to current month and year
    const now = new Date();
    const monthNames = [
      'January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December'
    ];
    this.currentPeriod = `${monthNames[now.getMonth()]} ${now.getFullYear()}`;
  }

  ngOnInit(): void {
    this.checkUserRole();
    this.loadSummaries();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private checkUserRole(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.isAdmin = user?.role === 'Admin' || user?.role === 'Administrator';
      });
  }

  private loadSummaries(): void {
    this.isLoading = true;
    
    this.balanceService.getSummaries()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (summaries: BalanceSummary[]) => {
          this.summaries = summaries;
          this.isLoading = false;
        },
        error: (error: any) => {
          console.error('Failed to load balance summaries:', error);
          this.isLoading = false;
          // Only show error notification for refresh action, not initial load
          if (this.summaries.length > 0) {
            this.notificationService.showError('Failed to refresh balance summary data.');
          }
        }
      });
  }

  loadSummariesByPeriod(year: number, month: number): void {
    this.isLoading = true;
    
    this.balanceService.getSummariesByPeriod(year, month)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (summaries: BalanceSummary[]) => {
          this.summaries = summaries;
          this.currentPeriod = this.formatPeriodDisplay(year, month);
          this.isLoading = false;
        },
        error: (error: any) => {
          console.error('Failed to load balance summaries for period:', error);
          this.isLoading = false;
          this.notificationService.showError('Failed to load balance summary data for the specified period.');
        }
      });
  }

  private formatPeriodDisplay(year: number, month: number): string {
    const monthNames = [
      'January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December'
    ];
    return `${monthNames[month - 1]} ${year}`;
  }

  formatAmount(amount: number): string {
    return CurrencyUtil.formatAmount(amount);
  }

  getCurrentMonthYear(): string {
    const now = new Date();
    const monthNames = [
      'January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December'
    ];
    return `${monthNames[now.getMonth()]} ${now.getFullYear()}`;
  }

  getBalanceClass(amount: number): string {
    return CurrencyUtil.getBalanceClass(amount);
  }

  // New methods for enhanced dashboard
  getTotalBalance(): string {
    const total = this.summaries.reduce((sum: number, summary: BalanceSummary) => sum + summary.totalAmount, 0);
    return CurrencyUtil.formatAmount(total);
  }

  getPositiveAccountsCount(): number {
    return this.summaries.filter((summary: BalanceSummary) => summary.totalAmount > 0).length;
  }

  getNegativeAccountsCount(): number {
    return this.summaries.filter((summary: BalanceSummary) => summary.totalAmount < 0).length;
  }

  getAccountIcon(accountName: string): string {
    const name = accountName.toLowerCase();
    if (name.includes('cash') || name.includes('petty')) return 'payments';
    if (name.includes('bank') || name.includes('checking') || name.includes('savings')) return 'account_balance';
    if (name.includes('credit') || name.includes('loan')) return 'credit_card';
    if (name.includes('investment') || name.includes('asset')) return 'trending_up';
    if (name.includes('expense') || name.includes('cost')) return 'money_off';
    return 'account_balance_wallet';
  }

  getStatusIcon(amount: number): string {
    if (amount > 1000000) return 'star';
    if (amount > 0) return 'trending_up';
    if (amount < 0) return 'warning';
    return 'remove_circle';
  }

  getCardClass(amount: number): string {
    if (amount > 1000000) return 'premium-card';
    if (amount > 100000) return 'gold-card';
    if (amount > 0) return 'positive-card';
    if (amount < 0) return 'negative-card';
    return 'neutral-card';
  }

  trackByAccount(index: number, summary: BalanceSummary): string {
    return summary.accountId;
  }

  onRefresh(): void {
    this.loadSummaries();
  }
}
