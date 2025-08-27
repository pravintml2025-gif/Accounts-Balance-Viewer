import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap, shareReplay, map } from 'rxjs/operators';

import { ConfigService } from './config.service';
import { Balance, BalanceSummary, UploadResponse } from '../models';
import { API_ENDPOINTS } from '../constants/api.constants';
import { CurrencyUtil } from '../../shared/utils/currency.util';

@Injectable({
  providedIn: 'root'
})
export class BalanceService {
  private readonly balancesCache$ = new BehaviorSubject<Balance[]>([]);
  private readonly summariesCache$ = new BehaviorSubject<BalanceSummary[]>([]);

  constructor(
    private http: HttpClient,
    private configService: ConfigService
  ) {}

  getLatestBalances(): Observable<Balance[]> {
    const url = this.configService.buildApiUrl(API_ENDPOINTS.BALANCES.LATEST);
    return this.http.get<Balance[]>(url).pipe(
      tap(balances => this.balancesCache$.next(balances)),
      shareReplay(1)
    );
  }

  getBalancesByPeriod(year: number, month: number): Observable<Balance[]> {
    const url = this.configService.buildApiUrl(API_ENDPOINTS.BALANCES.BY_PERIOD);
    const params = new HttpParams()
      .set('year', year.toString())
      .set('month', month.toString());
    
    return this.http.get<Balance[]>(url, { params });
  }

  uploadBalanceFile(file: File, year: number, month: number): Observable<UploadResponse> {
    const url = this.configService.buildApiUrl(API_ENDPOINTS.BALANCES.UPLOAD);
    const formData = new FormData();
    formData.append('file', file);
    formData.append('year', year.toString());
    formData.append('month', month.toString());

    return this.http.post<UploadResponse>(url, formData);
  }

  // Summary methods
  getSummaries(): Observable<BalanceSummary[]> {
    const url = this.configService.buildApiUrl(API_ENDPOINTS.BALANCES.SUMMARY);
    return this.http.get<BalanceSummary[]>(url).pipe(
      map(summaries => this.transformSummaries(summaries)),
      tap(summaries => this.summariesCache$.next(summaries)),
      shareReplay(1)
    );
  }

  getSummariesByPeriod(year: number, month: number): Observable<BalanceSummary[]> {
    const url = this.configService.buildApiUrl(API_ENDPOINTS.BALANCES.SUMMARY_BY_PERIOD);
    const params = new HttpParams()
      .set('year', year.toString())
      .set('month', month.toString());
    
    return this.http.get<BalanceSummary[]>(url, { params }).pipe(
      map(summaries => this.transformSummaries(summaries))
    );
  }

  // Transform raw API response to include computed properties
  private transformSummaries(summaries: BalanceSummary[]): BalanceSummary[] {
    return summaries.map(summary => this.transformSummary(summary));
  }

  private transformSummary(summary: BalanceSummary): BalanceSummary {
    return {
      ...summary,
      formattedAmount: CurrencyUtil.formatAmount(summary.totalAmount),
      periodDisplay: this.formatPeriodDisplay(summary.year, summary.month),
      lastUpdatedAt: new Date(summary.lastUpdatedAt)
    };
  }

  private formatPeriodDisplay(year: number, month: number): string {
    const monthNames = [
      'January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December'
    ];
    return `${monthNames[month - 1]} ${year}`;
  }

  // Clear cache when data changes
  clearCache(): void {
    this.balancesCache$.next([]);
    this.summariesCache$.next([]);
  }

  // Get current cached summaries
  getCurrentSummariesCache(): Observable<BalanceSummary[]> {
    return this.summariesCache$.asObservable();
  }
}
