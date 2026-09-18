using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Errors;
using Shouldly;
using Xunit;

namespace HireFlow.Domain.Tests;

public class JobTests
{
    private static readonly DateTime Now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ReturnsJob_WithStatusOpen()
    {
        // Act
        var result = Job.Create(1, "Dev", "Desc", "Remote", JobType.FullTime, Now);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var job = result.Value;
        job.Status.ShouldBe(JobStatus.Open);
        job.CreatedAt.ShouldBe(Now);
        job.ClosedAt.ShouldBeNull();
        job.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public void Update_WhenOpen_Succeeds()
    {
        // Arrange
        var job = Job.Create(1, "Dev", "Desc", "Remote", JobType.FullTime, Now).Value;

        // Act
        var result = job.Update("New Dev", "New Desc", "Office", JobType.PartTime, Now.AddDays(1));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        job.Title.ShouldBe("New Dev");
        job.UpdatedAt.ShouldBe(Now.AddDays(1));
    }

    [Fact]
    public void Update_WhenClosed_Fails()
    {
        // Arrange
        var job = Job.Create(1, "Dev", "Desc", "Remote", JobType.FullTime, Now).Value;
        job.Close(Now);

        // Act
        var result = job.Update("New Dev", "New Desc", "Office", JobType.PartTime, Now.AddDays(1));

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.Job.NotEditable);
    }

    [Fact]
    public void Close_WhenOpen_Succeeds()
    {
        // Arrange
        var job = Job.Create(1, "Dev", "Desc", "Remote", JobType.FullTime, Now).Value;

        // Act
        var result = job.Close(Now.AddDays(1));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        job.Status.ShouldBe(JobStatus.Closed);
        job.ClosedAt.ShouldBe(Now.AddDays(1));
        job.IsOpen.ShouldBeFalse();
    }

    [Fact]
    public void Close_WhenAlreadyClosed_Fails()
    {
        // Arrange
        var job = Job.Create(1, "Dev", "Desc", "Remote", JobType.FullTime, Now).Value;
        job.Close(Now);

        // Act
        var result = job.Close(Now);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.Job.AlreadyClosed);
    }

    [Fact]
    public void Reopen_WhenClosed_Succeeds()
    {
        // Arrange
        var job = Job.Create(1, "Dev", "Desc", "Remote", JobType.FullTime, Now).Value;
        job.Close(Now);

        // Act
        var result = job.Reopen(Now.AddDays(1));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        job.Status.ShouldBe(JobStatus.Open);
        job.ClosedAt.ShouldBeNull();
        job.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public void Reopen_WhenAlreadyOpen_Fails()
    {
        // Arrange
        var job = Job.Create(1, "Dev", "Desc", "Remote", JobType.FullTime, Now).Value;

        // Act
        var result = job.Reopen(Now);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.Job.AlreadyOpen);
    }
}
