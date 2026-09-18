using HireFlow.Domain.Enums;

namespace HireFlow.Domain.Entities;

public class User
{
    public int Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Candidate? Candidate { get; private set; }
    public Recruiter? Recruiter { get; private set; }

    private User() { }

    public static User Create(string fullName, string email, string passwordHash, UserRole role, DateTime now)
    {
        return new User
        {
            FullName = fullName,
            Email = email,
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = now
        };
    }
}
