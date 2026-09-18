
using HireFlow.Domain.Shared;

namespace HireFlow.Domain.Errors;

public static class DomainErrors
{
    public static class Auth
    {
        public static readonly Error EmailAlreadyExists = new(
            "Auth.EmailAlreadyExists",
            "A user with this email already exists.",
            ErrorType.Conflict);

        public static readonly Error InvalidCredentials = new(
            "Auth.InvalidCredentials",
            "Invalid email or password.",
            ErrorType.Unauthorized);

        public static readonly Error InvalidRole = new(
            "Auth.InvalidRole",
            "Role must be Candidate or Recruiter.",
            ErrorType.Validation);
    }

    public static class Candidate
    {
        public static readonly Error NotFound = new(
            "Candidate.NotFound",
            "Candidate profile was not found.",
            ErrorType.NotFound);
    }

    public static class Recruiter
    {
        public static readonly Error NotFound = new(
            "Recruiter.NotFound",
            "Recruiter profile was not found.",
            ErrorType.NotFound);
    }

    public static class Job
    {
        public static readonly Error NotFound = new(
            "Job.NotFound",
            "The job was not found or you do not have permission to access it.",
            ErrorType.NotFound);

        public static readonly Error NotOpen = new(
            "Job.NotOpen",
            "The job is not open for new applications.",
            ErrorType.Conflict);

        public static readonly Error AlreadyClosed = new(
            "Job.AlreadyClosed",
            "The job is already closed.",
            ErrorType.Conflict);

        public static readonly Error AlreadyOpen = new(
            "Job.AlreadyOpen",
            "The job is already open.",
            ErrorType.Conflict);

        public static readonly Error NotEditable = new(
            "Job.NotEditable",
            "Only open jobs can be edited.",
            ErrorType.Conflict);
    }

    public static class Application
    {
        public static readonly Error NotFound = new(
            "Application.NotFound",
            "The application was not found or you do not have permission to access it.",
            ErrorType.NotFound);

        public static readonly Error AlreadyExists = new(
            "Application.AlreadyExists",
            "You have already applied to this job.",
            ErrorType.Conflict);

        public static readonly Error CvRequired = new(
            "Application.CvRequired",
            "A CV URL must be provided or available on the candidate profile.",
            ErrorType.Validation);

        public static readonly Error CannotCancel = new(
            "Application.CannotCancel",
            "Only applications that are 'Applied' or 'UnderReview' can be cancelled.",
            ErrorType.Conflict);

        public static readonly Error StatusNotAllowed = new(
            "Application.StatusNotAllowed",
            "Recruiters cannot set the application status to 'Applied' or 'Cancelled'.",
            ErrorType.Validation);

        public static readonly Error InvalidTransition = new(
            "Application.InvalidTransition",
            "The requested status transition is not allowed.",
            ErrorType.Conflict);
    }

    public static class Concurrency
    {
        public static readonly Error Conflict = new(
            "Concurrency.Conflict",
            "The resource was modified by another user. Please reload and try again.",
            ErrorType.Conflict);
    }
}
