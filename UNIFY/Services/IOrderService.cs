using System.Threading.Tasks;
using UNIFY.Models;
namespace UNIFY.Services
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(int? userId);
        Task UpdateOrderStatusAsync(int orderId, string status, PaymentStatus? paymentStatus = null);
        Task<OrderDetailsDto> GetOrderDetailsAsync(int? userId);
    }
}