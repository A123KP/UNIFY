using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using UNIFY.Models;
using UNIFY.Data;
using UNIFY.Services; // Assuming your service interface is here
using System.Threading.Tasks;
using UNIFY.Auth;
namespace YourNamespace.Controllers
{
   
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IOrderService _orderService;
        private readonly IAdminCustomerService _adminService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public OrderController(AppDbContext context, IOrderService orderService, IHttpContextAccessor httpContextAccessor, IAdminCustomerService adminService)
        {
            _context = context;
            _orderService = orderService;
            _httpContextAccessor = httpContextAccessor;
            _adminService = adminService;
        }
        // 🌟 Existing Views
        [AuthorizeRole("CUSTOMER")]
        public IActionResult CustomerOrder()
        {
            return View();
        }
        //admin order logic 
        [AuthorizeRole("ADMIN")] // Only allow ADMIN role to access this action
        public async Task<IActionResult> AdminOrder()
        {
            var orders = await _adminService.GetAllOrdersWithUsersAsync();
            return View(orders); // View expects List<Order>
        }

        //myorder check for roles
        [AuthorizeRole("CUSTOMER")]
        public async Task<IActionResult> MyOrder()
        {
            var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");

            if (userRole == "Admin")
            {    //need to remove this after authorization is implemented
                // Optional: redirect to admin order view
                return RedirectToAction("AdminOrder");
            }

            var orders = await _adminService.GetOrdersForUserAsync(userId.Value);
            return View("MyOrder", orders);
        }




        [AuthorizeRole("ADMIN")] // Only allow ADMIN role to access this action
        [HttpPost]
        [ActionName("UpdateStatus")] // Keeps the route name the same
        public async Task<IActionResult> AdminUpdateStatus(int orderId, string status)
        {
            await _adminService.UpdateOrderStatusAsync(orderId, status);
            return Ok(new { success = true });
        }


        //customer order logic
        // 🆕 Step 1: Create order from cart
        [AuthorizeRole("CUSTOMER")] // Only allow CUSTOMER role to access this action
        public async Task<IActionResult> Create()
        {
            var userIdString = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (userIdString == null ) return RedirectToAction("Login", "Account");
            var userId = userIdString;
            var orderId = await _orderService.CreateOrderAsync(userId);
            return RedirectToAction("Latest");
        }
        // 🆕 Step 2: Show order after payment or creation
        [AuthorizeRole("CUSTOMER")] // Only allow CUSTOMER role to access this action
        public async Task<IActionResult> Latest()
        {
            var userIdString = HttpContext.Session.GetInt32("UserId");
            if (userIdString == null) return RedirectToAction("Login", "Account");

            var userId = userIdString;
            var details = await _orderService.GetOrderDetailsAsync(userId); // fetch latest order
            return View("CustomerOrder", details);
        }

        // 🆕 Step 3: Update order status (via Admin or internal system)
        //[HttpPost]
        //public async Task<IActionResult> UpdateStatus(int orderId, string status)
        //{
        //    await _orderService.UpdateOrderStatusAsync(orderId, status);
        //    return Ok();
        //}
    }
}