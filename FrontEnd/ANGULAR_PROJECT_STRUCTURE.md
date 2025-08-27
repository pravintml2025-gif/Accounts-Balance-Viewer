# 🏗️ Angular Project Structure - Accounts Balance Viewer

## 📋 Overview

This document outlines the refined Angular project structure following Clean Architecture principles, SOLID design patterns, and Angular best practices. The structure emphasizes the **Facade pattern** for better orchestration and maintains perfect alignment with the .NET Core backend architecture.

## 🎯 Architecture Philosophy

### Clean Architecture Alignment

```
Frontend (Angular)          Backend (.NET)
├── core/                   ↔ Adra.Core/           # Domain & Infrastructure
├── features/               ↔ Adra.Application/    # Use Cases & Services
├── shared/                 ↔ Adra.Infrastructure/ # Shared Components
└── environments/           ↔ Adra.Api/           # Configuration
```

### Key Architectural Patterns

- **Facade Pattern**: Orchestrates complex operations in feature modules
- **Singleton Pattern**: Core services are application-wide singletons
- **Module Pattern**: Feature-based lazy-loaded modules
- **Repository Pattern**: Abstracted data access through services

## 📁 Refined Project Structure

```
src/app/
├── core/                           # 🎯 Cross-cutting, singleton services
│   ├── guards/
│   │   ├── auth.guard.ts          # Route protection for authenticated users
│   │   ├── admin.guard.ts         # Admin-only route protection
│   │   └── guest.guard.ts         # Guest-only routes (login, etc.)
│   ├── interceptors/
│   │   ├── jwt.interceptor.ts     # JWT token attachment
│   │   ├── error.interceptor.ts   # Global error handling
│   │   └── loading.interceptor.ts # Global loading state management
│   ├── models/                     # 📊 Core domain models
│   │   ├── user.model.ts          # User entity and auth models
│   │   ├── balance.model.ts       # Balance domain models
│   │   ├── api-response.model.ts  # API response contracts
│   │   └── index.ts               # Barrel exports for clean imports
│   ├── services/                   # 🔧 Core application services
│   │   ├── auth.service.ts        # Authentication + JWT refresh logic
│   │   ├── balance.service.ts     # Balance API operations
│   │   ├── config.service.ts      # Environment configuration
│   │   ├── storage.service.ts     # Local storage abstraction
│   │   └── notification.service.ts # Toast notifications
│   ├── constants/                  # 📋 Application constants
│   │   ├── app.constants.ts       # App-wide constants
│   │   └── api.constants.ts       # API endpoints and configs
│   └── core.module.ts             # Core module with singleton providers
├── features/                       # 🚀 Feature modules (lazy-loaded business logic)
│   ├── auth/
│   │   ├── components/
│   │   │   └── login/
│   │   │       ├── login.component.ts
│   │   │       ├── login.component.html
│   │   │       ├── login.component.scss
│   │   │       └── login.component.spec.ts
│   │   ├── services/
│   │   │   └── auth.facade.ts     # 🎭 Facade for auth orchestration
│   │   ├── models/
│   │   │   └── login.model.ts     # Feature-specific models
│   │   ├── auth-routing.module.ts
│   │   └── auth.module.ts
│   ├── balances/
│   │   ├── components/
│   │   │   ├── balance-list/
│   │   │   │   ├── balance-list.component.ts
│   │   │   │   ├── balance-list.component.html
│   │   │   │   ├── balance-list.component.scss
│   │   │   │   └── balance-list.component.spec.ts
│   │   │   ├── balance-upload/    # 👨‍💼 Admin only feature
│   │   │   │   ├── balance-upload.component.ts
│   │   │   │   ├── balance-upload.component.html
│   │   │   │   ├── balance-upload.component.scss
│   │   │   │   └── balance-upload.component.spec.ts
│   │   │   └── balance-filter/
│   │   │       ├── balance-filter.component.ts
│   │   │       ├── balance-filter.component.html
│   │   │       ├── balance-filter.component.scss
│   │   │       └── balance-filter.component.spec.ts
│   │   ├── services/
│   │   │   └── balance.facade.ts  # 🎭 Facade for balance orchestration
│   │   ├── models/
│   │   │   └── balance-feature.model.ts
│   │   ├── balances-routing.module.ts
│   │   └── balances.module.ts
│   └── reports/                    # 📊 Optional admin-only feature (stretch goal)
│       ├── components/
│       │   └── reports/
│       │       ├── reports.component.ts
│       │       ├── reports.component.html
│       │       ├── reports.component.scss
│       │       └── reports.component.spec.ts
│       ├── services/
│       │   └── reports.facade.ts
│       ├── reports-routing.module.ts
│       └── reports.module.ts
├── shared/                         # 🔧 Reusable dumb components
│   ├── components/
│   │   ├── navbar/
│   │   │   ├── navbar.component.ts
│   │   │   ├── navbar.component.html
│   │   │   ├── navbar.component.scss
│   │   │   └── navbar.component.spec.ts
│   │   ├── loading-spinner/
│   │   │   ├── loading-spinner.component.ts
│   │   │   ├── loading-spinner.component.html
│   │   │   ├── loading-spinner.component.scss
│   │   │   └── loading-spinner.component.spec.ts
│   │   └── file-upload/
│   │       ├── file-upload.component.ts
│   │       ├── file-upload.component.html
│   │       ├── file-upload.component.scss
│   │       └── file-upload.component.spec.ts
│   ├── pipes/
│   │   ├── currency-format.pipe.ts   # Rs. x,xxx/= formatting
│   │   └── date-format.pipe.ts       # Date formatting
│   └── shared.module.ts
├── layout/                          # 📐 Application layout structure
│   ├── header/
│   │   ├── header.component.ts
│   │   ├── header.component.html
│   │   ├── header.component.scss
│   │   └── header.component.spec.ts
│   ├── sidebar/
│   │   ├── sidebar.component.ts
│   │   ├── sidebar.component.html
│   │   ├── sidebar.component.scss
│   │   └── sidebar.component.spec.ts
│   ├── footer/
│   │   ├── footer.component.ts
│   │   ├── footer.component.html
│   │   ├── footer.component.scss
│   │   └── footer.component.spec.ts
│   └── layout.component.ts
├── environments/                    # ⚙️ Environment configurations
│   ├── environment.ts              # Development configuration
│   └── environment.prod.ts         # Production configuration
├── app-routing.module.ts           # Main application routing
├── app.module.ts                   # Root application module
└── app.component.ts                # Root application component
```

