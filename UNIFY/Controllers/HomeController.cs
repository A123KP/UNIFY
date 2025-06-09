// UNIFY.Controllers/HomeController.cs (UPDATED TO USE IHomeService)
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNIFY.Auth;
using UNIFY.Models;
using UNIFY.Services; // Add this using statement

namespace UNIFY.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService _homeService; // Inject IHomeService

        public HomeController(ILogger<HomeController> logger, IHomeService homeService) // Adjust constructor
        {
            _logger = logger;
            _homeService = homeService;
        }

        [AllowAnonymous] // Only allow ADMIN and CUSTOMER roles to access these actions
        public async Task<IActionResult> Index()
        {
            // Call methods from the new HomeService
            var flashSaleProducts = await _homeService.GetFlashSaleProductsAsync();
            var exploreProducts = await _homeService.GetExploreProductsAsync();
            var categories = await _homeService.GetAllCategoriesAsync();

            ViewBag.FlashSaleProducts = flashSaleProducts;
            ViewBag.ExploreProducts = exploreProducts;
            ViewBag.Categories = categories;

            return View();
        }
        public IActionResult AccessDenied()
        {
            return View(); // Return a view that shows an access denied message
        }

        [AuthorizeRole("ADMIN", "CUSTOMER")] // Only allow  access these actions
        public async Task<IActionResult> Search(string searchTerm)
        {
            // Call search method from the new HomeService
            var products = await _homeService.SearchProductsAsync(searchTerm);

            ViewData["SearchTerm"] = searchTerm;
            return View("Search", products);
        }

       
        [HttpGet]
        public async Task<IActionResult> ByCategory(int categoryId)
        {
            // Fetch products for the given category using your home service
            var products = await _homeService.GetProductsByCategoryIdAsync(categoryId);

            // Get the category name using your home service
            var category = await _homeService.GetCategoryByIdAsync(categoryId); // Assuming you add this method
            var categoryName = category?.Name ?? "Unknown Category";

            // Store the category name in ViewData to be used in the view
            ViewData["searchTerm"] = categoryName;
            ViewData["Title"] = $"Products in {categoryName}"; // Set page title

            // Reuse the Search.cshtml view to display the filtered products
            return View("~/Views/Home/Search.cshtml", products);
        }

        [AllowAnonymous]
        [HttpGet("Product/Details/{id}")] // This defines the route for your details page
        public async Task<IActionResult> Details(int id)
        {
            var product = await _homeService.GetProductByIdAsync(id);

            if (product == null)
            {
                // Product not found, you can return a 404 or redirect to an error page
                return NotFound();
            }

            ViewData["Title"] = product.Name; // Set page title
            return View(product); // Pass the product model to the view
        }

        [AuthorizeRole("ADMIN", "CUSTOMER")] // Only allow ADMIN and CUSTOMER roles to access this action
        public async Task<IActionResult> AllProducts()
        {
            var allProducts = await _homeService.GetAllProductsAsync(); // Call the new service method
            ViewBag.AllProducts = allProducts;
            ViewData["Title"] = "All Products"; // Set page title for consistency
            return View(); // Return the AllProducts.cshtml view
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() //used for error handling without caching
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}