using HireFlow.Application.Common.Messaging;
using HireFlow.Application.DTOs.Jobs;

namespace HireFlow.Application.Features.Jobs.Queries.GetJobById;

/// <summary>Query to return a single job by id (open or closed).</summary>
public sealed record GetJobByIdQuery(int JobId) : IQuery<JobDto>;
