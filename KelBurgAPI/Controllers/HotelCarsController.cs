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
            _context.HotelCars.Add(carsToBeCreated);
            await _context.SaveChangesAsync();  
            return carsToBeCreated;
        } 
        
        [HttpGet("read")]
        public async Task<ActionResult<IEnumerable<HotelCars>>> GetCars(int? carId, int pageSize = 100, int pageNumber = 1)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("PageNumber and size must be greater than 0");
            }
        
            IQueryable<HotelCars> query = _context.HotelCars.AsQueryable();

            if (carId.HasValue)
            {
                query = query.Where(c => c.Id == carId);
            }
           
            List<HotelCars> hotelCars = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(hotelCars);
        }
        
        [HttpDelete("delete")]
        public async Task<ActionResult<Rooms>> DeleteCar(int carId)
        {
            HotelCars car = await _context.HotelCars.FindAsync(carId);
        
            if (car == null)
            {
                return NotFound("Car not found");
            }
        
            _context.HotelCars.Remove(car);
            await _context.SaveChangesAsync();
            return Ok(car);
        }
    }
}