## 🎭 Facade Pattern Implementation

### Why Facade Pattern?

The Facade pattern provides a simplified interface to complex subsystems, making feature modules more maintainable and testable.

### Auth Facade Example

```typescript
// filepath: src/app/features/auth/services/auth.facade.ts
import { Injectable } from "@angular/core";
import { Observable, BehaviorSubject } from "rxjs";
import { tap, catchError } from "rxjs/operators";

import { AuthService } from "@core/services/auth.service";
import { NotificationService } from "@core/services/notification.service";
import { StorageService } from "@core/services/storage.service";
import { LoginRequest, LoginResponse } from "../models/login.model";
import { User } from "@core/models";

@Injectable()
export class AuthFacade {
  private readonly currentUserSubject = new BehaviorSubject<User | null>(null);

  // Public observables
  readonly currentUser$ = this.currentUserSubject.asObservable();
  readonly isAuthenticated$ = this.authService.isAuthenticated$;
  readonly isLoading$ = this.authService.isLoading$;

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private storageService: StorageService
  ) {
    this.initializeAuthState();
  }

  /**
   * Orchestrates the login process
   */
  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.authService.login(credentials).pipe(
      tap((response) => {
        this.handleSuccessfulLogin(response);
      }),
      catchError((error) => {
        this.handleLoginError(error);
        throw error;
      })
    );
  }

  /**
   * Orchestrates the logout process
   */
  logout(): void {
    this.authService.logout();
    this.currentUserSubject.next(null);
    this.storageService.clear();
    this.notificationService.showSuccess("Logged out successfully");
  }

  /**
   * Checks if user has specific role
   */
  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    return user?.role === role;
  }

  /**
   * Checks if current user is admin
   */
  isAdmin(): boolean {
    return this.hasRole("Admin");
  }

  private initializeAuthState(): void {
    const storedUser = this.storageService.getUser();
    if (storedUser && this.authService.isTokenValid()) {
      this.currentUserSubject.next(storedUser);
    }
  }

  private handleSuccessfulLogin(response: LoginResponse): void {
    this.storageService.setToken(response.token);
    this.storageService.setUser(response.user);
    this.currentUserSubject.next(response.user);
    this.notificationService.showSuccess("Login successful");
  }

  private handleLoginError(error: any): void {
    this.notificationService.showError(
      error.message || "Login failed. Please try again."
    );
  }
}
```

