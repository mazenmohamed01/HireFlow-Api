namespace HireFlow.Domain.Enums;

/// <summary>
/// Replaces JobApplicationStatus. Values stored as string per plan section 3.2.
/// </summary>
public enum ApplicationStatus
{
    Applied = 1,
    UnderReview = 2,
    Interview = 3,
    Accepted = 4,
    Rejected = 5,
    Cancelled = 6
}
