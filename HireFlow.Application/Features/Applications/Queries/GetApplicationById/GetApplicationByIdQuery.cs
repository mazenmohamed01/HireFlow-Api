using HireFlow.Application.Common.Messaging;
using HireFlow.Application.DTOs.Applications;

namespace HireFlow.Application.Features.Applications.Queries.GetApplicationById;

/// <summary>Query to return application details. Visible to owning candidate or recruiter who owns the job.</summary>
public sealed record GetApplicationByIdQuery(int ApplicationId) : IQuery<ApplicationDto>;
