# HireFlow API

> A production-ready Recruitment REST API built with .NET 10 and Clean Architecture, designed to manage candidates, recruiters, job postings, and application workflows using the CQRS pattern.

---

## ✨ Overview

HireFlow API is a robust backend system that facilitates the recruitment process between recruiters and candidates. It provides a complete workflow for job publishing and application management while adhering to modern software engineering principles and domain-driven design concepts.

### Main Actors
- **Recruiters**: Can create, update, close, and reopen job postings. They review applications submitted by candidates and can change their status (Accept/Reject).
- **Candidates**: Can browse open jobs, submit job applications (including CVs and cover letters), track their application status, and cancel pending applications.

### Core Workflow
1. Recruiters register and complete their company profile.
2. Recruiters publish new job opportunities.
3. Candidates register and apply to open jobs.
4. Recruiters review applications and update their statuses.
5. Candidates track their application progress via their dashboard.

---

## 🚀 Features

### Authentication & Authorization
- User registration and login.
- JWT-based authentication.
- Role-based authorization (`Candidate`, `Recruiter`).
- Profile management for both actor types.

### Recruitment & Job Management
- **Recruiters**: Full lifecycle management of job postings (Create, Update, Close, Reopen).
- **Candidates**: Search, filter, and view open jobs with pagination.
- **Application Workflow**: Candidates apply to jobs; Recruiters accept or reject them. Candidates can withdraw (cancel) applications.

### Architecture & Patterns
- **Clean Architecture**: Strict separation of concerns (API, Application, Domain, Infrastructure).
- **CQRS**: Command Query Responsibility Segregation implemented using `MediatR`.
- **Result Pattern**: Domain-driven error handling avoiding exception-based control flow.
- **Repository & Unit of Work**: Abstracted data access layer over Entity Framework Core.
- **Validation Pipeline**: Automatic request validation using `FluentValidation` injected into the MediatR pipeline.
- **Centralized Error Handling**: Global exception handling with RFC 7807 `ProblemDetails` responses.

---

## 🏗️ Architecture

The solution follows a strict Clean Architecture layout where dependencies point inwards toward the Domain layer.

```text
HireFlow
├── HireFlow.API              # Presentation layer: Controllers, Middlewares, DI configuration
├── HireFlow.Application      # Application layer: CQRS Handlers, Behaviors, DTOs, Interfaces
├── HireFlow.Domain           # Domain layer: Entities, Value Objects, Enums, Result Pattern
└── HireFlow.Infrastructure   # Infrastructure layer: EF Core, Repositories, JWT, Hashing
```

### CQRS Structure
Inside the `HireFlow.Application` layer, features are organized by Vertical Slices focusing on operations:

```text
Features/
├── Auth/
│   ├── Commands/Register/
│   └── Queries/Login/
├── Jobs/
│   ├── Commands/CreateJob/
│   └── Queries/GetOpenJobs/
...
```
Each operation folder encapsulates the Command/Query, its Handler, and its Validator.

---

## 🛠️ Tech Stack

- **Framework**: .NET 10.0 (ASP.NET Core Web API)
- **Database**: Entity Framework Core with SQL Server
- **CQRS**: MediatR
- **Validation**: FluentValidation
- **Mapping**: Mapster
- **Logging**: Serilog (Console & File sinks)
- **Documentation**: Swagger / OpenAPI (with XML comments and JWT Support)
- **Security**: BCrypt (Password Hashing), Microsoft.AspNetCore.Authentication.JwtBearer

---

## 🚦 Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or Developer edition)

### Configuration

The application requires some configuration in `appsettings.json` or User Secrets. 

Make sure your database connection string is properly set:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=HireFlowDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

The JWT settings and Serilog are preconfigured out-of-the-box in `appsettings.json`.

### Running the Application

1. **Restore dependencies**:
   ```bash
   dotnet restore HireFlow.slnx
   ```

2. **Run database migrations**:
   *(Assuming EF Core tools are installed and migrations exist)*
   ```bash
   dotnet ef database update --project HireFlow.Infrastructure --startup-project HireFlow.API
   ```

3. **Run the API**:
   ```bash
   dotnet run --project HireFlow.API
   ```

4. **Explore the API**:
   Open a browser and navigate to `https://localhost:<port>/swagger` to access the interactive API documentation. You can use the "Authorize" button to test protected endpoints using a generated JWT token.
