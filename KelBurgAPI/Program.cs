using KelBurgAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace KelBurgAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and your JWT token.",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
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
                        new string[] { }
                    }
                });
            });

            IConfiguration Configuration = builder.Configuration;
            
            string secretFilePath = Environment.GetEnvironmentVariable("DefaultConnection");
            string connectionString = null;
            if (!string.IsNullOrEmpty(secretFilePath) && File.Exists(secretFilePath))
            {
                connectionString = File.ReadAllText(secretFilePath).Trim();
            }
            else
            {
                connectionString = Configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("DefaultConnection string is not set.");
            }

            builder.Services.AddDbContext<DatabaseContext>(options =>
                options.UseNpgsql(connectionString));
            
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var jwtKey = GetSecretValue(Configuration["JwtSettings:Key"]);
                var jwtIssuer = GetSecretValue(Configuration["JwtSettings:Issuer"]);
                var jwtAudience = GetSecretValue(Configuration["JwtSettings:Audience"]);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });
            
            var app = builder.Build();

            ApplyMigrations(app);
            
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            
            app.MapGet("/", async context =>
            {
                context.Response.Redirect("/swagger");
                await Task.CompletedTask;
            });

            // Console.WriteLine($"JWT Key: {Configuration["JwtSettings:Key"]}");
            // Console.WriteLine($"JWT Issuer: {Configuration["JwtSettings:Issuer"]}");
            // Console.WriteLine($"JWT Audience: {Configuration["JwtSettings:Audience"]}");

            app.Run();
        }

        public static void ApplyMigrations(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                dbContext.Database.Migrate();
            }
        }
        
        public static string GetSecretValue(string keyOrFilePath)
        {
            if (keyOrFilePath != null && File.Exists(keyOrFilePath))
            {
                return File.ReadAllText(keyOrFilePath).Trim();
            }
            return keyOrFilePath;
        }
    }
}