using Microsoft.EntityFrameworkCore;
using KelburgAPI.Data;
namespace KelburgAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

            // Specify when frontend is created
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    name: MyAllowSpecificOrigins,
                    policy =>
                    {
                        policy.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
                    }
                );
            });

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                });

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure Entity Framework with PostgreSQL
            IConfiguration Configuration = builder.Configuration;

            // Try to read connection string from the Docker secret file
            string secretFilePath = Environment.GetEnvironmentVariable("DefaultConnection");
            string connectionString = null;

            if (!string.IsNullOrEmpty(secretFilePath) && File.Exists(secretFilePath))
            {
                connectionString = File.ReadAllText(secretFilePath).Trim(); // Trim to remove unnecessary spaces or newlines
            }
            else
            {
                // Fallback to environment variable or appsettings.json
                connectionString = Configuration.GetConnectionString("DefaultConnection") 
                                   ?? throw new InvalidOperationException("DefaultConnection string is not set.");
            }

            // Register the database context
            builder.Services.AddDbContext<DatabaseContext>(options =>
                options.UseNpgsql(connectionString));

            WebApplication app = builder.Build();

            app.UseCors(MyAllowSpecificOrigins);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();

            app.Run();
        }
    }
}
