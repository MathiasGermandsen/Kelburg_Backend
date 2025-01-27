using Microsoft.EntityFrameworkCore;
using KelBurgAPI.Models;

namespace KelburgAPI.Data;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
    
    public DbSet<Users> Users { get; set; }
    public DbSet<Rooms> Rooms { get; set; }
    public DbSet<Booking> Booking { get; set; }
    public DbSet<Tickets> Tickets { get; set; }
    public DbSet<ServicePricesDict> ServicePricesDict { get; set; }
}