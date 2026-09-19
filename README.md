# HireFlow API

HireFlow is a scalable, Clean Architecture ASP.NET Core API for recruitment.
It supports candidates and recruiters, handling job postings, candidate applications, and application tracking workflows.

## Architecture & Tech Stack
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, API).
- **Domain Pattern**: Rich Domain Model, Result/Error patterns for control flow (avoiding exceptions).
- **Framework**: .NET 8 / .NET 10
- **Database**: Entity Framework Core with SQL Server
- **Security**: JWT Authentication, Role-based policies (`CandidateOnly`, `RecruiterOnly`), Rate Limiting.
- **Mapping**: Mapster (via `IRegister`)

## How to Run

1. **Configure Secrets**: 
   Add a User Secret or an environment variable for `ConnectionStrings:DefaultConnection`.
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=HireFlowDb;Trusted_Connection=True;"
   ```

2. **Apply Migrations**:
   The InitialSchema is already created. Update your database using:
   ```bash
   dotnet ef database update --project HireFlow.Infrastructure --startup-project HireFlow.API
   ```

3. **Run**:
   ```bash
   dotnet run --project HireFlow.API
   ```

4. **Testing Endpoints**:
   A `.http` file is provided (`HireFlow.http`) in the root directory which contains the full flow from Registration to Application Status updates.
   Alternatively, visit `https://localhost:5001/scalar/v1` to view the OpenAPI endpoints interface.

## Business Rules Highlights
- **Recruiters** own their jobs. They can create, update (if Open), close, and reopen jobs. They can review and change statuses for applications.
- **Candidates** can apply for open jobs, track their own applications, and cancel them (as long as they are not terminal).
- **Security**: Jobs and Applications are strictly bound to their owners. Operations on unauthorized resources will return `404 Not Found` to prevent data leakage.
- **Data Integrity**: Using `RowVersion` for optimistic concurrency. EF Core filtered unique indexes ensure candidates cannot apply to the same job twice (but can re-apply if cancelled).
