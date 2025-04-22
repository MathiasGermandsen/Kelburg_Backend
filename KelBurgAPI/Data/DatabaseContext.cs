using Microsoft.EntityFrameworkCore;
using KelBurgAPI.Models;

namespace KelBurgAPI.Data;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
    
    public DbSet<Users> users { get; set; }
    public DbSet<Rooms> rooms { get; set; }
    public DbSet<Bookings> booking { get; set; }
    public DbSet<Tickets> tickets { get; set; }
    public DbSet<Services> services { get; set; }
    public DbSet<HotelCars> hotelcars { get; set; }
}