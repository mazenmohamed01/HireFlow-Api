namespace HireFlow.Application.DTOs.Profiles;

public record CandidateProfileDto(
    int Id,
    string FullName,
    string Email,
    string? Phone,
    string? CvUrl
);

public record UpdateCandidateProfileRequest(
    string FullName,
    string? Phone,
    string? CvUrl
);

public record RecruiterProfileDto(
    int Id,
    string FullName,
    string Email,
    string CompanyName
);

public record UpdateRecruiterProfileRequest(
    string FullName,
    string CompanyName
);
