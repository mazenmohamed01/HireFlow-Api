using HireFlow.API.Middleware;
using HireFlow.API.Services;
using HireFlow.Application.Common;
using HireFlow.Application.Common.Behaviors;
using HireFlow.Application.Interfaces;
using HireFlow.Infrastructure.Persistence;
using HireFlow.Infrastructure.Security;
using HireFlow.Infrastructure.Repositories;
using FluentValidation;
using MediatR;

using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

namespace HireFlow.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── Serilog ───────────────────────────────────────────────────────────
        builder.Host.UseSerilog((context, loggerConfig) => 
            loggerConfig.ReadFrom.Configuration(context.Configuration));

        // ── Controllers ───────────────────────────────────────────────────────
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
        mapsterConfig.Scan(Assembly.GetAssembly(typeof(ICurrentUser))!);
        builder.Services.AddSingleton(mapsterConfig);
        builder.Services.AddScoped<IMapper, ServiceMapper>();

        // ── Repositories / Services ───────────────────────────────────────
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
        builder.Services.AddScoped<IRecruiterRepository, RecruiterRepository>();
        builder.Services.AddScoped<IJobRepository, JobRepository>();
        builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        builder.Services.AddSingleton<IPasswordHasher, PasswordHasherService>();
        builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // ── MediatR (CQRS) ────────────────────────────────────────────────
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(ICurrentUser))!);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // ── FluentValidation (all validators in Application assembly) ─────
        builder.Services.AddValidatorsFromAssembly(
            Assembly.GetAssembly(typeof(ICurrentUser)),
            includeInternalTypes: true);

        // ── Unit of Work ──────────────────────────────────────────────────
        builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // ── Identity ──────────────────────────────────────────────────────
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();

        // TimeProvider (built-in .NET 8+) — injectable singleton
        builder.Services.AddSingleton(TimeProvider.System);

        // ── Global exception handler ──────────────────────────────────────
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // ── Authentication & Authorization ──────────────────────────────────
        var jwtKey = builder.Configuration["Jwt:Key"] ?? "super-secret-key-that-must-be-very-long!";
        var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "HireFlowAPI";
        var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "HireFlowClients";

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        builder.Services.AddAuthorization();

        // ── Rate Limiting ──────────────────────────────────────────────────────────
        builder.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("AuthPolicy", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 5;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 2;
            });
        });

        // ── CORS ──────────────────────────────────────────────────────────────────
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DefaultCorsPolicy", policy =>
            {
                var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
                }
                else
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            });
        });

        // ── Swagger ───────────────────────────────────────────────────────────────
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "HireFlow API", 
                Version = "v1",
                Description = "A clean-architecture recruitment platform API supporting Candidates and Recruiters.",
                Contact = new OpenApiContact
                {
                    Name = "HireFlow Support",
                    Email = "mazenmohamedsalah77@gmail.com"
                }
            });
            
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Enter your JWT token directly.\r\n\r\nExample: eyJhbGciOiJIUzI1NiIsInR5...",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML Comments
            var apiXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
            if (File.Exists(apiXmlPath)) c.IncludeXmlComments(apiXmlPath);

            var applicationXmlFile = "HireFlow.Application.xml";
            var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
            if (File.Exists(applicationXmlPath)) c.IncludeXmlComments(applicationXmlPath);
        });

        // ── Health checks ──────────────────────────────────────────────────────────
        builder.Services.AddHealthChecks();

        // ─────────────────────────────────────────────────────────────────
        var app = builder.Build();

        // ── Exception handler (must be first) ─────────────────────────────
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "HireFlow API v1");
                c.DocumentTitle = "HireFlow API Documentation";
                c.EnableDeepLinking();
                c.DisplayRequestDuration();
            });
        }

        app.UseCors("DefaultCorsPolicy");
        app.UseRateLimiter();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }
}
