import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, map, catchError, throwError } from 'rxjs';
import { JwtHelperService } from '@auth0/angular-jwt';

import { ConfigService } from './config.service';
import { StorageService } from './storage.service';
import { LoginRequest, LoginResponse, User } from '../models';
import { API_ENDPOINTS } from '../constants/api.constants';
import { APP_CONSTANTS } from '../constants/app.constants';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly jwtHelper = new JwtHelperService();
  private readonly currentUserSubject = new BehaviorSubject<User | null>(null);
  private readonly isLoadingSubject = new BehaviorSubject<boolean>(false);

  // Public observables
  readonly currentUser$ = this.currentUserSubject.asObservable();
  readonly isLoading$ = this.isLoadingSubject.asObservable();
  readonly isAuthenticated$ = this.currentUser$.pipe(
    map(user => !!user && this.isTokenValid())
  );

  constructor(
    private http: HttpClient,
    private configService: ConfigService,
    private storageService: StorageService
  ) {
    this.initializeAuthState();
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    this.isLoadingSubject.next(true);
    const url = this.configService.buildApiUrl(API_ENDPOINTS.AUTH.LOGIN);
    
    return this.http.post<LoginResponse>(url, credentials).pipe(
      map(response => {
        this.handleSuccessfulLogin(response);
        this.isLoadingSubject.next(false);
        return response;
      }),
      catchError(error => {
        this.isLoadingSubject.next(false);
        return throwError(() => error);
      })
    );
  }

  logout(): void {
    this.currentUserSubject.next(null);
    this.storageService.clear();
  }

  isTokenValid(): boolean {
    try {
      const token = this.storageService.getToken();
      if (!token) {
        return false;
      }
      return !this.jwtHelper.isTokenExpired(token);
    } catch (error) {
      console.warn('Error validating token:', error);
      // Clear invalid token
      this.storageService.removeToken();
      return false;
    }
  }

  getToken(): string | null {
    return this.storageService.getToken();
  }

  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    return user?.role === role;
  }

  isAdmin(): boolean {
    return this.hasRole(APP_CONSTANTS.USER_ROLES.ADMIN);
  }

  private initializeAuthState(): void {
    const storedUser = this.storageService.getUser();
    if (storedUser && this.isTokenValid()) {
      this.currentUserSubject.next(storedUser);
    }
  }

  private handleSuccessfulLogin(response: LoginResponse): void {
    this.storageService.setToken(response.accessToken);
    
    // Decode JWT token to extract user information
    try {
      const decodedToken = this.jwtHelper.decodeToken(response.accessToken);
      const user: User = {
        id: decodedToken.sub || decodedToken.nameid || '',
        username: decodedToken.unique_name || decodedToken.name || '',
        email: decodedToken.email || '',
        role: response.roles[0] || 'User',
        isActive: true
      };
      
      this.storageService.setUser(user);
      this.currentUserSubject.next(user);
    } catch (error) {
      console.error('Error decoding token:', error);
      // Fallback: create minimal user object
      const user: User = {
        id: '',
        username: 'Unknown',
        email: '',
        role: response.roles[0] || 'User',
        isActive: true
      };
      
      this.storageService.setUser(user);
      this.currentUserSubject.next(user);
    }
  }
}
