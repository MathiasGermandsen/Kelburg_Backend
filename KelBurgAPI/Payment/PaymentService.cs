using KelBurgAPI.Controllers;
using Stripe.Checkout;
using KelBurgAPI.Models;
using Stripe;

namespace KelBurgAPI.Payment;

public class PaymentService : IPaymentService
{
    private IConfiguration _configuration;
    public PaymentService(IConfiguration configuration)
    {
        _configuration = configuration;
        StripeConfiguration.ApiKey = SecretHelper.GetSecretValue(_configuration["SecretApiKey"]);
    }
    
    public Session CreateCheckoutSession(Bookings booking, Rooms room, ServicePaymentDTO service, HotelCars car)
    {
        string description = $"Room: {room.RoomType} | Room View: {room.ViewType} | {booking.PeopleCount} Occupants" +
                             (car != null ? $" | Car: {car.Manufacturer} {car.Model} - Size: {car.Size}" : "") +
                             $" | Service: {service.PrettyName}";
        
        List<SessionLineItemOptions> lineItems = new List<SessionLineItemOptions>();
        lineItems.Add(new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                UnitAmount = booking.BookingPrice * 100,
                Currency = "dkk",
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = $"Booking at Kelburg Hotel | {booking.StartDate:yyyy-MM-dd} - {booking.EndDate:yyyy-MM-dd}",
                    Description = description,
                }
            },
            Quantity = 1,
        });

        SessionCreateOptions options = new SessionCreateOptions
        {
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = "http://10.133.51.102:13000/bookingSuccess",
            CancelUrl = "http://10.133.51.102:13000/rooms",
        };

        SessionService sessionService = new SessionService();
        Session session = sessionService.Create(options);
        return session;
    }
}
