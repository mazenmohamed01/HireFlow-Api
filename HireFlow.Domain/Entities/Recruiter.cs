namespace HireFlow.Domain.Entities;

public class Recruiter
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string CompanyName { get; set; } = string.Empty;
}
