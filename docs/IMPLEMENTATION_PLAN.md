# Recruitment API — Implementation Plan

> Place this file at `docs/IMPLEMENTATION_PLAN.md` in the repository.
> It is the **single source of truth** for scope, business rules, design and execution order.
> If the code and this document disagree, stop and ask before choosing.

---

## 1. System Summary

A Recruitment API with two kinds of users:

- **Recruiter** — creates and manages job postings, closes/reopens them, reviews applications and moves them through the hiring pipeline.
- **Candidate** — browses open jobs, applies, tracks applications, cancels an application.

Both roles **register and log in** (JWT).

---

## 2. Architecture & Technical Constraints

### 2.1 Layers (dependency direction is strict)

```
API  ──►  Application  ──►  Domain
 │             ▲
 └──► Infrastructure ──┘   (Infrastructure implements Application abstractions)
```

| Project | Responsibility | May reference |
|---|---|---|
| `*.Domain` | Entities, enums, domain rules (returning `Result`), domain errors | nothing |
| `*.Application` | Application services (one per feature area), DTOs, validators, mapping config, abstractions (repositories, `ICurrentUser`, `IJwtTokenService`, `IPasswordHasher`, `IUnitOfWork`), `Result`/`Error` types | Domain |
| `*.Infrastructure` | EF Core `DbContext`, configurations, migrations, repositories, JWT generation, password hashing | Application, Domain |
| `*.API` | Controllers, middleware, DI composition root, Swagger, auth setup | Application, Infrastructure (composition root only) |

Rules:
- Domain has **no** dependency on EF Core, Mapster, ASP.NET, or any framework package.
- Controllers contain **no** business logic and **no** direct `DbContext` access.
- Keep the project's **existing target framework** and existing package versions unless an upgrade is required and approved.

### 2.2 Mandatory patterns

1. **Application Services — NO CQRS.** Do **not** use CQRS, MediatR, or any mediator / command / query / handler abstraction.
   - Each feature area has one service **interface + implementation** in `Application/Services/<Area>/`: `IAuthService`, `ICandidateService`, `IRecruiterService`, `IJobService`, `IApplicationService` (method catalogue in 7.5). One method per use case (e.g., `JobService.CloseAsync(id)`), each returning `Result` / `Result<T>`.
   - Controllers depend on the service **interfaces only**.
   - DTOs live in `Application/DTOs/<Area>/`, validators in `Application/Validators/<Area>/`.
   - If a service grows too large, split it by sub-area (e.g., public jobs vs. recruiter job management), **not** by command/query.
   - If the existing code already contains CQRS/mediator code, the Phase 0 audit must list it and propose converting it to services. Do not leave two styles side by side.
   - **Do not add a library with commercial licensing without asking.**
2. **Result pattern** — no exceptions for expected/business failures.
   - `Result` and `Result<T>` with `IsSuccess`, `IsFailure`, `Error`.
   - `Error(string Code, string Description, ErrorType Type)`; `ErrorType`: `Validation, NotFound, Conflict, Unauthorized, Forbidden, Failure`.
   - `ValidationError` carrying a list/dictionary of field errors.
   - Domain methods that can fail **return `Result`** (not throw).
   - Exceptions are only for truly unexpected failures and are handled by a global `IExceptionHandler`.
3. **Mapster** for mapping.
   - Mapping profiles as `IRegister` classes in the Application layer; register with `TypeAdapterConfig.GlobalSettings.Scan(assembly)` and a singleton `IMapper` (`ServiceMapper`).
   - Direction: **Entity → DTO only.** Never map request DTOs onto entities; construct entities through domain factory methods so invariants hold.
   - For read queries use `ProjectToType<TDto>()` on `IQueryable` where it keeps SQL efficient.
4. **Validation** — FluentValidation validators per request DTO, invoked explicitly at the start of each service method through a small helper that converts a failed validation result into a `Result` with `ErrorType.Validation` (no exception).
5. **Persistence** — EF Core + SQL Server. Follow the project's existing repository/`DbContext` abstraction style; do not mix two styles.
6. **Auth** — JWT bearer, role-based policies.

