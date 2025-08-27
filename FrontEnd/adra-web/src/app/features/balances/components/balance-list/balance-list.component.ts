import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { Observable, Subject } from 'rxjs';
import { takeUntil, map } from 'rxjs/operators';

import { BalanceService } from '../../../../core/services/balance.service';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { Balance } from '../../../../core/models';
import { CurrencyUtil } from '../../../../shared/utils/currency.util';

@Component({
  selector: 'app-balance-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatToolbarModule,
    MatFormFieldModule,
    MatSelectModule
  ],
  templateUrl: './balance-list.component.html',
  styleUrls: ['./balance-list.component.scss']
})
export class BalanceListComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  
  displayedColumns: string[] = ['account', 'amount', 'period', 'lastUpdated'];
  dataSource: Balance[] = [];
  isLoading = false;
  isAdmin = false;
  private isInitialLoad = true;
  
  periodForm: FormGroup;
  years: number[] = [];
  months = [
    { value: 1, name: 'January' },
    { value: 2, name: 'February' },
    { value: 3, name: 'March' },
    { value: 4, name: 'April' },
    { value: 5, name: 'May' },
    { value: 6, name: 'June' },
    { value: 7, name: 'July' },
    { value: 8, name: 'August' },
    { value: 9, name: 'September' },
    { value: 10, name: 'October' },
    { value: 11, name: 'November' },
    { value: 12, name: 'December' }
  ];
  
  constructor(
    private balanceService: BalanceService,
    private authService: AuthService,
    private fb: FormBuilder,
    private notificationService: NotificationService
  ) {
    // Initialize years array (last 5 years)
    const currentYear = new Date().getFullYear();
    for (let year = currentYear; year >= currentYear - 4; year--) {
      this.years.push(year);
    }
    
    // Initialize form with current year and month
    const currentDate = new Date();
    this.periodForm = this.fb.group({
      year: [currentDate.getFullYear()],
      month: [currentDate.getMonth() + 1]
    });
  }

  ngOnInit(): void {
    this.checkUserRole();
    
    // Load initial data
    this.loadBalancesByPeriod();
    
    // Listen to form changes (without startWith to avoid double loading)
    this.periodForm.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.isInitialLoad = false; // Subsequent changes are not initial load
        this.loadBalancesByPeriod();
      });
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

  private loadBalancesByPeriod(): void {
    if (!this.periodForm.valid) return;
    
    this.isLoading = true;
    const { year, month } = this.periodForm.value;
    
    this.balanceService.getBalancesByPeriod(year, month)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (balances) => {
          this.dataSource = balances;
          this.isLoading = false;
          
          // Only show notification for manual period changes (not initial load) and when no data
          if (balances.length === 0 && !this.isInitialLoad) {
            const monthName = this.months.find(m => m.value === month)?.name;
            this.notificationService.showInfo(`No balance data found for ${monthName} ${year}`);
          }
          
          this.isInitialLoad = false;
        },
        error: (error) => {
          this.isLoading = false;
          this.dataSource = [];
          
          // System-level errors (401, 500, network) are handled by interceptor
          // Handle remaining client errors that components should manage
          if (error.status === 400) {
            this.notificationService.showWarning(error.error?.message || 'Invalid request parameters');
          } else if (error.status === 404) {
            // This shouldn't happen anymore with the API changes, but keep as fallback
            const monthName = this.months.find(m => m.value === month)?.name;
            this.notificationService.showInfo(`No balance data found for ${monthName} ${year}`);
          }
          // Other errors (401, 500, network) are already handled by interceptor
          
          console.error('Error loading balances:', error);
          this.isInitialLoad = false;
        }
      });
  }

  onRefresh(): void {
    this.loadBalancesByPeriod();
  }

  trackByAccountId(index: number, balance: Balance): string {
    return balance.id;
  }

  formatAmount(amount: number): string {
    return CurrencyUtil.formatAmount(amount);
  }

  formatPeriod(year: number, month: number): string {
    const date = new Date(year, month - 1);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'long' });
  }
}
