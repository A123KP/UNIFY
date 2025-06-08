using UNIFY.Models;

namespace UNIFY.Services
{
    public interface IAdminCustomerService
    {
        Task<List<Order>> GetAllOrdersWithUsersAsync();
        Task UpdateOrderStatusAsync(int orderId, string newStatus);
        Task<List<Order>> GetOrdersForUserAsync(int userId);
    }



}
