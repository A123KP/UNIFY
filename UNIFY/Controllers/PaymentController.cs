// controller/Payment.cs
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UNIFY.Models;
using UNIFY.Services; // Ensure this is included
using Microsoft.Extensions.Logging; // For logging
using Serilog;
using UNIFY.Auth; // For Serilog

namespace UNIFY.Controllers
{
    [AuthorizeRole("CUSTOMER")] // Only allow CUSTOMER role to access these actions
    public class PaymentController : Controller

    {

        private readonly IPaymentService _paymentService;

        private readonly IOrderService _orderService;

        
        public async Task<IActionResult> Payment()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var latestOrder = await _paymentService.GetLatestOrderForUserAsync(userId.Value);
            if (latestOrder == null)
            {
                return View("Error", "No recent order found.");
            }

            return View(latestOrder); // Pass order to view
        }


        public PaymentController(IPaymentService paymentService, IOrderService orderService)

        {

            _paymentService = paymentService;

            _orderService = orderService;

        }


        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || string.IsNullOrEmpty(request.PaymentMethod))
            {
                return Json(new { success = false, message = "Invalid request" });
            }
            var method = request.PaymentMethod.ToLower();
            var status = method switch
            {
                "cod" => PaymentStatus.PENDING,
                "card" or "upi" => PaymentStatus.COMPLETED,
                "cancelled" => PaymentStatus.FAILED
            };

            var payment = await _paymentService.RecordPaymentAsync(userId.Value, status);

            if (payment == null)
            {
                return Json(new { success = false, message = "Failed to record payment" });
            }

            await _orderService.UpdateOrderStatusAsync(payment.OrderId, null, status);

            return Json(new
            {
                success = true,
                message = method == "cancelled" ? "Payment was cancelled." : "Payment processed successfully",
                redirectUrl = Url.Action("Latest", "Order")
            });
        }


    }
}
 