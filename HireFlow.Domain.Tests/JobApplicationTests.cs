using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Errors;
using Shouldly;
using Xunit;

namespace HireFlow.Domain.Tests;

public class JobApplicationTests
{
    private static readonly DateTime Now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Job CreateOpenJob() => Job.Create(1, "Dev", "Desc", "Remote", JobType.FullTime, Now).Value;
    private static Job CreateClosedJob()
    {
        var job = CreateOpenJob();
        job.Close(Now);
        return job;
    }

    [Fact]
    public void Create_WhenJobIsOpen_AndCvProvided_Succeeds()
    {
        var job = CreateOpenJob();

        var result = JobApplication.Create(1, job, "http://cv.com", null, "Cover", Now);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe(ApplicationStatus.Applied);
        result.Value.CvUrl.ShouldBe("http://cv.com");
    }

    [Fact]
    public void Create_WhenJobIsClosed_Fails()
    {
        var job = CreateClosedJob();

        var result = JobApplication.Create(1, job, "http://cv.com", null, null, Now);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.Job.NotOpen);
    }

    [Fact]
    public void Create_WhenNoCvProvided_Fails()
    {
        var job = CreateOpenJob();

        var result = JobApplication.Create(1, job, null, null, null, Now);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.Application.CvRequired);
    }

    [Fact]
    public void Create_FallsBackToProfileCv()
    {
        var job = CreateOpenJob();

        var result = JobApplication.Create(1, job, null, "http://profile-cv.com", null, Now);

        result.IsSuccess.ShouldBeTrue();
        result.Value.CvUrl.ShouldBe("http://profile-cv.com");
    }

    [Fact]
    public void Cancel_WhenApplied_Succeeds()
    {
        var app = JobApplication.Create(1, CreateOpenJob(), "cv", null, null, Now).Value;

        var result = app.Cancel(Now.AddDays(1));

        result.IsSuccess.ShouldBeTrue();
        app.Status.ShouldBe(ApplicationStatus.Cancelled);
        app.CancelledAt.ShouldBe(Now.AddDays(1));
    }

    [Fact]
    public void Cancel_WhenUnderReview_Succeeds()
    {
        var app = JobApplication.Create(1, CreateOpenJob(), "cv", null, null, Now).Value;
        app.ChangeStatusByRecruiter(ApplicationStatus.UnderReview, Now);

        var result = app.Cancel(Now.AddDays(1));

        result.IsSuccess.ShouldBeTrue();
        app.Status.ShouldBe(ApplicationStatus.Cancelled);
    }

    [Theory]
    [InlineData(ApplicationStatus.Interview)]
    [InlineData(ApplicationStatus.Accepted)]
    [InlineData(ApplicationStatus.Rejected)]
    public void Cancel_WhenInOtherStatus_Fails(ApplicationStatus status)
    {
        var app = JobApplication.Create(1, CreateOpenJob(), "cv", null, null, Now).Value;
        
        if (status == ApplicationStatus.Interview || status == ApplicationStatus.Accepted || status == ApplicationStatus.Rejected)
            app.ChangeStatusByRecruiter(ApplicationStatus.UnderReview, Now);
        if (status == ApplicationStatus.Interview || status == ApplicationStatus.Accepted || status == ApplicationStatus.Rejected)
            app.ChangeStatusByRecruiter(ApplicationStatus.Interview, Now);
        if (status == ApplicationStatus.Accepted)
            app.ChangeStatusByRecruiter(ApplicationStatus.Accepted, Now);
        if (status == ApplicationStatus.Rejected)
            app.ChangeStatusByRecruiter(ApplicationStatus.Rejected, Now);

        var result = app.Cancel(Now.AddDays(1));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.Application.CannotCancel);
    }

    [Theory]
    [InlineData(ApplicationStatus.Applied)]
    [InlineData(ApplicationStatus.Cancelled)]
    public void ChangeStatus_ToAppliedOrCancelled_Fails(ApplicationStatus target)
    {
        var app = JobApplication.Create(1, CreateOpenJob(), "cv", null, null, Now).Value;

        var result = app.ChangeStatusByRecruiter(target, Now);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.Application.StatusNotAllowed);
    }

    [Theory]
    [InlineData(ApplicationStatus.UnderReview, ApplicationStatus.Interview)]
    [InlineData(ApplicationStatus.UnderReview, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Interview, ApplicationStatus.Accepted)]
    [InlineData(ApplicationStatus.Interview, ApplicationStatus.Rejected)]
    public void ChangeStatus_ValidTransitions_Succeed(ApplicationStatus from, ApplicationStatus to)
    {
        var app = JobApplication.Create(1, CreateOpenJob(), "cv", null, null, Now).Value;
        if (from == ApplicationStatus.Interview || from == ApplicationStatus.UnderReview)
            app.ChangeStatusByRecruiter(ApplicationStatus.UnderReview, Now);
        if (from == ApplicationStatus.Interview)
            app.ChangeStatusByRecruiter(ApplicationStatus.Interview, Now);

        var result = app.ChangeStatusByRecruiter(to, Now);

        result.IsSuccess.ShouldBeTrue();
        app.Status.ShouldBe(to);
    }

    [Fact]
    public void ChangeStatus_AppliedToInterview_Fails()
    {
        var app = JobApplication.Create(1, CreateOpenJob(), "cv", null, null, Now).Value;

        var result = app.ChangeStatusByRecruiter(ApplicationStatus.Interview, Now);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.Application.InvalidTransition);
    }

    [Fact]
    public void ChangeStatus_WhenTerminal_Fails()
    {
        var app = JobApplication.Create(1, CreateOpenJob(), "cv", null, null, Now).Value;
        app.ChangeStatusByRecruiter(ApplicationStatus.UnderReview, Now);
        app.ChangeStatusByRecruiter(ApplicationStatus.Interview, Now);
        app.ChangeStatusByRecruiter(ApplicationStatus.Accepted, Now); // Terminal

        var result = app.ChangeStatusByRecruiter(ApplicationStatus.UnderReview, Now);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.Application.InvalidTransition);
    }
}
