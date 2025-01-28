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
    
    [HttpPost("createPredefined")]
    public async Task<ActionResult<ServicePricesDict>> CreatePredefinedServicePrices()
    {
        ServicePricesDict breakfastPrice = new ServicePricesDict()
        {
            Type = "breakfast",
            PricePrPersonPrNight = 100
        };
        ServicePricesDict allInclusivePrice = new ServicePricesDict()
        {
            Type = "allinclusive",
            PricePrPersonPrNight = 200
        };
        
        List<ServicePricesDict> servicePricesDict = new List<ServicePricesDict>();
        servicePricesDict.Add(breakfastPrice);
        servicePricesDict.Add(allInclusivePrice);
        
        _context.ServicePricesDict.AddRange(servicePricesDict);
        await _context.SaveChangesAsync();
        return Ok(servicePricesDict);
    }
    
    [HttpGet("read")]
    public async Task<ActionResult<IReadOnlyList<ServicePricesDict>>> GetServicePrices()
    {
        List<ServicePricesDict> allServicePricesList = new List<ServicePricesDict>();
        
        allServicePricesList = await _context.ServicePricesDict.ToListAsync();
        
        return Ok(allServicePricesList);
    }

    [HttpPatch("updatePrice")]
    public async Task<ActionResult<ServicePricesDict>> ChangeServicePrice(string type, int NewPricePrPersonPrNight)
    {
        ServicePricesDict serviceToUpdate = await _context.ServicePricesDict.FirstOrDefaultAsync(s => s.Type == type);
        serviceToUpdate.PricePrPersonPrNight = NewPricePrPersonPrNight;
        _context.ServicePricesDict.Update(serviceToUpdate);
        await _context.SaveChangesAsync();
        return Ok(serviceToUpdate);
    }
    
}