### 2.3 Library policy
- Prefer the built-ins and the libraries above.
- For tests: xUnit + plain assertions or Shouldly. **Avoid adding libraries with commercial licensing.** Ask before adding any new package not listed here.

---

## 3. Domain Model

Key type: keep the existing key type; if the project has none, use `Guid`, generated in the domain factory.
All timestamps are **UTC**, obtained through `TimeProvider` (or `IDateTimeProvider` if the framework is older than .NET 8) injected in Application, and passed into domain methods.

### 3.1 Entities

**User**
| Field | Type | Notes |
|---|---|---|
| Id | key | |
| FullName | string(100) | required |
| Email | string(256) | required, stored normalized (trimmed, lower-case), **unique** |
| PasswordHash | string(500) | required |
| Role | `UserRole` | `Candidate`, `Recruiter` |
| CreatedAt | datetime (UTC) | |

**Candidate** (profile, 1:1 with User)
| Field | Type | Notes |
|---|---|---|
| Id | key | |
| UserId | FK → User | unique |
| CvUrl | string(2048)? | nullable, absolute https URL |
| Phone | string(30)? | optional |

**Recruiter** (profile, 1:1 with User)
| Field | Type | Notes |
|---|---|---|
| Id | key | |
| UserId | FK → User | unique |
| CompanyName | string(200) | required |

**Job**
| Field | Type | Notes |
|---|---|---|
| Id | key | |
| RecruiterId | FK → Recruiter | owner |
| Title | string(200) | required |
| Description | string(4000) | required |
| Location | string(200)? | optional |
| JobType | `JobType` | `FullTime, PartTime, Contract, Internship, Remote` |
| Status | `JobStatus` | `Open, Closed` |
| CreatedAt | datetime | |
| UpdatedAt | datetime? | |
| ClosedAt | datetime? | set on close, cleared on reopen |
| RowVersion | byte[] | concurrency token |

**Application**
| Field | Type | Notes |
|---|---|---|
| Id | key | |
| CandidateId | FK → Candidate | |
| JobId | FK → Job | |
| Status | `ApplicationStatus` | see 4.3 |
| AppliedAt | datetime | |
| CancelledAt | datetime? | set on cancel |
| UpdatedAt | datetime? | |
| CvUrl | string(2048) | **snapshot** of the CV used at apply time |
| CoverLetter | string(4000)? | optional |
| RowVersion | byte[] | concurrency token |

**Optional (Phase 9, only if time allows): `ApplicationStatusHistory`** — `Id, ApplicationId, OldStatus, NewStatus, ChangedAt, ChangedByUserId`.

### 3.2 Enums
```
UserRole:          Candidate = 1, Recruiter = 2
JobStatus:         Open = 1, Closed = 2
JobType:           FullTime = 1, PartTime = 2, Contract = 3, Internship = 4, Remote = 5
ApplicationStatus: Applied = 1, UnderReview = 2, Interview = 3, Accepted = 4, Rejected = 5, Cancelled = 6
```
Persist enums **as strings** (`HasConversion<string>()`, `HasMaxLength(30)`) for readable data.

### 3.3 Domain behavior (methods return `Result`)

**Job**
- `Job.Create(recruiterId, title, description, location, jobType, now)` → `Result<Job>` (Status = Open)
- `Update(title, description, location, jobType, now)` → `Result` — allowed **only when Open** (else `Job.NotEditable`)
- `Close(now)` → `Result` — fails with `Job.AlreadyClosed` if already Closed; sets `Status=Closed`, `ClosedAt=now`
- `Reopen(now)` → `Result` — fails with `Job.AlreadyOpen` if Open; sets `Status=Open`, `ClosedAt=null`
- `bool IsOpen`

