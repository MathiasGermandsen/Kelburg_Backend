using KelBurgAPI.Data;
using KelBurgAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KelBurgAPI.Controllers;

[Route("api/[controller]")]
[ApiController]

public class ServicePricesDictController : ControllerBase
{
    private readonly DatabaseContext _context;

    public ServicePricesDictController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpPost("create")]
    public async Task<ActionResult<ServicePricesDict>> CreateTicket([FromQuery] ServicePricesDictCreateDTO servicePrices)
    {
        if (servicePrices == null)
        {
            return BadRequest("Service Prices is null");
        }
        
        ServicePricesDict newServicePrice = new ServicePricesDict()
        {
            Type = servicePrices.Type,
            PricePrPersonPrNight = servicePrices.PricePrPersonPrNight,
        };
        
        _context.ServicePricesDict.Add(newServicePrice);
        await _context.SaveChangesAsync();
        return Ok(newServicePrice);
    }
    
    [HttpGet("read")]
    public async Task<ActionResult<IReadOnlyList<ServicePricesDict>>> GetServicePrices()
    {
        List<ServicePricesDict> allServicePricesList = new List<ServicePricesDict>();
        
        allServicePricesList = await _context.ServicePricesDict.ToListAsync();
        
        return Ok(allServicePricesList);
    }
}