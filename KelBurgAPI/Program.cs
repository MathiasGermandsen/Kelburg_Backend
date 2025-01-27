using KelBurgAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace KelBurgAPI
{
    public class program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            
            IConfiguration Configuration = builder.Configuration;

            string connectionString = Configuration.GetConnectionString("DefaultConnection") 
                                      ?? Environment.GetEnvironmentVariable("DefaultConnection");

            builder.Services.AddDbContext<AppDBContext>(options =>
                options.UseNpgsql(connectionString));
            
            // Configure JWT Authentication
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = Configuration["JwtSettings:Issuer"],
                    ValidAudience = Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey
                    (
                        Encoding.UTF8.GetBytes(Configuration["JwtSettings:Key"])
                    ),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });
            
            var app = builder.Build();
            
            app.UseSwagger();
            app.UseSwaggerUI();
            
            app.UseHttpsRedirection();
            
            app.UseAuthorization();
            
            app.MapControllers();
            
            app.Run();
        }
    }
}

