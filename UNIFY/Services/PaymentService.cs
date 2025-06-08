using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using UNIFY.Data;
using UNIFY.Models;
using Serilog;

namespace UNIFY.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the latest order for a given user instead of relying on `orderId` from the URL.
        /// </summary>
        public async Task<Order> GetLatestOrderForUserAsync(int userId)
        {
            try
            {
                // Fetch the most recent order for the user
                return await _context.Orders
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.OrderDate) // Get latest order
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching latest order for UserId: {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Records a payment in the database for the latest order of the user.
        /// </summary>
        public async Task<Payment> RecordPaymentAsync(int userId, PaymentStatus status)
        {
            try
            {
                var order = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.OrderDate)
                    .FirstOrDefaultAsync();
                if (order == null)
                {
                    Log.Warning("No recent order found for UserId: {UserId}", userId);
                    return null;
                }
                if (order.TotalAmount == null)
                    throw new InvalidOperationException("Order total amount is null");

                var newPayment = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = order.TotalAmount.Value,
                    PaymentStatus = status,
                    PaymentDate = DateTime.UtcNow
                };
                _context.Payments.Add(newPayment);
                await _context.SaveChangesAsync();
                Log.Information("Payment recorded for OrderId: {OrderId}", order.OrderId);
                return newPayment;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error recording payment for UserId: {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Updates order status based on payment result.
        /// </summa
    }
}
