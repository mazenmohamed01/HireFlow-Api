# HireFlow — Phase 0 Audit Report

> **Status:** WAITING FOR APPROVAL — no code has been changed.  
> **Date:** 2026-09-18  
> **Auditor:** Antigravity (senior .NET backend engineer role)

---

## 1. Current Structure Summary

### 1.1 Solution File

| Item | Current Value |
|---|---|
| Solution file | `JobApplication.API.slnx` (root) |
| Solution name / project prefix | `JobApplication.*` (not `HireFlow.*`) |
| Folder names | `HireFlow.*` (mismatch with solution/namespace) |

> [!WARNING]
> The **folder names** are `HireFlow.*` but the **project files, namespaces and solution** all use `JobApplication.*`. This naming split must be resolved (see Q-01).

### 1.2 Projects & References

| Project (folder) | Assembly name | Target | References |
|---|---|---|---|
| `HireFlow.Domain` | `JobApplication.Domain` | net10.0 | — (clean) |
| `HireFlow.Application` | `JobApplication.Application` | net10.0 | Domain |
| `HireFlow.Infrastructure` | `JobApplication.Infrastructure` | net10.0 | Application, Domain |
| `HireFlow.API` | `JobApplication.API` | net10.0 | Infrastructure |

Reference graph is structurally correct; no upward leaks at the project-reference level.

### 1.3 Packages & Versions

| Package | Project | Version |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | Infrastructure | 10.0.12 |
| `Microsoft.EntityFrameworkCore.SqlServer` | Infrastructure | 10.0.12 |
| `Microsoft.EntityFrameworkCore.Tools` | Infrastructure | 10.0.12 |
| `Microsoft.EntityFrameworkCore.Design` | API | 10.0.12 |
| `Microsoft.AspNetCore.OpenApi` | API | 10.0.11 |
| `Scalar.AspNetCore` | API | 2.17.4 |

**Missing packages** (required by the plan, not yet added):

| Package | Layer | Purpose |
|---|---|---|
| `Mapster` | Application | Entity → DTO mapping |
| `Mapster.DependencyInjection` | Application | `IMapper` / `ServiceMapper` DI |
| `FluentValidation` | Application | Request validators |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | API | JWT bearer auth |
| `xUnit` + `FluentAssertions` | Test projects | Unit and integration tests |
| `Microsoft.AspNetCore.Mvc.Testing` | Test projects | `WebApplicationFactory` |

> [!NOTE]
> No test projects exist at all in the solution. They must be created.

### 1.4 Folder Structure

```
HireFlow/
├── HireFlow.Domain/
│   ├── Entities/           Candidate.cs, Job.cs, JobCandidateApplication.cs
│   ├── Enums/              JobApplicationStatus.cs
│   └── Class1.cs           (scaffold stub — unused)
├── HireFlow.Application/
│   ├── DTOs/               CreateJobDto.cs
│   ├── Interfaces/         IJobRepository.cs
│   ├── Services/           JobService.cs
│   └── Class1.cs           (scaffold stub — unused)
├── HireFlow.Infrastructure/
│   ├── Migrations/         20260916185448_init + snapshot
│   ├── Persistence/        ApplicationDbContext.cs
│   ├── Repositories/       JobRepository.cs
│   └── Class1.cs           (scaffold stub — unused)
└── HireFlow.API/
    ├── Controllers/        JobsController.cs
    ├── Properties/         launchSettings.json
    ├── Program.cs
    ├── appsettings.json    (contains plain-text connection string — security violation)
    └── appsettings.Development.json
```

**Missing folders/areas:**
- `Domain/Errors/`, Domain behavior methods
- `Application/Common/` (Result, PagedResult, ICurrentUser, IUnitOfWork, validation helper)
- `Application/Services/<Auth|Candidate|Recruiter|Application>/`
- `Application/Validators/<Area>/`
- `Application/Mapping/` (Mapster IRegister profiles)
- `Infrastructure/Persistence/Configurations/` (IEntityTypeConfiguration per entity)
- `Infrastructure/Services/` (JwtTokenService, PasswordHasher)
- `API/Middleware/` (global exception handler)
- `Tests/` (domain unit tests, integration tests)

### 1.5 Existing Entities

