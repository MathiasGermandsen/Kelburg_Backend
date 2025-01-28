using Microsoft.AspNetCore.Mvc;
using KelBurgAPI.Models;
using KelBurgAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace KelBurgAPI.Controllers;

[Route("api/[controller]")]
[ApiController]

public class ServicesController : ControllerBase
{
    private readonly DatabaseContext _context;
    
    public ServicesController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpPost("createPredefinedServices")]
    public async Task<ActionResult<List<Services>>> CreatePredefinedServices()
    {
        ServiceCreateDTO AllInclusiveService = new ServiceCreateDTO()
        {
            AllInclusive = true,
            Breakfast = false,
            Dinner = false,
            BreakfastAndDinner = false,
            PricePrPersonPrNight = 150
        };
        
        ServiceCreateDTO BreakfastService = new ServiceCreateDTO()
        {
            AllInclusive = false,
            Breakfast = true,
            Dinner = false,
            BreakfastAndDinner = false,
            PricePrPersonPrNight = 50
        };
        
        ServiceCreateDTO DinnerService = new ServiceCreateDTO()
        {
            AllInclusive = false,
            Breakfast = false,
            Dinner = true,
            BreakfastAndDinner = false,
            PricePrPersonPrNight = 80
        };
        
        ServiceCreateDTO BreakfastAndDinnerService = new ServiceCreateDTO()
        {
            AllInclusive = false,
            Breakfast = false,
            Dinner = false,
            BreakfastAndDinner = true,
            PricePrPersonPrNight = 130
        };
        List<ServiceCreateDTO> servicesDTOs = new List<ServiceCreateDTO>();
        servicesDTOs.Add(AllInclusiveService);
        servicesDTOs.Add(BreakfastService);
        servicesDTOs.Add(DinnerService);
        servicesDTOs.Add(BreakfastAndDinnerService);        
        
        List<Services> services = new List<Services>();

        foreach (ServiceCreateDTO service in servicesDTOs)
        {
            Services newService = new Services()
            {
                AllInclusive = service.AllInclusive,
                Breakfast = service.Breakfast,
                Dinner = service.Dinner,
                BreakfastAndDinner = service.BreakfastAndDinner,
                PricePrPersonPrNight = service.PricePrPersonPrNight,
            };
            services.Add(newService);
        }
        
        _context.Services.AddRange(services);
        await _context.SaveChangesAsync();
        return services;
    }
}