### Balance Facade Example

```typescript
// filepath: src/app/features/balances/services/balance.facade.ts
import { Injectable } from "@angular/core";
import { Observable, BehaviorSubject, combineLatest } from "rxjs";
import { map, tap, catchError, switchMap } from "rxjs/operators";

import { BalanceService } from "@core/services/balance.service";
import { NotificationService } from "@core/services/notification.service";
import { Balance } from "@core/models";
import { BalanceFilter } from "../models/balance-feature.model";

@Injectable()
export class BalanceFacade {
  private readonly balancesSubject = new BehaviorSubject<Balance[]>([]);
  private readonly filterSubject = new BehaviorSubject<BalanceFilter>({});
  private readonly loadingSubject = new BehaviorSubject<boolean>(false);

  // Public observables
  readonly balances$ = this.balancesSubject.asObservable();
  readonly filter$ = this.filterSubject.asObservable();
  readonly isLoading$ = this.loadingSubject.asObservable();

  // Computed observables
  readonly filteredBalances$ = combineLatest([
    this.balances$,
    this.filter$,
  ]).pipe(map(([balances, filter]) => this.applyFilter(balances, filter)));

  constructor(
    private balanceService: BalanceService,
    private notificationService: NotificationService
  ) {}

  /**
   * Loads latest balances
   */
  loadLatestBalances(): void {
    this.loadingSubject.next(true);

    this.balanceService
      .getLatestBalances()
      .pipe(
        tap((balances) => {
          this.balancesSubject.next(balances);
          this.loadingSubject.next(false);
        }),
        catchError((error) => {
          this.handleError("Failed to load balances", error);
          this.loadingSubject.next(false);
          throw error;
        })
      )
      .subscribe();
  }

  /**
   * Loads balances for specific period
   */
  loadBalancesByPeriod(year: number, month: number): void {
    this.loadingSubject.next(true);

    this.balanceService
      .getBalancesByPeriod(year, month)
      .pipe(
        tap((balances) => {
          this.balancesSubject.next(balances);
          this.loadingSubject.next(false);
        }),
        catchError((error) => {
          this.handleError(
            "Failed to load balances for selected period",
            error
          );
          this.loadingSubject.next(false);
          throw error;
        })
      )
      .subscribe();
  }

  /**
   * Orchestrates file upload process
   */
  uploadBalanceFile(file: File, year: number, month: number): Observable<any> {
    this.loadingSubject.next(true);

    return this.balanceService.uploadBalanceFile(file, year, month).pipe(
      tap(() => {
        this.notificationService.showSuccess(
          "Balance file uploaded successfully"
        );
        // Refresh balances after upload
        this.loadLatestBalances();
      }),
      catchError((error) => {
        this.handleError("Failed to upload balance file", error);
        this.loadingSubject.next(false);
        throw error;
      })
    );
  }

  /**
   * Updates current filter
   */
  updateFilter(filter: BalanceFilter): void {
    this.filterSubject.next({ ...this.filterSubject.value, ...filter });
  }

  /**
   * Clears current filter
   */
  clearFilter(): void {
    this.filterSubject.next({});
  }

  private applyFilter(balances: Balance[], filter: BalanceFilter): Balance[] {
    return balances.filter((balance) => {
      if (filter.year && balance.year !== filter.year) return false;
      if (filter.month && balance.month !== filter.month) return false;
      if (
        filter.account &&
        !balance.account.toLowerCase().includes(filter.account.toLowerCase())
      )
        return false;
      return true;
    });
  }

  private handleError(message: string, error: any): void {
    console.error(message, error);
    this.notificationService.showError(message);
  }
}
```

