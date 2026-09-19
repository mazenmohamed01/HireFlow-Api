namespace HireFlow.Application.DTOs.Candidates;

public sealed record CandidateProfileDto(
    int Id,
    int UserId,
    string FullName,
    string Email,
    string? CvUrl,
    string? Phone);

public sealed record UpdateCandidateProfileRequest(
    string FullName,
    string? Phone,
    string? CvUrl);
