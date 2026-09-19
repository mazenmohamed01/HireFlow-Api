namespace HireFlow.Application.DTOs.Recruiters;

public sealed record RecruiterProfileDto(
    int Id,
    int UserId,
    string FullName,
    string Email,
    string CompanyName);

public sealed record UpdateRecruiterProfileRequest(
    string FullName,
    string CompanyName);