**Application**
- `Application.Create(candidateId, job, cvUrl, coverLetter, now)` → `Result<Application>`
  - fails `Job.NotOpen` if the job is Closed
  - fails `Application.CvRequired` if `cvUrl` is empty
  - creates with `Status=Applied`, `AppliedAt=now`
  - (duplicate-active-application check is done in `ApplicationService` + guaranteed by the DB filtered unique index)
- `Cancel(now)` → `Result` — allowed only from `Applied` or `UnderReview`; else `Application.CannotCancel`; sets `Status=Cancelled`, `CancelledAt=now`
- `ChangeStatusByRecruiter(target, now)` → `Result`
  - `target` of `Cancelled` or `Applied` → `Application.StatusNotAllowed`
  - otherwise enforce the transition table in 4.3 → else `Application.InvalidTransition`
  - **does not look at the job status** (recruiter may process applications of a closed job)

---

## 4. Business Rules

### 4.1 Identity & access
| ID | Rule |
|---|---|
| BR-A1 | A user registers as **Candidate** or **Recruiter** only. `Admin` can never be created through the API. |
| BR-A2 | Email is unique (case-insensitive, trimmed). |
| BR-A3 | Registration creates the `User` **and** the matching profile (`Candidate` or `Recruiter`) in **one transaction**. Recruiter registration requires `CompanyName`. |
| BR-A4 | Passwords are hashed (never stored/logged in plain text). Policy: min 8 chars, at least one upper, lower, digit. |
| BR-A5 | Login returns a JWT containing: `sub` (UserId), `email`, `role`, and `candidate_id` **or** `recruiter_id`. Invalid credentials always return the same generic error (no user enumeration). |
| BR-A6 | Role authorization is enforced by policies **and** resource ownership is re-checked inside services (never trust the route id alone). |

### 4.2 Jobs
| ID | Rule |
|---|---|
| BR-J1 | Only a Recruiter can create a job; it starts as `Open`. |
| BR-J2 | A Recruiter can update, close, and reopen **only their own** jobs. |
| BR-J3 | A Job can be edited only while `Open`. To edit a closed job, reopen it first. |
| BR-J4 | Closing is **not a delete** — `Status=Closed`, `ClosedAt` set. Closing an already-closed job → `Job.AlreadyClosed`. |
| BR-J5 | **Reopen is allowed** (any time, by the owner): `Status=Open`, `ClosedAt=null`. Reopening an open job → `Job.AlreadyOpen`. |
| BR-J6 | Public listing shows **Open jobs only**. Job details are readable for any existing job and expose `status`. |
| BR-J7 | Closing or reopening a job **never modifies existing applications**. |

### 4.3 Applications
| ID | Rule |
|---|---|
| BR-P1 | Only a Candidate can apply, and only to an **Open** job (`Job.NotOpen` otherwise). |
| BR-P2 | A candidate cannot have **two non-cancelled applications** for the same job (`Application.AlreadyExists`). |
| BR-P3 | **After cancelling, the candidate may apply again** to the same job as long as the job is still Open. A re-application is a **new** `Application` row; the cancelled one is kept as history. |
| BR-P4 | A candidate whose application was `Rejected` or `Accepted` **cannot** re-apply to that job (the application is not cancelled, so BR-P2 applies). |
| BR-P5 | CV resolution: `request.CvUrl` if provided, else the candidate profile's `CvUrl`, else `Application.CvRequired`. The resolved value is stored as a snapshot on the application. |
| BR-P6 | A candidate can cancel **only their own** application, and **only** while status is `Applied` or `UnderReview`. Cancel sets `CancelledAt`. |
| BR-P7 | A **Recruiter can never set `Cancelled`** (nor `Applied`). Cancel is a candidate-only action. |
| BR-P8 | A Recruiter can change the status of applications **only for their own jobs**, and **also when the job is Closed** (closing blocks new applications only). |
| BR-P9 | Status changes must follow the transition table below. |
| BR-P10 | Terminal statuses: `Accepted`, `Rejected`, `Cancelled` — no further changes. |
| BR-P11 | An application is visible to: the candidate who owns it, and the recruiter who owns the job. Everyone else gets `NotFound`. |