| Entity | Fields | Key type |
|---|---|---|
| `Candidate` | `Id (int)`, `Name (string)`, `CvUrl (string)` | int auto-identity |
| `Job` | `Id (int)`, `Title (string)`, `Description (string)`, `IsActive (bool)` | int auto-identity |
| `JobCandidateApplication` | `Id (int)`, `CandidateId`, `JobId`, `JobApplicationStatus (int)`, `AppliedAt`, `StatusUpdatedAt` | int auto-identity |

**No `User`, `Recruiter` entities exist.**

### 1.6 Existing Enums

| Enum | Values | Issues |
|---|---|---|
| `JobApplicationStatus` | Applied, UnderReview, **InterView** (typo), Accepted, Rejected | Missing `Cancelled`; typo `InterView`; wrong name vs plan |

**Missing:** `UserRole`, `JobStatus`, `JobType`.

### 1.7 Existing Migrations

| Migration | Tables | Issues |
|---|---|---|
| `20260916185448_init` | `Candidates`, `Jobs`, `JobCandidateApplications` | Cascade deletes (plan: Restrict); enums as int (plan: string); no lengths |

### 1.8 Existing Endpoints

| Method | Route | Issues |
|---|---|---|
| `POST` | `/api/Jobs` | No auth, no validation, no Result pattern, no ownership |

### 1.9 Existing Tests

**None.** No test project exists.

### 1.10 DI Setup (Program.cs)

Registers: `ApplicationDbContext`, concrete `JobService` (not interface — violation), `IJobRepository`, OpenAPI/Scalar.

**Missing:** Auth middleware, JWT, policies, global exception handler, Mapster, FluentValidation, health checks, rate limiting, CORS.

---

## 2. Gap Analysis Table

