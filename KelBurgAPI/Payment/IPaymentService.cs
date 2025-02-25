using Stripe.Checkout;
using KelBurgAPI.Models;
namespace KelBurgAPI.Payment;

public interface IPaymentService
{
    Session CreateCheckoutSession(Bookings Booking, Rooms Room, ServicePaymentDTO service, HotelCars car);
}