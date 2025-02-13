using KelBurgAPI.BogusGenerators;
using KelBurgAPI.Data;
using KelBurgAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("GenCars")]

        public async Task<ActionResult<HotelCars>> GenCars([FromBody] HotelCarsDTO hotelCars)
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
    }
}