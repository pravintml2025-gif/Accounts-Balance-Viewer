# Accounts Balance Viewer

A full-stack .NET 9 Web API application with Angular 18 frontend built using Clean Architecture. It manages account balances with JWT authentication, role-based authorization, and secure file upload capabilities.

# Architecture

This project follows Clean Architecture principles with a clear separation of concerns.

# Backend (.NET 9)

Adra.Api: ASP.NET Core Web API (Presentation layer)

Adra.Application: Application layer (Use cases, DTOs, Services)

Adra.Core: Core layer (Entities, Interfaces, Domain models)

Adra.Infrastructure: Infrastructure layer (EF Core, Repositories, Data access)

Tests: Unit tests (27 tests, all passing)

# Frontend (Angular 18)

Angular 18: Standalone components, Angular Material UI

Clean Architecture: Core services, feature modules, shared components

Lazy Loading: Optimized routing and module loading

# Technologies

# Backend

.NET 9.0.203

Entity Framework Core

ASP.NET Core Identity

JWT Bearer Authentication

EPPlus (Excel file processing)

Serilog (structured logging)

Swagger

xUnit + FluentAssertions (unit testing)

# Frontend

Angular 18.2.13

Angular Material 18.2.14

RxJS 7.8.0

JWT Authentication (@auth0/angular-jwt)

NgxToastr (notifications)

TypeScript 5.5.4

# Features

# Authentication & Authorization

JWT token-based authentication (120-minute expiry)

Role-based authorization (Admin/User roles)

ASP.NET Core Identity integration

Secure password policies

# Account Balance Management

View the latest month's balances

Historical balance tracking by period

Real-time balance summaries

File Upload & Processing

Multi-format support: CSV, TSV, Excel (.xlsx)

Comprehensive validation: account existence, data integrity

Detailed reporting: success/error tracking with line-by-line feedback

Admin-only access for uploads

# Assumptions & Notes

Uploads are restricted to predefined accounts listed in the document.

Non-predefined accounts are ignored and not imported.

Each new upload inserts or updates balances for the current month.

If multiple uploads exist for the same month, the last upload is taken as final.

The landing page displays the sum of all balances for the current month.

"View All Records" shows monthly balances with filters by year and month.

Upload Balance Data supports TSV, Excel, and CSV formats.

# Prerequisites

# Development Environment

.NET 9.0.203 SDK

Node.js 22.15.0 or later

npm 10.9.2 or later

Angular CLI 18.2.20

SQL Server (LocalDB, Express, or full instance)

Visual Studio Code or Visual Studio 2022

# The project is hosted on GitHub and can be cloned using the following URL:

# Database Configuration

Update appsettings.Development with your SQL Server settings.

# Fron end Configuration

Update the Angular environment file with your API URL.

# Database Setup

This will create the database, apply migrations, and seed sample data.

Seeded data includes:

Users: admin@adra.com/Admin@123, john.doe@adra.com/User@123

business accounts (R&D, Canteen, CEO’s car, Marketing, Parking fines)

months of balance history

# Tests

The Tests project contains all backend unit tests. Running the backend project will automatically execute all test cases to verify functionality.

# Default Login Credentials

Admin: admin@adra.com / Admin@123

User: john.doe@adra.com / User@123