| # | Item | Current State | Target (Plan) | Action | Risk |
|---|---|---|---|---|---|
| G-01 | Project/namespace naming | Folders `HireFlow.*`, assemblies `JobApplication.*` | Consistent naming | **DECIDE** (Q-01) | LOW |
| G-02 | `User` entity | Missing | Required with FullName, Email, PasswordHash, Role, CreatedAt | **ADD** | HIGH |
| G-03 | `Recruiter` profile entity | Missing | 1:1 with User; CompanyName required | **ADD** | HIGH |
| G-04 | `Candidate` entity | `Name`, `CvUrl` only; no `UserId` FK | `UserId` FK, `Phone?`, `CvUrl?` (nullable) | **REFACTOR** + data migration | HIGH |
| G-05 | `Job` entity | `IsActive (bool)`, no `RecruiterId`, no `Status`, no `JobType`, no timestamps, no `RowVersion` | Full plan spec | **REFACTOR** | HIGH |
| G-06 | `Application` entity | Minimal; no `CvUrl` snapshot, no `CancelledAt`, no `UpdatedAt`, no `RowVersion` | Full plan spec + rename | **REFACTOR** | HIGH |
| G-07 | `UserRole` enum | Missing | `Candidate=1, Recruiter=2` | **ADD** | HIGH |
| G-08 | `JobStatus` enum | Missing | `Open=1, Closed=2` | **ADD** | HIGH |
| G-09 | `JobType` enum | Missing | `FullTime…Remote` | **ADD** | HIGH |
| G-10 | `ApplicationStatus` enum | Partial (`JobApplicationStatus`), missing `Cancelled`, typo `InterView` | `ApplicationStatus` with 6 values stored as string | **REFACTOR** | MEDIUM |
| G-11 | Domain behavior methods | None | `Job.Create/Update/Close/Reopen`, `Application.Create/Cancel/ChangeStatusByRecruiter` | **ADD** | HIGH |
| G-12 | `Result` / `Result<T>` / `Error` / `ErrorType` | Missing | Required in `Application/Common` | **ADD** | HIGH |
| G-13 | `ValidationError` | Missing | Carries field-error dictionary | **ADD** | HIGH |
| G-14 | `PagedResult<T>` / `PaginationParams` | Missing | Required; clamp pageSize 1..50 | **ADD** | MEDIUM |
| G-15 | `ICurrentUser` | Missing | JWT claim extraction | **ADD** | HIGH |
| G-16 | `IUnitOfWork` | Missing (`SaveChangesAsync` on `IJobRepository`) | Separate unit-of-work abstraction | **ADD** | MEDIUM |
| G-17 | `IAuthService` | Missing | `RegisterAsync`, `LoginAsync`, `GetCurrentUserAsync` | **ADD** | HIGH |
| G-18 | `ICandidateService` | Missing | `GetProfileAsync`, `UpdateProfileAsync` | **ADD** | HIGH |
| G-19 | `IRecruiterService` | Missing | `GetProfileAsync`, `UpdateProfileAsync` | **ADD** | HIGH |
| G-20 | `IJobService` | Missing (only concrete `JobService` without interface) | Interface + full method catalogue | **ADD** | HIGH |
| G-21 | `IApplicationService` | Missing | Full method catalogue | **ADD** | HIGH |
| G-22 | `JobService` | Returns `int`; no Result; no validation; no interface; manual mapping | Interface, Result, FluentValidation, Mapster | **REFACTOR** | HIGH |
| G-23 | `IJobRepository` | `SaveChangesAsync` lives here | Remove `SaveChangesAsync`; add ownership-filtered queries | **REFACTOR** | MEDIUM |
| G-24 | `JobRepository` | Minimal; `SaveChangesAsync` inside | Refactor; ownership-filtered queries | **REFACTOR** | MEDIUM |
| G-25 | `ApplicationDbContext` | No configurations; no `ModelBuilder` overrides | `IEntityTypeConfiguration<T>` per entity | **REFACTOR** | HIGH |
| G-26 | `JobsController` | Depends on concrete `JobService`; no auth; returns raw `int` | Interface dependency; `[Authorize]`; `Result → ProblemDetails` | **REFACTOR** | HIGH |
| G-27 | `AuthController` | Missing | Register, Login, Me | **ADD** | HIGH |
| G-28 | `CandidatesController` | Missing | Profile GET/PUT, `me/applications` | **ADD** | HIGH |
| G-29 | `RecruitersController` | Missing | Profile GET/PUT, `me/jobs` | **ADD** | HIGH |
| G-30 | `ApplicationsController` | Missing | Apply, cancel, status-change, detail | **ADD** | HIGH |
| G-31 | Mapster | Not installed | `IRegister` profiles per area; `ProjectToType<>` on reads | **ADD** | HIGH |
| G-32 | FluentValidation | Not installed | Validators per request DTO | **ADD** | HIGH |
| G-33 | JWT auth | Not installed | `JwtBearer` + `CandidateOnly`/`RecruiterOnly` policies | **ADD** | HIGH |
| G-34 | `IPasswordHasher` | Missing | Interface + implementation | **ADD** | HIGH |
| G-35 | `IJwtTokenService` | Missing | Interface + implementation | **ADD** | HIGH |
| G-36 | Global exception handler | Missing | `IExceptionHandler` → `ProblemDetails` | **ADD** | HIGH |
| G-37 | Entity configurations | Missing | `IEntityTypeConfiguration<T>` per entity | **ADD** | HIGH |
| G-38 | Filtered unique index on Applications | Missing | `(CandidateId, JobId) WHERE Status <> 'Cancelled'` | **ADD** | HIGH |
| G-39 | `RowVersion` on Job and Application | Missing | Optimistic concurrency | **ADD** | MEDIUM |
| G-40 | Rate limiting | Missing | `AddRateLimiter` on `/api/auth/*` | **ADD** | MEDIUM |
| G-41 | CORS | Missing | Explicit policy | **ADD** | MEDIUM |
| G-42 | Health check | Missing | `/health` endpoint | **ADD** | LOW |
| G-43 | Swagger / OpenAPI | Partial (Scalar only, Dev only) | Full Swagger with JWT bearer, `ProducesResponseType`, XML descriptions | **IMPROVE** | LOW |
| G-44 | `.http` file | Exists (weatherforecast stub only) | Full flow HTTP requests | **REPLACE** | LOW |
| G-45 | README | Missing | Architecture, setup, secrets, migrations, endpoints, BRs, tests | **ADD** | LOW |
| G-46 | Test projects | Missing | Domain unit tests + integration tests (S1–S7) | **ADD** | HIGH |
| G-47 | Secrets management | Connection string in `appsettings.json` | User Secrets (dev) / env vars (prod) | **FIX** | HIGH |
| G-48 | `Class1.cs` stubs | Present in all three class-library projects | Remove (dead code) | **REMOVE** | LOW |
| G-49 | `[ForeignKey]` data annotation in Domain | `JobCandidateApplication` uses `System.ComponentModel.DataAnnotations.Schema` | Move FK config to Infrastructure EF config | **REFACTOR** | MEDIUM |
| G-50 | Enum storage | Currently int | Must be string (`HasConversion<string>()`) | **CHANGE** | HIGH |
| G-51 | Cascade delete behavior | Currently `Cascade` on both FKs | Must be `Restrict` everywhere | **CHANGE** | HIGH |