**Transition table** (✔ = allowed)

| From ↓ / To → | UnderReview | Interview | Accepted | Rejected | Cancelled |
|---|:-:|:-:|:-:|:-:|:-:|
| **Applied** | ✔ (R) | ✘ | ✘ | ✔ (R) | ✔ (C) |
| **UnderReview** | — | ✔ (R) | ✘ | ✔ (R) | ✔ (C) |
| **Interview** | ✘ | — | ✔ (R) | ✔ (R) | ✘ |
| **Accepted / Rejected / Cancelled** | terminal — everything ✘ | | | | |

(R) = Recruiter via `PATCH status` · (C) = Candidate via `cancel`

### 4.4 Ownership response policy
- Wrong **role** → `403` (handled by authorization policies).
- Resource that does not exist **or is not owned by the caller** → `404` (avoids leaking existence). Use one consistent approach across all endpoints.

### 4.5 Assumptions (change here first if you disagree)
1. Jobs are editable only while Open (BR-J3).
2. Not-owned resources return 404, not 403 (4.4).
3. `Rejected`/`Accepted` applications block re-application (BR-P4).
4. `Applied → Interview` directly is not allowed; it must pass through `UnderReview`.
5. Public job details are visible for Closed jobs too (BR-J6).

---

## 5. Error Catalogue

| Code | Type | HTTP | When |
|---|---|---|---|
| `Validation.Failed` | Validation | 400 | FluentValidation failure (with field errors) |
| `Auth.EmailAlreadyExists` | Conflict | 409 | Register with used email |
| `Auth.InvalidCredentials` | Unauthorized | 401 | Wrong email/password |
| `Auth.InvalidRole` | Validation | 400 | Register with role other than Candidate/Recruiter |
| `Candidate.NotFound` / `Recruiter.NotFound` | NotFound | 404 | Profile missing for current user |
| `Job.NotFound` | NotFound | 404 | Missing or not owned (management endpoints) |
| `Job.NotOpen` | Conflict | 409 | Apply to closed job |
| `Job.AlreadyClosed` | Conflict | 409 | Close a closed job |
| `Job.AlreadyOpen` | Conflict | 409 | Reopen an open job |
| `Job.NotEditable` | Conflict | 409 | Update a closed job |
| `Application.NotFound` | NotFound | 404 | Missing or not visible to caller |
| `Application.AlreadyExists` | Conflict | 409 | Active application already exists |
| `Application.CvRequired` | Validation | 400 | No CV on request nor profile |
| `Application.CannotCancel` | Conflict | 409 | Status is not Applied/UnderReview |
| `Application.StatusNotAllowed` | Validation | 400 | Recruiter sends `Cancelled`/`Applied` |
| `Application.InvalidTransition` | Conflict | 409 | Transition not in the table |
| `Concurrency.Conflict` | Conflict | 409 | `DbUpdateConcurrencyException` |
| `Unexpected` | Failure | 500 | Unhandled exception (via global handler) |

Errors are defined as static members in `Domain/Errors/*Errors.cs` (or Application, following existing convention). API maps `ErrorType` → HTTP status and returns **`ProblemDetails`** (RFC 7807) with `code` in `extensions`; validation errors include an `errors` dictionary.

---

## 6. API Contract

Base route: `/api`. All timestamps ISO-8601 UTC. Lists are paged: `page` (default 1), `pageSize` (default 10, **max 50**), response `PagedResult<T> { items, page, pageSize, totalCount, totalPages }`.

### 6.1 Auth
| Method & Route | Access | Notes |
|---|---|---|
| `POST /api/auth/register` | Public | Body: `fullName, email, password, role (Candidate\|Recruiter), companyName (required for Recruiter)`. `201`. |
| `POST /api/auth/login` | Public | Body: `email, password`. `200` → `{ accessToken, expiresAtUtc, user{...} }`. |
| `GET  /api/auth/me` | Authenticated | Current user + role + profile id. |
| `POST /api/auth/refresh` | Public | **Optional** (Phase 9). Refresh-token rotation. |

