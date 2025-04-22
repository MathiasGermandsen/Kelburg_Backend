using KelBurgAPI.BogusGenerators;
using KelBurgAPI.Data;
using KelBurgAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KelBurgAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class HotelCarsController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public HotelCarsController(DatabaseContext context)
        {
            _context = context;
        }

        [HttpPost("create")]

        public async Task<ActionResult<HotelCars>> CreateCar([FromBody] HotelCarsDTO hotelCars)
        {
            if (hotelCars == null)
            {
                return BadRequest("HotelCars is null");
            }

            HotelCars carsToBeCreated = new HotelCars()
            {
                Manufacturer = hotelCars.Manufacturer,
                Model = hotelCars.Model,
                Vin = hotelCars.Vin,
                Size = hotelCars.Size,
                Type = hotelCars.Type,
            };
            _context.hotelcars.Add(carsToBeCreated);
            await _context.SaveChangesAsync();  
            return carsToBeCreated;
        }

        [HttpGet("read")]
        public async Task<ActionResult<IEnumerable<HotelCars>>> GetHotelCars(int? carId, int? carSize, int pageSize = 100, int pageNumber = 1)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("PageNumber and size must be greater than 0");
            }
        
            IQueryable<HotelCars> query = _context.hotelcars.AsQueryable();

            if (carId.HasValue)
            {
                query = query.Where(c => c.Id == carId);
            }
            
            if (carSize.HasValue)
            {
                query = query.Where(c => c.Size == carSize);
            }

            List<HotelCars> cars = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(cars);
        }
        
        [HttpGet("availableBetweenDates")]
        public async Task<ActionResult<IEnumerable<HotelCars>>> GetAvailableBetweenDates(DateTime startDate, DateTime endDate, int? carSize, int pageSize = 100, int pageNumber = 1)
        {
            List<HotelCars> allCars = await _context.hotelcars.ToListAsync();
            List<Bookings> allBookings = await _context.booking.ToListAsync();

            List<HotelCars> availableCars = allCars
                .Where(car =>
                    (!carSize.HasValue || car.Size == carSize.Value) &&
                    !allBookings.Any(booking =>
                        booking.CarId == car.Id &&
                        ((booking.StartDate < endDate && booking.EndDate > startDate) ||
                         (startDate < booking.EndDate && endDate > booking.StartDate))
                    )
                )
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize) 
                .ToList();

            return Ok(availableCars);
        }
        
        [HttpDelete("delete")]
        public async Task<ActionResult<Bookings>> DeleteCar(int carId)
        {
            HotelCars carToDelete = await _context.hotelcars.FindAsync(carId);

            if (carToDelete == null)
            {
                return NotFound("Car not found");
            }

            _context.hotelcars.Remove(await _context.hotelcars.FindAsync(carId));
            await _context.SaveChangesAsync();
            return Ok(carToDelete);
        }
    }
}