---

## 3. Architecture Violations Found

### 3.1 Layer Leaks

| Violation | Location | Severity |
|---|---|---|
| `System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute` used in Domain entity | `JobCandidateApplication.cs` | MEDIUM — Domain must have zero framework dependencies |
| `JobsController` depends on concrete `JobService`, not an interface | `JobsController.cs` | HIGH — violates DIP |
| Connection string stored in `appsettings.json` (committed to source) | `appsettings.json` | HIGH — security violation |

### 3.2 Business Logic in Wrong Layer

| Violation | Location | Severity |
|---|---|---|
| Entity construction (`new Job { IsActive = true }`) inside `JobService` — manual mapping, not a domain factory | `JobService.cs` L21-26 | HIGH |
| `SaveChangesAsync` on `IJobRepository` — UoW responsibility bleed | `IJobRepository.cs` | MEDIUM |
| No validation at all in `JobService.CreateAsync` | `JobService.cs` | HIGH |

### 3.3 CQRS / Mediator Code

**None found.** No MediatR, no command/query handlers. The existing `JobService` is a plain class — aligned with the plan's no-CQRS requirement. No conversion needed.

### 3.4 Exceptions as Control Flow

Not applicable — no business logic exceptions found. The startup guard in `Program.cs` (`throw new InvalidOperationException` for missing connection string) is acceptable.

### 3.5 Manual Mapping

`JobService.CreateAsync` manually sets entity fields from DTO. Must be replaced by a domain factory method.

### 3.6 Missing Validation, Authorization, Ownership

All absent. `CreateAsync` does no input validation. No `[Authorize]` on any endpoint. No concept of resource ownership.

---

## 4. Concrete Refactor List

### Phase 1 — Foundation

1. Remove `Class1.cs` stubs from all three class-library projects.
2. Add `Application/Common/`: `Result.cs`, `Result{T}.cs`, `Error.cs`, `ErrorType.cs`, `ValidationError.cs`.
3. Add `Application/Common/PagedResult.cs`, `PaginationParams.cs`.
4. Add `Application/Common/ICurrentUser.cs`, `IUnitOfWork.cs`.
5. Add `Application/Common/ValidationHelper.cs` (FluentValidation → Result conversion).
6. Add all five service interfaces (stubs).
7. Install `Mapster` + `Mapster.DependencyInjection` in Application.
8. Install `FluentValidation` in Application.
9. Add Mapster DI setup in API.
10. Add `API/Extensions/ResultExtensions.cs` (`ToActionResult()` helper).
11. Add `API/Middleware/GlobalExceptionHandler.cs` implementing `IExceptionHandler`.
12. Update `Program.cs`: add exception handler, auth middleware stubs.

### Phase 2 — Domain Model

1. Refactor `Candidate.cs`: remove `Name`, add `UserId (int)`, `Phone (string?)`, make `CvUrl` nullable.
2. Remove `DataAnnotations.Schema` import from `JobCandidateApplication.cs`.
3. Add `User.cs`, `Recruiter.cs` entities.
4. Refactor `Job.cs`: add `RecruiterId`, `Location?`, `JobType`, `Status`, `CreatedAt`, `UpdatedAt?`, `ClosedAt?`, `RowVersion`. Remove `IsActive`.
5. Rename `JobCandidateApplication` → target name (see Q-09). Add `CvUrl`, `CoverLetter?`, `CancelledAt?`, `UpdatedAt?`, `RowVersion`. Change status to `ApplicationStatus`.
6. Add enums: `UserRole`, `JobStatus`, `JobType`. Refactor `JobApplicationStatus` → `ApplicationStatus`, add `Cancelled`, fix `InterView` → `Interview`.
7. Add domain behavior methods on `Job` and `Application` all returning `Result`.
8. Add `Domain/Errors/`: `AuthErrors.cs`, `JobErrors.cs`, `ApplicationErrors.cs`, `CandidateErrors.cs`, `RecruiterErrors.cs`, `ConcurrencyErrors.cs`.

