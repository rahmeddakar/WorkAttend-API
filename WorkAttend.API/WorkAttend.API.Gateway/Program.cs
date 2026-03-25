using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;
using System.Text;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.BLL.ServicesBLL;
using WorkAttend.API.Gateway.DAL.Repositories;
using WorkAttend.API.Gateway.DAL.services.ActivityServices;
using WorkAttend.API.Gateway.DAL.services.AdminPanelServices;
using WorkAttend.API.Gateway.DAL.services.AdminsServices;
using WorkAttend.API.Gateway.DAL.services.EmployeeServices;
using WorkAttend.SecurityToken;

namespace WorkAttend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Host.UseNLog();
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WorkAttend API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste only the raw JWT token here."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            });

            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<IUserAccessContextManager, UserAccessContextManager>();
            builder.Services.AddScoped<PunchMobileRepository>();
            builder.Services.AddScoped<IAdminsManager, AdminsManager>();
            builder.Services.AddScoped<IAdminsService, AdminsService>(); 
            builder.Services.AddScoped<IEmployeeService, EmployeesService>();
            builder.Services.AddScoped<IEmployeesManager, EmployeeManager>();
            builder.Services.AddScoped<TokenGenerator>();
            builder.Services.AddScoped<IActivityManager, ActivityManager>();
            builder.Services.AddScoped<IActivityService, ActivityService>();
            builder.Services.AddScoped<IAdminPanelManager, AdminPanelManager>();
            builder.Services.AddScoped<IAdminPanelService, AdminPanelService>();

            var jwtKey = builder.Configuration["JwtSettings:Key"];
            var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
            var jwtAudience = builder.Configuration["JwtSettings:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("JwtSettings:Key is missing in appsettings.json");

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                        ValidAudience = builder.Configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!)),
                        ClockSkew = TimeSpan.Zero
                    };
                });            

            builder.Services.AddAuthorization();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
