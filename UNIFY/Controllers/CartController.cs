// controller/Cart.cs
using Microsoft.AspNetCore.Mvc;
using UNIFY.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using UNIFY.Models; // For UNIFY.Models.Cart
using System.Linq;
using Microsoft.AspNetCore.Http; // For ISession
using Microsoft.Extensions.Logging; // For ILogger
using Serilog;
using Microsoft.AspNetCore.Authorization;
using UNIFY.Auth; // For Serilog

namespace UNIFY.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService; // Kept for potential future use
        private readonly IOrderService _orderService; // <--- NEW: Inject IOrderService
        private readonly ILogger<CartController> _logger; // Added for logging

        public CartController(ICartService cartService, IProductService productService, IOrderService orderService, ILogger<CartController> logger) // <--- NEW: Add IOrderService to constructor
        {
            _cartService = cartService;
            _productService = productService;
            _orderService = orderService; // <--- NEW: Assign injected service
            _logger = logger;
        }
        [AllowAnonymous] // Allows unauthenticated access to this action
        public IActionResult Cart()
        {
            return View(); // The Cart.cshtml view from the Canvas
        }

        // Only allow CUSTOMER role to access this action
        [HttpPost]
        // [ValidateAntiForgeryToken] // Consider adding this back if you use the token in AJAX
        public async Task<IActionResult> ProcessCheckout([FromBody] List<Cart> cartItemsFromClient)
        {
            _logger.LogInformation("ProcessCheckout action started.");

            if (cartItemsFromClient == null || !cartItemsFromClient.Any())
            {
                _logger.LogWarning("ProcessCheckout: Cart is empty or null.");
                return Json(new { success = false, message = "Cart is empty." });
            }

            // --- User ID Retrieval ---
            var userIdFromSession = HttpContext.Session.GetInt32("UserId");
            if (!userIdFromSession.HasValue)
            {
                if (HttpContext.Session.GetString("Username") == null)
                {
                    _logger.LogWarning("ProcessCheckout: User not logged in.");
                    TempData["ErrorMessage"] = "You need to be logged in to proceed to checkout.";
                    return Json(new { success = false, message = "User not logged in.", redirectUrl = Url.Action("Login", "User") });
                }
                _logger.LogError("ProcessCheckout: Username session exists but UserId session is missing.");
                return Json(new { success = false, message = "Session error. Please log in again.", redirectUrl = Url.Action("Login", "User") });
            }

            int currentUserId = userIdFromSession.Value;
            _logger.LogInformation($"ProcessCheckout: Processing for UserId: {currentUserId}");

            // Prepare the list to be sent to the service (set UserId, clear nav props etc.)
            foreach (var item in cartItemsFromClient)
            {
                item.UserId = currentUserId;
                item.CartId = 0;
                item.User = null;
                item.Product = null;
                _logger.LogInformation($"ProcessCheckout: Item to process - ProductId: {item.ProductId}, Quantity: {item.Quantity}, UserId set to: {item.UserId}");
            }

            try
           {
                await _cartService.SaveCartAsync(cartItemsFromClient); // Pass the list of Cart entities

                _logger.LogInformation($"ProcessCheckout: Cart saved successfully for UserId: {currentUserId}.");

                var orderId = await _orderService.CreateOrderAsync(currentUserId);
                _logger.LogInformation($"ProcessCheckout: Order created successfully with OrderId: {orderId}.");

                string redirectUrl = Url.Action("Payment", "Payment"); // Ensure PaymentController and Payment action exist
                _logger.LogInformation($"ProcessCheckout: Redirecting to {redirectUrl}");

                return Json(new { success = true, message = "Cart processed successfully! Go to Payment", redirectUrl = redirectUrl });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"ProcessCheckout: Error occurred during checkout process for UserId: {currentUserId}.");
                return Json(new { success = false, message = "An error occurred during checkout: " + ex.Message });
            }
        }
    }
}