### Phase 3 — Persistence

1. Add `Infrastructure/Persistence/Configurations/` with one `IEntityTypeConfiguration<T>` per entity.
2. Configure lengths, required/optional, enum-as-string, `RowVersion`, `DeleteBehavior.Restrict`.
3. Add all indexes from plan 8.2 including filtered unique index on Applications.
4. Refactor `ApplicationDbContext.cs`.
5. Refactor `IJobRepository` + `JobRepository`: remove `SaveChangesAsync`, add ownership-filtered queries.
6. Add repository interfaces and implementations for User, Candidate, Recruiter, Application.
7. Add `UnitOfWork.cs` implementing `IUnitOfWork`.
8. Create new migration (`AddFullSchema` or `InitialSchema` if reset approved).

### Phase 4 — Authentication

1. Install `Microsoft.AspNetCore.Authentication.JwtBearer` in API.
2. Add `Infrastructure/Services/PasswordHasher.cs`.
3. Add `Infrastructure/Services/JwtTokenService.cs`.
4. Implement `AuthService`.
5. Add `AuthController`.
6. Add `API/CurrentUser.cs`.
7. Add `CandidateOnly` and `RecruiterOnly` auth policies.
8. Move JWT secret + connection string to User Secrets.
9. Add `RegisterRequestValidator`, `LoginRequestValidator`.

### Test Projects (scaffold in Phase 1)

1. `HireFlow.Domain.Tests` (xUnit) — domain unit tests.
2. `HireFlow.Application.Tests` (xUnit) — service unit tests with mocked repositories.
3. `HireFlow.Integration.Tests` (xUnit + `WebApplicationFactory`) — S1–S7 scenarios.

---

## 5. Database / Data Migration Approach

### 5.1 Existing Schema (from `init` migration)

```
Candidates:               Id(int PK), Name(nvarchar MAX NOT NULL), CvUrl(nvarchar MAX NOT NULL)
Jobs:                     Id(int PK), Title(nvarchar MAX), Description(nvarchar MAX), IsActive(bit)
JobCandidateApplications: Id(int PK), CandidateId(int FK Cascade), JobId(int FK Cascade),
                          JobApplicationStatus(int), AppliedAt(datetime2), StatusUpdatedAt(datetime2)
```

### 5.2 Delta Required (if incremental migration chosen)

| Table | Change |
|---|---|
| `Users` | New table |
| `Candidates` | Add `UserId`, `Phone`. Backfill Users from `Name`. Drop `Name`. Make `CvUrl` nullable. |
| `Recruiters` | New table |
| `Jobs` | Add `RecruiterId`, `Location`, `JobType`, `Status`, `CreatedAt`, `UpdatedAt`, `ClosedAt`, `RowVersion`. Backfill `Status` from `IsActive`. Drop `IsActive`. |
| `JobCandidateApplications` | Rename to `Applications`. Add `CvUrl`, `CoverLetter`, `CancelledAt`, `UpdatedAt`, `RowVersion`. Rename `JobApplicationStatus` → `Status` and change from int to string. Change FK delete behavior Cascade → Restrict. Add filtered unique index. |
| New indexes | All from plan 8.2 |

### 5.3 Recommendation

> [!IMPORTANT]
> **Key decision needed (Q-02):** The existing `init` migration covers a local dev database (`JobApplication`). The schema changes are extremely extensive — adding `User`, `Recruiter`, renaming tables, changing enum storage from int to string (requires column-level data migration), changing FK behavior. If there is **no production data or data worth preserving**, a database reset is strongly recommended: drop the existing migration(s) and create a single clean `InitialSchema` migration. This is far safer and less error-prone than incremental SQL on a dev scaffold.

---

## 6. Questions Requiring Your Confirmation

### Q-01 — Naming: `HireFlow.*` vs `JobApplication.*`

Folder names are `HireFlow.*`; project files, namespaces, and solution use `JobApplication.*`.

- **Option A (Recommended):** Rename project files, namespaces and solution to `HireFlow.*` during Phase 1.
- **Option B:** Keep `JobApplication.*` everywhere and accept the mismatch.
- **Option C:** Keep `JobApplication.*` assemblies but rename the solution file to `HireFlow.slnx`.