### 6.2 Profiles
| Method & Route | Access | Notes |
|---|---|---|
| `GET /api/candidates/me` | Candidate | |
| `PUT /api/candidates/me` | Candidate | `fullName, phone, cvUrl` (updates User.FullName + Candidate fields). |
| `GET /api/recruiters/me` | Recruiter | |
| `PUT /api/recruiters/me` | Recruiter | `fullName, companyName`. |

### 6.3 Jobs
| Method & Route | Access | Notes |
|---|---|---|
| `GET /api/jobs` | Public | **Open only**. Query: `search` (title/description), `jobType`, `location`, `page`, `pageSize`. Newest first. |
| `GET /api/jobs/{id}` | Public | Any existing job; includes `status`, company name. |
| `POST /api/jobs` | Recruiter | `201` + `Location` header. |
| `PUT /api/jobs/{id}` | Recruiter (owner) | Only while Open (BR-J3). |
| `POST /api/jobs/{id}/close` | Recruiter (owner) | `204`. BR-J4. |
| `POST /api/jobs/{id}/reopen` | Recruiter (owner) | `204`. BR-J5. |
| `GET /api/recruiters/me/jobs` | Recruiter | Query: `status`, `page`, `pageSize`. Includes `applicationsCount`. |

### 6.4 Applications
| Method & Route | Access | Notes |
|---|---|---|
| `POST /api/jobs/{jobId}/applications` | Candidate | Body (all optional): `cvUrl, coverLetter`. `201`. BR-P1..P5. |
| `GET /api/candidates/me/applications` | Candidate | Query: `status`, `page`, `pageSize`. Includes job title/company/status. |
| `GET /api/applications/{id}` | Candidate (owner) or Recruiter (job owner) | BR-P11. |
| `POST /api/applications/{id}/cancel` | Candidate (owner) | `204`. BR-P6. |
| `GET /api/jobs/{jobId}/applications` | Recruiter (job owner) | Query: `status`, `page`, `pageSize`. Works for Closed jobs too. Returns candidate name/email, cvUrl snapshot, coverLetter, status, appliedAt. |
| `PATCH /api/applications/{id}/status` | Recruiter (job owner) | Body: `{ "status": "UnderReview" \| "Interview" \| "Accepted" \| "Rejected" }`. BR-P7..P10. |

---

## 7. Application-Layer Design Details

### 7.1 Building blocks (`Application/Common`)
- `Result`, `Result<T>`, `Error`, `ErrorType`, `ValidationError`
- `PagedResult<T>`, `PaginationParams` (clamps `pageSize` to 1..50)
- Service interfaces + implementations (2.2), registered in DI (assembly scanning or explicit `AddScoped`)
- Validation helper (`ValidationResult` → `Result`)
- `ICurrentUser { UserId; Role; CandidateId?; RecruiterId? }` — implemented in API from JWT claims via `IHttpContextAccessor`
- `IUnitOfWork.SaveChangesAsync`
- Repository interfaces (or `IApplicationDbContext`) per existing style
- `IPasswordHasher`, `IJwtTokenService`
- `TimeProvider`

### 7.2 Service method conventions
1. Resolve identity from `ICurrentUser` (never from request body).
2. Load aggregate **with ownership filter in the query** (e.g., `job.Id == id && job.RecruiterId == currentRecruiterId`) → `NotFound` if null.
3. Call the domain method; if `Result` fails → return it.
4. Persist through `IUnitOfWork`; catch `DbUpdateConcurrencyException` → `Concurrency.Conflict`; catch unique-index violation on the filtered index → `Application.AlreadyExists`.
5. Map with Mapster, return `Result<TDto>`.

