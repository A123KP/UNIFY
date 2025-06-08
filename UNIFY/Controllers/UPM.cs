using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNIFY.Auth;
using UNIFY.Models;
using UNIFY.Services;

namespace UNIFY.Controllers
{
    public class UPM : Controller
    {
        private readonly IUserService _userService; //

        public UPM(IUserService userService)
        {
            _userService = userService;
        }
        [AllowAnonymous] // Allows unauthenticated access to these actions
        public IActionResult Register()
        {
            ViewBag.ShowPublicNavbar = true;
            return View();
        }
        [AllowAnonymous]// Allows unauthenticated access to these actions
        public IActionResult Login()
        {
            ViewBag.ShowPublicNavbar = true;
            return View();
        }

        [AuthorizeRole("ADMIN","CUSTOMER")] // STORING AS ENUM IN DB BUT IN SESSION STROING AS STRING
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // ✅ Clears all session data
            TempData["SuccessMessage"] = "You have been logged out.";
            return RedirectToAction("Login");
        }


        // POST: Login
        [AllowAnonymous] // Allows unauthenticated access to this action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoginUser(string LoginEmail, string LoginPassword)
        {
            if (string.IsNullOrWhiteSpace(LoginEmail) || string.IsNullOrWhiteSpace(LoginPassword))
            {
                TempData["ErrorMessage"] = "All fields are required.";
                return View("Login");
            }

            var user = _userService.GetUserByEmailOrUsername(LoginEmail);
            if (user == null || !_userService.VerifyPassword(LoginPassword, user.Password))
            {
                TempData["ErrorMessage"] = "Invalid credentials.";
                return View("Login");
            }

            //roles cannot be null soo no need to check roles present as enum
            if (!Enum.IsDefined(typeof(UserRole), user.Role)) //check if the role is valid 
            {
                TempData["ErrorMessage"] = "Invalid user role.";
                return View("Login");
            }

            _userService.StartSession(HttpContext, user);

            if (user.Role == UserRole.ADMIN)
            {
                TempData["SuccessMessage"] = "Welcome, Admin!";  //TempData survives, but ViewBag does not after form submission redirect
                return RedirectToAction("Product", "Product"); //to return use viewbag url doesnt change

            }

            TempData["SuccessMessage"] = "Login successful.";
            return RedirectToAction("Index", "Home");
        }

        // POST: Register
        [AllowAnonymous] // Allows unauthenticated access to this action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterUser([Bind("Username,Email,Password")] User user, string ConfirmPassword)
        {
            try
            {
                _userService.RegisterUser(user, ConfirmPassword);
                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Register");
            }
        }
        //profile
        [AuthorizeRole("ADMIN", "CUSTOMER")] // Only allow authenticated users with ADMIN or CUSTOMER roles
        [HttpGet]
        public IActionResult Profile()
        {
            int? userId = _userService.GetUserIdFromSession(HttpContext);
            if (userId == null)
                return RedirectToAction("Login");
            var user = _userService.GetUserById(userId.Value);
            ViewBag.UserId = userId.Value; //used to conditionally render name address
            ViewBag.Email = user.Email ?? ""; //used to fetch details of email
            return View();
        }
        //profile
        [AuthorizeRole("ADMIN", "CUSTOMER")] // Only allow authenticated users with ADMIN or CUSTOMER roles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(string Email)
        {
            var userId = _userService.GetUserIdFromSession(HttpContext);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Session expired. Please login again.";
                return RedirectToAction("Login");
            }

            try
            {
                _userService.UpdateUserEmail(userId.Value, Email);
                TempData["SuccessMessage"] = "Email updated successfully!";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Profile");
        }
    }
}