namespace HireFlow.Domain.Entities;

public class Recruiter
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string CompanyName { get; private set; } = string.Empty;

    private Recruiter() { }

    public static Recruiter Create(int userId, string companyName)
    {
        return new Recruiter
        {
            UserId = userId,
            CompanyName = companyName
        };
    }

    public void UpdateProfile(string companyName)
    {
        CompanyName = companyName;
    }

    public void UpdateCompanyName(string companyName)
    {
        CompanyName = companyName;
    }
}
