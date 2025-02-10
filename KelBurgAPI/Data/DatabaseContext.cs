using Microsoft.EntityFrameworkCore;
using KelBurgAPI.Models;

namespace KelBurgAPI.Data;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
    
    public DbSet<Users> Users { get; set; }
    public DbSet<Rooms> Rooms { get; set; }
    public DbSet<Bookings> Booking { get; set; }
    public DbSet<Tickets> Tickets { get; set; }
    public DbSet<Services> Services { get; set; }

}