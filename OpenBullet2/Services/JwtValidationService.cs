using Microsoft.IdentityModel.Tokens;
using OpenBullet2.Core.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OpenBullet2.Services;

public class JwtValidationService
{
    private readonly JwtSecurityTokenHandler handler;
    private readonly TokenValidationParameters validationParams;

    public JwtValidationService(OpenBulletSettingsService settingsService)
    {
        handler = new JwtSecurityTokenHandler();
        validationParams = new TokenValidationParameters {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(settingsService.Settings.SecuritySettings.JwtKey),
            ClockSkew = TimeSpan.Zero
        };
    }

    public ClaimsPrincipal ValidateToken(string jwt, out SecurityToken validatedToken) =>
        handler.ValidateToken(jwt, validationParams, out validatedToken);
}