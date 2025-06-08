// Service/IPaymentService.cs
using System.Threading.Tasks;
using UNIFY.Models;

namespace UNIFY.Services
{
    public interface IPaymentService
    {
        // Fetches order details to get the total amount
        Task<Order> GetLatestOrderForUserAsync(int userId);

        // Records a new payment in the database
        Task<Payment> RecordPaymentAsync(int userId, PaymentStatus status);
    }
}