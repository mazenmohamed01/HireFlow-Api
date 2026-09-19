using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Testcontainers.MsSql;
using Xunit;

namespace HireFlow.Integration.Tests.Persistence;

public class ApplicationFilteredIndexTests : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer;
    private ApplicationDbContext _dbContext = null!;
    private DbContextOptions<ApplicationDbContext> _options = null!;

    public ApplicationFilteredIndexTests()
    {
        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_msSqlContainer.GetConnectionString())
            .Options;

        _dbContext = new ApplicationDbContext(_options);
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _msSqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task FilteredIndex_AllowsReapplyAfterCancel_ButBlocksDuplicates()
    {
        // Arrange - setup seed data
        var now = DateTime.UtcNow;
        var user = User.Create("John", "john@example.com", "hash", UserRole.Candidate, now);
        var recruiterUser = User.Create("Jane", "jane@example.com", "hash2", UserRole.Recruiter, now);
        
        await _dbContext.Users.AddRangeAsync(user, recruiterUser);
        await _dbContext.SaveChangesAsync();

        var candidate = Candidate.Create(user.Id, "http://cv.com", "1234");
        var recruiter = Recruiter.Create(recruiterUser.Id, "Company");

        await _dbContext.Candidates.AddAsync(candidate);
        await _dbContext.Recruiters.AddAsync(recruiter);
        await _dbContext.SaveChangesAsync();

        var job = Job.Create(recruiter.Id, "Dev", "Desc", "Remote", JobType.FullTime, now).Value;
        await _dbContext.Jobs.AddAsync(job);
        await _dbContext.SaveChangesAsync();

        // 1. First Apply
        var app1 = JobApplication.Create(candidate.Id, job, null, "http://cv.com", null, now).Value;
        await _dbContext.JobApplications.AddAsync(app1);
        await _dbContext.SaveChangesAsync(); // Should succeed

        // 2. Duplicate Apply -> Should Fail (Unique Index Violation)
        var app2 = JobApplication.Create(candidate.Id, job, null, "http://cv2.com", null, now.AddHours(1)).Value;
        await _dbContext.JobApplications.AddAsync(app2);
        
        var ex = await Should.ThrowAsync<DbUpdateException>(async () => await _dbContext.SaveChangesAsync());
        ex.InnerException!.Message.ShouldContain("IX_JobApplications_CandidateId_JobId");

        // Clear tracking so we can proceed
        _dbContext.ChangeTracker.Clear();

        // 3. Cancel first application
        var trackedApp1 = await _dbContext.JobApplications.FirstAsync(a => a.Id == app1.Id);
        trackedApp1.Cancel(now.AddHours(2));
        await _dbContext.SaveChangesAsync(); // Should succeed

        // 4. Re-apply -> Should Succeed because first is Cancelled
        var app3 = JobApplication.Create(candidate.Id, job, null, "http://cv3.com", null, now.AddHours(3)).Value;
        await _dbContext.JobApplications.AddAsync(app3);
        await _dbContext.SaveChangesAsync(); // Must not throw
        
        app3.Id.ShouldBeGreaterThan(0);
    }
}
