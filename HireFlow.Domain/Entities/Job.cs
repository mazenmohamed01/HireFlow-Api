namespace HireFlow.Domain.Entities;

public class Job
{
    public int Id { get; set; }
    public int RecruiterId { get; set; }
    public Recruiter Recruiter { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public bool IsActive { get; set; } // preserved for migration compatibility; Phase 3 replaces with Status
}
