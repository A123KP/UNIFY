using System;

using System.Linq;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using UNIFY.Data;

using UNIFY.Models;

namespace UNIFY.Services

{

    public class OrderService : IOrderService

    {

        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)

        {

            _context = context;

        }

        public async Task<int> CreateOrderAsync(int? userId)

        {

            var cartItems = await _context.Carts

                                          .Where(c => c.UserId == userId)

                                          .ToListAsync();

            if (cartItems == null || !cartItems.Any())

                throw new Exception("Cart is empty");

            var productIds = cartItems.Select(c => c.ProductId).ToList();

            var products = await _context.Products

                                         .Where(p => productIds.Contains(p.ProductId))

                                         .ToDictionaryAsync(p => p.ProductId);

            decimal total = 0;

            foreach (var item in cartItems)

            {

                if (!products.TryGetValue(item.ProductId, out var product))

                    throw new Exception($"Product with ID {item.ProductId} not found");

                total += product.Price * item.Quantity;

            }

            var order = new Order

            {

                UserId = (int)userId,

                TotalAmount = total,

                OrderDate = DateTime.Now,

                Status = OrderStatus.PENDING

            };

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            foreach (var item in cartItems)

            {

                _context.OrderItems.Add(new OrderItem

                {

                    OrderId = order.OrderId,

                    ProductId = item.ProductId,

                    Quantity = item.Quantity,

                    Price = products[item.ProductId].Price

                });

            }

            await _context.SaveChangesAsync();

            return order.OrderId;

        }

        public async Task UpdateOrderStatusAsync(int orderId, string status, PaymentStatus? paymentStatus = null)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new Exception("Order not found");
            if (paymentStatus.HasValue)
            {
                // Update order status based on payment status
                switch (paymentStatus.Value)
                {
                    case PaymentStatus.COMPLETED:
                        order.Status = OrderStatus.SHIPPED; // keep as PENDING until shipped
                        break;
                    case PaymentStatus.FAILED:
                        order.Status = OrderStatus.CANCELLED;
                        break;
                    case PaymentStatus.PENDING:
                    default:
                        order.Status = OrderStatus.PENDING;
                        break;
                }
            }
            else
            {
                // Just update order status from direct input (like SHIPPED, DELIVERED, etc.)
                if (Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
                {
                    order.Status = parsedStatus;
                }
                else
                {
                    throw new Exception("Invalid order status value");
                }
            }
            await _context.SaveChangesAsync();
            // Clear cart only if status is SHIPPED or CANCELLED
            if (order.Status == OrderStatus.SHIPPED || order.Status == OrderStatus.CANCELLED)
            {
                var userId = order.UserId;
                var cartItems = _context.Carts.Where(c => c.UserId == userId);
                _context.Carts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<OrderDetailsDto> GetOrderDetailsAsync(int? userId) //take user id to fetch order details of latest user
        {
            var order = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .FirstOrDefaultAsync(); // ✅ Fetch latest order dynamically

            if (order == null)
                throw new Exception($"No recent order found for UserId: {userId}");

            var items = await _context.OrderItems
                .Where(oi => oi.OrderId == order.OrderId)
                .Include(oi => oi.Product)
                .ToListAsync();

            return new OrderDetailsDto
            {
                Order = order,
                Items = items
            };
        }

    }

}