## 🏗️ Core Module Configuration

```typescript
// filepath: src/app/core/core.module.ts
import { NgModule, Optional, SkipSelf } from "@angular/core";
import { CommonModule } from "@angular/common";
import { HTTP_INTERCEPTORS } from "@angular/common/http";

// Interceptors
import { JwtInterceptor } from "./interceptors/jwt.interceptor";
import { ErrorInterceptor } from "./interceptors/error.interceptor";
import { LoadingInterceptor } from "./interceptors/loading.interceptor";

// Services
import { AuthService } from "./services/auth.service";
import { BalanceService } from "./services/balance.service";
import { ConfigService } from "./services/config.service";
import { StorageService } from "./services/storage.service";
import { NotificationService } from "./services/notification.service";

@NgModule({
  imports: [CommonModule],
  providers: [
    // HTTP Interceptors
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: LoadingInterceptor,
      multi: true,
    },
    // Core Services (Singletons)
    AuthService,
    BalanceService,
    ConfigService,
    StorageService,
    NotificationService,
  ],
})
export class CoreModule {
  constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
    if (parentModule) {
      throw new Error(
        "CoreModule is already loaded. Import it in AppModule only."
      );
    }
  }
}
```

## 🚀 Feature Module Example

```typescript
// filepath: src/app/features/balances/balances.module.ts
import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule } from "@angular/forms";

import { SharedModule } from "@shared/shared.module";
import { BalancesRoutingModule } from "./balances-routing.module";

// Components
import { BalanceListComponent } from "./components/balance-list/balance-list.component";
import { BalanceUploadComponent } from "./components/balance-upload/balance-upload.component";
import { BalanceFilterComponent } from "./components/balance-filter/balance-filter.component";

// Services (Facade)
import { BalanceFacade } from "./services/balance.facade";

@NgModule({
  declarations: [
    BalanceListComponent,
    BalanceUploadComponent,
    BalanceFilterComponent,
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    SharedModule,
    BalancesRoutingModule,
  ],
  providers: [
    BalanceFacade, // Feature-scoped service
  ],
})
export class BalancesModule {}
```

## 📊 Lazy Loading Configuration

```typescript
// filepath: src/app/app-routing.module.ts
import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { AuthGuard } from "@core/guards/auth.guard";
import { AdminGuard } from "@core/guards/admin.guard";
import { GuestGuard } from "@core/guards/guest.guard";

const routes: Routes = [
  {
    path: "",
    redirectTo: "/auth/login",
    pathMatch: "full",
  },
  {
    path: "auth",
    loadChildren: () =>
      import("./features/auth/auth.module").then((m) => m.AuthModule),
    canActivate: [GuestGuard], // Only for non-authenticated users
  },
  {
    path: "balances",
    loadChildren: () =>
      import("./features/balances/balances.module").then(
        (m) => m.BalancesModule
      ),
    canActivate: [AuthGuard], // Requires authentication
  },
  {
    path: "reports",
    loadChildren: () =>
      import("./features/reports/reports.module").then((m) => m.ReportsModule),
    canActivate: [AdminGuard], // Admin only
  },
  {
    path: "**",
    redirectTo: "/auth/login",
  },
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, {
      enableTracing: false, // Set to true for debugging
      preloadingStrategy: PreloadAllModules,
    }),
  ],
  exports: [RouterModule],
})
export class AppRoutingModule {}
```

## 🔧 Component Communication Pattern

### Smart Component (Container) with Facade

