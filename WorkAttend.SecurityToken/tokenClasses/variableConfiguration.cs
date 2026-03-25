using Microsoft.Extensions.Configuration;

namespace WorkAttend.SecurityToken
{
    public class VariableConfiguration
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int ExpireMinutes { get; set; }

        public static VariableConfiguration FromConfiguration(IConfiguration configuration)
        {
            return new VariableConfiguration
            {
                Issuer = configuration["JwtSettings:Issuer"] ?? string.Empty,
                Audience = configuration["JwtSettings:Audience"] ?? string.Empty,
                Key = configuration["JwtSettings:Key"] ?? string.Empty,
                ExpireMinutes = int.TryParse(configuration["JwtSettings:ExpireMinutes"], out var minutes) ? minutes : 120
            };
        }
    }
}