using UNIFY.Data;
using UNIFY.Models;
using Microsoft.EntityFrameworkCore;
namespace UNIFY.Services
{
    public class AdminCustomerService : IAdminCustomerService
    {
        private readonly AppDbContext _context;

        public AdminCustomerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllOrdersWithUsersAsync()
        {
            return await _context.Orders
                .Include(o => o.User) // Ensures User.Username is available
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersForUserAsync(int userId)
        {
            return await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
        }


        public async Task UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) throw new Exception("Order not found");

            if (Enum.TryParse<OrderStatus>(newStatus, true, out var parsedStatus))
            {
                order.Status = parsedStatus;
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Invalid status value");
            }
        }
    }

}
