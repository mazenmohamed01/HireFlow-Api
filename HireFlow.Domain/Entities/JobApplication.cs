namespace HireFlow.Domain.Entities;

/// <summary>
/// Represents a candidate's application for a job posting.
/// Full domain behavior (Create, Cancel, ChangeStatusByRecruiter) is added in Phase 2.
/// </summary>
public class JobApplication
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
    public int JobId { get; set; }
    public Job Job { get; set; } = null!;
    public DateTime AppliedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
