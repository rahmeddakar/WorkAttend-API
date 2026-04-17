using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace WorkAttend.SecurityToken
{
    public class TokenGenerator
    {
        private readonly VariableConfiguration _jwtConfig;

        public TokenGenerator(IConfiguration configuration)
        {
            _jwtConfig = VariableConfiguration.FromConfiguration(configuration);
        }

        public string GenerateToken(
            string userId,
            string userName,
            string email,
            string role,
            string databaseName,
            string companyUrl)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.UniqueName, userName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, email ?? string.Empty),

                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName ?? string.Empty),
                new Claim(ClaimTypes.Email, email ?? string.Empty),
                new Claim(ClaimTypes.Role, role ?? string.Empty),

                new Claim("databaseName", databaseName ?? string.Empty),
                new Claim("username", userName ?? string.Empty),
                new Claim("companyURL", companyUrl ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtConfig.ExpireMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string? GetDatabaseName(ClaimsPrincipal user)
            => user.FindFirst("databaseName")?.Value;

        public static string? GetUserName(ClaimsPrincipal user)
            => user.FindFirst("username")?.Value ?? user.FindFirst(ClaimTypes.Name)?.Value;

        public static string? GetCompanyURL(ClaimsPrincipal user)
            => user.FindFirst("companyURL")?.Value;

        public int GetExpireMinutes()
        {
            return _jwtConfig.ExpireMinutes;
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return WebEncoders.Base64UrlEncode(randomBytes);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                ValidIssuer = _jwtConfig.Issuer,
                ValidAudience = _jwtConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out Microsoft.IdentityModel.Tokens.SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}