**My default if not answered:** Option A.

---

### Q-02 — Database Reset vs. Incremental Migration

- **Option A (Recommended):** Reset — drop `init` migration, create a single clean `InitialSchema`. No data to preserve in a dev scaffold.
- **Option B:** Keep `init` and layer a new migration on top with full data-migration SQL.

**If Option B:** please tell me if there is any data in the `JobApplication` database that must be preserved.

---

### Q-03 — Primary Key Type: `int` vs `Guid`

Existing entities use `int`. Plan says "keep existing key type." I will use `int` throughout. Confirm or override.

---

### Q-04 — `IPasswordHasher` Implementation

- **Option A (Recommended):** `Microsoft.AspNetCore.Identity.PasswordHasher<TUser>` — built-in, PBKDF2, no new package.
- **Option B:** `BCrypt.Net-Next` — extra package.
- **Option C:** Custom PBKDF2 via `Rfc2898DeriveBytes`.

---

### Q-05 — Swagger Library

- **Option A (Recommended):** Keep `Microsoft.AspNetCore.OpenApi` + `Scalar.AspNetCore` with JWT bearer configured via native OpenAPI transformer.
- **Option B:** Replace with `Swashbuckle.AspNetCore` for traditional Swagger UI.

---

### Q-06 — `TimeProvider`

`TimeProvider` is built into .NET 8+. Since the project targets net10.0, I will use it directly without a custom `IDateTimeProvider` wrapper. Confirm.

---

### Q-07 — Connection String in `appsettings.json`

During Phase 4, I will move the connection string to **User Secrets** for dev and document the format in the README. `appsettings.json` will contain a placeholder only. Confirm this is acceptable.

---

### Q-08 — Plan Section 4.5 Assumptions

Please confirm all five assumptions from section 4.5 are correct:

| # | Assumption |
|---|---|
| A1 | Jobs editable only while Open (BR-J3). |
| A2 | Not-owned resources return 404, not 403. |
| A3 | Rejected/Accepted block re-application (BR-P4). |
| A4 | `Applied → Interview` directly is not allowed. |
| A5 | Public job details visible for Closed jobs. |

---

### Q-09 — Entity Name for Application (name collision risk)

Renaming `JobCandidateApplication` to `Application` creates a class named `Application` that collides with the `*.Application` namespace segment.

- **Option A (Recommended):** Name the entity `JobApplication` — descriptive, no collision.
- **Option B:** Name it `Application` and use fully-qualified names where needed.
- **Option C:** Name it `CandidateApplication`.

**My preference:** Option A — `JobApplication`.

---

### Q-10 — Integration Test Database

- **Option A:** LocalDB — no extra dependencies; requires LocalDB installed locally.
- **Option B:** `Testcontainers.MsSql` — Docker-based; fully isolated; requires Docker.
- **Option C:** `InMemory` — no SQL Server required, but cannot test the filtered unique index (BR-P2/P3).

**Recommendation:** Option A or B. Option C is insufficient for full business-rule coverage.

---

## 7. Violations Summary

| Category | Count |
|---|---|
| Framework dependency in Domain (`DataAnnotations.Schema`) | 1 |
| Controller depends on concrete class (not interface) | 1 |
| Service without interface | 1 |
| No Result pattern anywhere | ALL |
| No FluentValidation | ALL |
| No Mapster | ALL |
| No auth / JWT / policies | ALL |
| No ownership checks | ALL |
| Secrets committed to source | 1 |
| Missing entities (`User`, `Recruiter`) | 2 |
| Missing enums (`UserRole`, `JobStatus`, `JobType`) | 3 |
| Enum value typo (`InterView`) | 1 |
| Enum storage wrong type (int → string required) | 1 |
| FK delete behavior wrong (Cascade → Restrict required) | 2 |
| Missing test projects | 3 |
| CQRS / Mediator code | **0 (clean)** |

---

> **STATE: `WAITING_FOR_APPROVAL`**
>
> Audit complete. No code changed. Please answer the questions above — especially **Q-01**, **Q-02**, **Q-09**, **Q-10** — then send **`ابدأ`** / **`START EXECUTION`** / **`CONFIRMED – PROCEED`** to begin Phase 1.
