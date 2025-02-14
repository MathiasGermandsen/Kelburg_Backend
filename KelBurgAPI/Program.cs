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

            // 1) Add services to the container
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

            // 2) Configure database
            // Try to get the connection string from a Docker secret file, falling back to configuration.
            string secretFilePath = Environment.GetEnvironmentVariable("DefaultConnection");
            string connectionString = null;
            if (!string.IsNullOrEmpty(secretFilePath) && File.Exists(secretFilePath))
            {
                connectionString = File.ReadAllText(secretFilePath).Trim(); // Remove unnecessary spaces or newlines
            }
            else
            {
                connectionString = Configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("DefaultConnection string is not set.");
            }

            // Register the database context
            builder.Services.AddDbContext<DatabaseContext>(options =>
                options.UseNpgsql(connectionString));

            // 3) Configure JWT authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                // Use the helper method to load secret values.
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

            // 4) Build the app
            var app = builder.Build();

            ApplyMigrations(app);

            // 5) Configure middleware
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseAuthorization();

            // 6) Map controllers
            app.MapControllers();

            // Optional: redirect root to Swagger UI
            app.MapGet("/", async context =>
            {
                context.Response.Redirect("/swagger");
                await Task.CompletedTask;
            });

            // Logging JWT settings for debugging (if needed)
            Console.WriteLine($"JWT Key: {Configuration["JwtSettings:Key"]}");
            Console.WriteLine($"JWT Issuer: {Configuration["JwtSettings:Issuer"]}");
            Console.WriteLine($"JWT Audience: {Configuration["JwtSettings:Audience"]}");

            // 7) Run the application
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
            if (File.Exists(keyOrFilePath))
            {
                return File.ReadAllText(keyOrFilePath).Trim();
            }
            return keyOrFilePath;
        }
    }
}