### 7.3 Apply-to-job flow (`ApplicationService.ApplyAsync` — reference implementation of the rules)
1. Validate request (`cvUrl` absolute https ≤ 2048, `coverLetter` ≤ 4000).
2. Load job by id → `Job.NotFound` if missing.
3. Load candidate by `ICurrentUser.CandidateId`.
4. If an application exists for `(candidate, job)` with `Status != Cancelled` → `Application.AlreadyExists`.
5. Resolve CV (BR-P5).
6. `Application.Create(...)` (fails with `Job.NotOpen` if closed).
7. Save. If the DB filtered unique index throws → map to `Application.AlreadyExists` (covers the race between two simultaneous requests).

### 7.4 Mapping profiles (Mapster)
One `IRegister` per feature area. Examples: `Job → JobDto / JobListItemDto`, `Application → ApplicationDto / CandidateApplicationItemDto / RecruiterApplicationItemDto`, `User+Candidate → CandidateProfileDto`. Flatten related data with explicit `.Map(dest => ..., src => ...)`; **no** business logic inside mapping.

### 7.5 Service catalogue (no CQRS)
| Service | Methods (all `async`, all return `Result` / `Result<T>` / `Result<PagedResult<T>>`) |
|---|---|
| `IAuthService` | `RegisterAsync`, `LoginAsync`, `GetCurrentUserAsync` |
| `ICandidateService` | `GetProfileAsync`, `UpdateProfileAsync` |
| `IRecruiterService` | `GetProfileAsync`, `UpdateProfileAsync` |
| `IJobService` | `GetOpenJobsAsync(filter)`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `CloseAsync`, `ReopenAsync`, `GetMyJobsAsync(filter)` |
| `IApplicationService` | `ApplyAsync`, `GetMyApplicationsAsync(filter)`, `GetByIdAsync`, `CancelAsync`, `GetJobApplicationsAsync(jobId, filter)`, `ChangeStatusAsync` |

---

## 8. Persistence Design

### 8.1 Configuration
- One `IEntityTypeConfiguration<T>` per entity in `Infrastructure/Persistence/Configurations`.
- Lengths/required exactly as in section 3. Enums stored as string.
- `RowVersion` on `Job` and `Application` (`IsRowVersion()`).
- **Delete behavior: `Restrict` everywhere** (no cascade deletes; nothing is hard-deleted by the API).

### 8.2 Indexes
| Table | Index | Purpose |
|---|---|---|
| Users | unique `(Email)` | BR-A2 |
| Candidates | unique `(UserId)` | 1:1 |
| Recruiters | unique `(UserId)` | 1:1 |
| Jobs | `(Status, CreatedAt DESC)` | public listing |
| Jobs | `(RecruiterId, Status)` | "my jobs" |
| Applications | **filtered unique** `(CandidateId, JobId)` **WHERE `Status <> 'Cancelled'`** | BR-P2/P3 — allows re-apply after cancel, blocks duplicates. Write the filter to match the chosen enum storage (string vs int). |
| Applications | `(JobId, Status)` | recruiter list |
| Applications | `(CandidateId, AppliedAt DESC)` | candidate list |

### 8.3 Migration strategy
- Add **new** migrations; do **not** edit or delete migrations that may already be applied anywhere.
- Existing `Candidate.Name/Email` → move to `User` (create `User` rows, link `Candidate.UserId`), then drop the columns. Existing `Job.IsActive` → `Status` (`true→Open`, `false→Closed`). Existing `Application` rows: add `CvUrl` (backfill from Candidate.CvUrl), `CancelledAt`, `UpdatedAt`, `RowVersion`.
- If the existing database only contains disposable development data, a reset is acceptable **but must be announced in the Phase 0 report and confirmed** before doing it.

### 8.4 Query guidelines
- Read queries: `AsNoTracking()` + projection (`ProjectToType`).
- Always paginate list endpoints; always order deterministically.
- Avoid N+1; use projection instead of `Include` chains for DTOs.

---

## 9. Security & Cross-Cutting

