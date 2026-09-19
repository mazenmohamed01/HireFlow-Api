using HireFlow.Application.Common;
using HireFlow.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HireFlow.Infrastructure.Security;

public sealed class JwtTokenService(IConfiguration configuration, TimeProvider timeProvider) : IJwtTokenService
{
    public (string AccessToken, DateTime ExpiresAtUtc) Generate(User user, int profileId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (user.Role == Domain.Enums.UserRole.Candidate)
        {
            claims.Add(new Claim("CandidateId", profileId.ToString()));
        }
        else if (user.Role == Domain.Enums.UserRole.Recruiter)
        {
            claims.Add(new Claim("RecruiterId", profileId.ToString()));
        }

        var expires = timeProvider.GetUtcNow().UtcDateTime.AddHours(2);
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
