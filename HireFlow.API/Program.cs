using HireFlow.API.Middleware;
using HireFlow.API.Services;
using HireFlow.Application.Common;
using HireFlow.Application.Interfaces;
using HireFlow.Application.Services.Jobs;
using HireFlow.Infrastructure.Persistence;
using HireFlow.Infrastructure.Repositories;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Reflection;

namespace HireFlow.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── Controllers ───────────────────────────────────────────────────
        builder.Services.AddControllers();

        // ── Database ──────────────────────────────────────────────────────
        var connectionString =
            builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. " +
                "Add it to User Secrets or environment variables.");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // ApplicationDbContext also implements IUnitOfWork
        builder.Services.AddScoped<HireFlow.Application.Common.IUnitOfWork>(
            sp => sp.GetRequiredService<ApplicationDbContext>());

        // ── Mapster ───────────────────────────────────────────────────────
        // Scan Application assembly for IRegister mapping profiles.
        var mapsterConfig = new TypeAdapterConfig();
        mapsterConfig.Scan(Assembly.GetAssembly(typeof(IJobService))!);
        builder.Services.AddSingleton(mapsterConfig);
        builder.Services.AddScoped<IMapper, ServiceMapper>();

        // ── Repositories / Services ───────────────────────────────────────
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
        builder.Services.AddScoped<IRecruiterRepository, RecruiterRepository>();
        builder.Services.AddScoped<IJobRepository, JobRepository>();
        builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        builder.Services.AddScoped<IJobService, JobService>();

        // ── Unit of Work ──────────────────────────────────────────────────
        builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // ── Identity (Stub for Phase 1-3) ─────────────────────────────────
        builder.Services.AddScoped<ICurrentUser, DummyCurrentUser>();

        // TimeProvider (built-in .NET 8+) — injectable singleton
        builder.Services.AddSingleton(TimeProvider.System);

        // ── Global exception handler ──────────────────────────────────────
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // ── Authentication (placeholder; wired up in Phase 4) ────────────
        // builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)…
        // builder.Services.AddAuthorization(…);

        // ── OpenAPI / Scalar ──────────────────────────────────────────────
        builder.Services.AddOpenApi();

        // ── Health checks ─────────────────────────────────────────────────
        builder.Services.AddHealthChecks();

        // ─────────────────────────────────────────────────────────────────
        var app = builder.Build();

        // ── Exception handler (must be first) ─────────────────────────────
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        // Authentication / Authorization middleware stubs (Phase 4 activates these).
        // app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }
}