- JWT validation: issuer, audience, lifetime, signing key; small `ClockSkew`. Secrets via User Secrets/environment variables — **never committed**.
- Authorization policies: `CandidateOnly`, `RecruiterOnly`, applied per controller/action; default fallback policy = authenticated, with `[AllowAnonymous]` explicitly on public endpoints.
- Rate limiting on `/api/auth/*` (built-in `AddRateLimiter`).
- CORS configured explicitly (no wildcard in production config).
- Global `IExceptionHandler` → `ProblemDetails`; no stack traces outside Development.
- Structured logging (built-in `ILogger`, or Serilog if already used). Never log passwords/tokens/PII beyond ids.
- Health check endpoint `/health`.
- Swagger/OpenAPI with JWT bearer scheme, XML/summary descriptions, response types (`ProducesResponseType`) for each endpoint.

---

## 10. Phased Execution Plan

> After **every phase**: `dotnet build` clean (no new warnings), `dotnet test` green, commit on the feature branch with a clear message, and write a short phase summary (what changed, decisions, open questions).

### Phase 0 — Audit & Alignment (**NO code changes; STOP for approval**)
Tasks
1. Map the solution: projects, references, target framework, packages + versions, folder structure, naming conventions, DI setup, existing entities/migrations, existing endpoints, existing tests.
2. Compare against sections 2–9 of this plan. Produce a **gap analysis table**: item · current state · target · action (keep / refactor / add / remove) · risk.
3. Detect and list violations: layer leaks (e.g., EF in Domain, logic in controllers), **any CQRS/mediator/handler code (to be converted to application services)**, exceptions used for flow control, manual mapping, missing validation, missing ownership checks.
4. Propose the concrete refactor list and migration/data plan (section 8.3).
5. List questions/assumptions needing my confirmation (include section 4.5).

Deliverable: `docs/AUDIT_REPORT.md`.
Acceptance: report complete; **wait for my approval**.

### Phase 1 — Foundation (building blocks)
Tasks: solution/project structure fixes per audit; `Result`/`Error`/`ErrorType`/`ValidationError`; service abstractions + DI registration + validation helper (and removal/conversion of any existing CQRS/mediator code, per the audit); Mapster setup + scanning; `PagedResult`/`PaginationParams`; `ICurrentUser`; `TimeProvider`; `IUnitOfWork`; API `Result → ProblemDetails` mapping (`ToActionResult()` extension); global `IExceptionHandler`; Swagger with JWT.
Acceptance: solution builds; a sample endpoint flows Controller → Service → `Result` → `ProblemDetails`/200 correctly; unit tests for `Result` and pagination clamping.

### Phase 2 — Domain Model
Tasks: entities, enums, factories and behaviors from 3.1–3.3 (all failing paths return `Result` with catalogue errors); remove framework dependencies from Domain.
Acceptance: **domain unit tests** cover every cell of the transition table, `Cancel`, `Close`, `Reopen`, `Update`, `Create` (open/closed job, missing CV). All BR-P6..P10, BR-J3..J5 covered.

### Phase 3 — Persistence
Tasks: `DbContext`, entity configurations, indexes (8.2 incl. filtered unique), `RowVersion`, repositories/UoW, migration(s) + data migration per 8.3, optional dev seed data.
Acceptance: migration applies on a clean DB and on the existing DB; index definitions verified in SQL; repository tests (or integration tests) confirm the filtered unique index allows re-apply after cancel and blocks duplicates.

### Phase 4 — Authentication
Tasks: `IPasswordHasher` + implementation; `IJwtTokenService` + implementation (claims per BR-A5); Register (user + profile in one transaction), Login, `me`; policies `CandidateOnly`/`RecruiterOnly`; validators (password policy, role restriction).
Acceptance: all BR-A1..A6 covered by tests; token contains expected claims; Admin registration rejected; duplicate email → 409; generic invalid-credentials message.

### Phase 5 — Profiles
Tasks: candidate/recruiter `GET`/`PUT me`.
Acceptance: role restrictions enforced; `cvUrl` validation; updates persisted.