```typescript
// filepath: src/app/features/balances/components/balance-list/balance-list.component.ts
import { Component, OnInit, ChangeDetectionStrategy } from "@angular/core";
import { Observable } from "rxjs";

import { Balance } from "@core/models";
import { BalanceFacade } from "../../services/balance.facade";
import { BalanceFilter } from "../../models/balance-feature.model";

@Component({
  selector: "app-balance-list",
  templateUrl: "./balance-list.component.html",
  styleUrls: ["./balance-list.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BalanceListComponent implements OnInit {
  // Reactive state from facade
  balances$: Observable<Balance[]> = this.balanceFacade.filteredBalances$;
  isLoading$: Observable<boolean> = this.balanceFacade.isLoading$;
  filter$: Observable<BalanceFilter> = this.balanceFacade.filter$;

  constructor(private balanceFacade: BalanceFacade) {}

  ngOnInit(): void {
    this.balanceFacade.loadLatestBalances();
  }

  onFilterChange(filter: BalanceFilter): void {
    this.balanceFacade.updateFilter(filter);
  }

  onPeriodSelected(year: number, month: number): void {
    this.balanceFacade.loadBalancesByPeriod(year, month);
  }

  onRefresh(): void {
    this.balanceFacade.loadLatestBalances();
  }
}
```

### Dumb Component (Presentational)

```typescript
// filepath: src/app/shared/components/balance-table/balance-table.component.ts
import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
} from "@angular/core";

import { Balance } from "@core/models";

@Component({
  selector: "app-balance-table",
  templateUrl: "./balance-table.component.html",
  styleUrls: ["./balance-table.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BalanceTableComponent {
  @Input() balances: Balance[] = [];
  @Input() loading = false;
  @Input() displayedColumns: string[] = [
    "account",
    "amount",
    "period",
    "actions",
  ];

  @Output() refresh = new EventEmitter<void>();
  @Output() edit = new EventEmitter<Balance>();
  @Output() delete = new EventEmitter<Balance>();

  trackByAccountId(index: number, balance: Balance): string {
    return balance.id;
  }

  onRefresh(): void {
    this.refresh.emit();
  }

  onEdit(balance: Balance): void {
    this.edit.emit(balance);
  }

  onDelete(balance: Balance): void {
    this.delete.emit(balance);
  }
}
```

## 🎯 Key Benefits of This Structure

### 1. **Facade Pattern Benefits**

- **Simplified API**: Components interact with simple facade methods
- **Better Testing**: Easy to mock facade for unit tests
- **Orchestration**: Complex business logic centralized in facades
- **Loose Coupling**: Components don't know about multiple services

### 2. **Clean Separation of Concerns**

- **Core**: Cross-cutting concerns and singleton services
- **Features**: Business logic grouped by feature
- **Shared**: Reusable presentational components
- **Layout**: Application structure components

### 3. **Scalability & Maintainability**

- **Lazy Loading**: Features loaded on demand
- **Module Boundaries**: Clear feature boundaries
- **Team Development**: Different teams can work on different features
- **Code Reusability**: Shared components across features

## 🚀 Implementation Priority

### Phase 1: Foundation Setup

```bash
# 1. Create Angular project
ng new adra-web --routing --style=scss --strict

# 2. Install dependencies
npm install @angular/material @angular/cdk @angular/animations
npm install @auth0/angular-jwt ngx-toastr

# 3. Generate core structure
ng generate module core
ng generate service core/services/auth
ng generate service core/services/config
ng generate guard core/guards/auth
ng generate interceptor core/interceptors/jwt
```

### Phase 2: Feature Implementation

```bash
# 4. Generate feature modules
ng generate module features/auth --routing
ng generate component features/auth/components/login
ng generate service features/auth/services/auth-facade

ng generate module features/balances --routing
ng generate component features/balances/components/balance-list
ng generate service features/balances/services/balance-facade
```

### Phase 3: Shared Components

```bash
# 5. Generate shared components
ng generate module shared
ng generate component shared/components/navbar
ng generate component shared/components/loading-spinner
ng generate pipe shared/pipes/currency-format
```

This refined structure with the Facade pattern provides excellent separation of concerns, maintainability, and follows Angular best practices while perfectly aligning with your Clean Architecture .NET backend! 🎯

---

_This structure serves as the definitive blueprint for building a professional, maintainable Angular application using the Facade pattern and Clean Architecture principles._
