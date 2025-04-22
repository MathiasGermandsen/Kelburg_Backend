using KelBurgAPI.Data;
using KelBurgAPI.Models;
using KelBurgAPI.Payment;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Microsoft.EntityFrameworkCore;

namespace KelBurgAPI.Controllers;

[Route("api/[controller]")]
[ApiController]

public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly DatabaseContext _context;
    
    public PaymentController(IPaymentService paymentService, DatabaseContext context)
    {
       _paymentService = paymentService;
       _context = context;
    }

    [HttpPost("checkout")]
    public ActionResult CreateCheckoutSession(Bookings booking)
    {
        Rooms selectedRoom = _context.rooms.Find(booking.RoomId);
        Services selectedService = _context.services.Find(booking.ServiceId);
        ServicePaymentDTO mappedService = new ServicePaymentDTO();

        mappedService = new ServicePaymentDTO()
        {
            AllInclusive = selectedService.AllInclusive,
            Breakfast = selectedService.Breakfast,
            Dinner = selectedService.Dinner,
            BreakfastAndDinner = selectedService.BreakfastAndDinner,
            PricePrPersonPrNight = selectedService.PricePrPersonPrNight,
        };
    
        mappedService.PrettyName = mappedService.AllInclusive 
            ? "All Inclusive" : mappedService.Breakfast 
                ? "Breakfast" : mappedService.Dinner 
                    ? "Dinner" : mappedService.BreakfastAndDinner 
                        ? "Breakfast and Dinner" : "No Service";
            
        HotelCars car = _context.hotelcars.Find(booking.CarId);
        
        Session? session = _paymentService.CreateCheckoutSession(booking, selectedRoom, mappedService, car);
        return Ok(session.Url);
    }
}