using KelBurgAPI.Data;
using KelBurgAPI.Models;
using KelBurgAPI.Payment;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace KelBurgAPI.Controllers;

[Route("api/[controller]")]
[ApiController]

public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly DatabaseContext _context;
    private readonly LogService _logService = new LogService();
    
    public PaymentController(IPaymentService paymentService, DatabaseContext context)
    {
       _paymentService = paymentService;
       _context = context;
    }

    [HttpPost("checkout")]
    public ActionResult CreateCheckoutSession(Bookings booking)
    {
        _logService.LogMessageWithFrame("Create Checkout Initiated...");
        try
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
                ? "All Inclusive"
                : mappedService.Breakfast
                    ? "Breakfast"
                    : mappedService.Dinner
                        ? "Dinner"
                        : mappedService.BreakfastAndDinner
                            ? "Breakfast and Dinner"
                            : "No Service";

            HotelCars car = _context.hotelcars.Find(booking.CarId);

            Session? session = _paymentService.CreateCheckoutSession(booking, selectedRoom, mappedService, car);
            
            _logService.LogMessageWithFrame(JsonSerializer.Serialize(booking));
            _logService.LogMessageWithFrame(JsonSerializer.Serialize(selectedRoom));
            _logService.LogMessageWithFrame(JsonSerializer.Serialize(mappedService));
            _logService.LogMessageWithFrame(JsonSerializer.Serialize(car));
            
            _logService.LogMessageWithFrame(session.Url);
            return Ok(session.Url);
        }
        catch (Exception ex)
        {
            _logService.LogMessageWithFrame(ex.Message);
            return BadRequest();
        }
    }
}