### Phase 6 — Jobs
Tasks: create, update (Open only), close, reopen, public list (Open only, search/filter/paging), public details, recruiter "my jobs" with `applicationsCount`.
Acceptance: BR-J1..J7 covered; ownership → 404 for other recruiters; closing/reopening leaves applications untouched (test).

### Phase 7 — Applications
Tasks: apply, candidate list, details (dual-actor authorization), cancel, recruiter list per job, recruiter status change.
Acceptance: BR-P1..P11 covered; specifically tested: re-apply after cancel (allowed), duplicate active (409), apply on closed (409), recruiter status change on **closed** job (allowed), recruiter sending `Cancelled` (400), invalid transitions (409), concurrent cancel vs status change (409 via `RowVersion`).

### Phase 8 — Hardening
Tasks: rate limiting, CORS, health check, logging, response-type annotations, Swagger examples, review of all services for ownership checks and N+1, `.http` file (or Postman collection) with the full flow.
Acceptance: security checklist (section 9) satisfied; no endpoint missing an explicit authorization attribute.

### Phase 9 — Optional extras (only after 1–8 are done and approved)
Refresh tokens; `ApplicationStatusHistory`; email notifications abstraction (interface only); Redis caching for the public jobs list.

### Phase 10 — Documentation & Final Review
Tasks: update `README.md` (architecture, how to run, config/secrets, migrations, endpoint list, business rules summary, how to run tests); final review against Definition of Done.
Acceptance: DoD (section 12) fully checked.

---

## 11. Test Plan

### 11.1 Unit tests (Domain & Application)
- Transition matrix: every (from, to) pair asserted allowed/denied with the correct error.
- `Application.Cancel` from each status; `ChangeStatusByRecruiter` never accepts `Cancelled`/`Applied`.
- `Job.Close/Reopen/Update` on Open/Closed.
- `Application.Create` on closed job and without CV.
- Validators: password policy, URL validation, pagination limits.
- Service tests (mocked repositories): ownership → `NotFound`; CV resolution order; duplicate detection.

### 11.2 Integration tests (`WebApplicationFactory` + SQL Server via Testcontainers or LocalDB)
Scenarios:
- **S1 Happy path:** register recruiter → login → create job → register candidate → login → list jobs → apply → recruiter lists applications → `UnderReview` → `Interview` → `Accepted`.
- **S2 Cancel & re-apply:** apply → cancel → apply again (201) → duplicate apply (409) → cancel from `Interview` (409).
- **S3 Closed job:** apply → recruiter closes job → new candidate apply (409) → recruiter moves existing application to `Interview` (200) → recruiter reopens → new apply (201).
- **S4 Authorization:** candidate calls recruiter endpoints (403); recruiter B accesses recruiter A's job/applications (404); candidate B reads candidate A's application (404); anonymous on protected route (401).
- **S5 Recruiter cannot cancel:** `PATCH status = Cancelled` → 400.
- **S6 Concurrency:** parallel cancel vs recruiter status change → exactly one wins, the other gets 409.
- **S7 Validation:** bad email/password/role/URL/pagination.

---

## 12. Definition of Done

- [ ] Solution builds with no new warnings; all tests green.
- [ ] Layer rules respected (verify project references; optional architecture test).
- [ ] No business logic in controllers; no direct `DbContext` in API.
- [ ] Every business rule (BR-*) is implemented **and** covered by at least one test.
- [ ] Result pattern used for all expected failures; no exceptions for control flow.
- [ ] Mapster used for all Entity → DTO mappings; no manual mapping left except justified cases.
- [ ] Ownership check present in every service method that touches an owned resource.
- [ ] Filtered unique index present and verified; re-apply after cancel works.
- [ ] Migrations apply cleanly; data migration verified against existing schema.
- [ ] Swagger documents all endpoints with auth and response types.
- [ ] README and `.http` file updated.
- [ ] No secrets committed.
