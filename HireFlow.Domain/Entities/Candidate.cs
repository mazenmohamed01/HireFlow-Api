namespace HireFlow.Domain.Entities;

public class Candidate
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string? CvUrl { get; private set; }
    public string? Phone { get; private set; }

    private Candidate() { }

    public static Candidate Create(int userId, string? cvUrl, string? phone)
    {
        return new Candidate
        {
            UserId = userId,
            CvUrl = cvUrl,
            Phone = phone
        };
    }

    public void UpdateProfile(string? cvUrl, string? phone)
    {
        CvUrl = cvUrl;
        Phone = phone;
    }
}
