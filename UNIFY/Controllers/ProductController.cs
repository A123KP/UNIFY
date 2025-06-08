// UNIFY.Controllers/ProductController.cs
using Microsoft.AspNetCore.Mvc;
using UNIFY.Models;
using UNIFY.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UNIFY.Auth; // Required for IEnumerable<Product> return type

namespace UNIFY.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IProductService productService, IWebHostEnvironment hostEnvironment)
        {
            _productService = productService;
            _hostEnvironment = hostEnvironment;
        }
        [AuthorizeRole("ADMIN")] // Only allow ADMIN and CUSTOMER roles to access these actions
        [HttpGet]
        public async Task<IActionResult> Product(string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var (products, totalCount) = await _productService.GetProductsAsync(searchTerm, pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.SearchTerm = searchTerm; // Pass search term back to view

            return View(products);
        }
        [AuthorizeRole("ADMIN")] // Only allow ADMIN role to access these actions
        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            ViewBag.Categories = await _productService.GetAllCategoriesAsync();
            return View();
        }
        [AuthorizeRole("ADMIN")] // check this and addproduct up
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product, IFormFile? productImage)
        {
            // Remove "Category" from ModelState validation as it's a navigation property and not directly bound from the form
            if (ModelState.ContainsKey("Category"))
            {
                ModelState.Remove("Category");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bool success = await _productService.AddProductAsync(product, productImage, _hostEnvironment.WebRootPath);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Product added successfully!";
                        return RedirectToAction(nameof(Product));
                    }
                    else
                    {
                        ModelState.AddModelError("", "An error occurred while saving the product or image. Please check logs.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error adding product: {ErrorMessage}", ex.Message);
                    ModelState.AddModelError("", "An error occurred while adding the product: " + ex.Message);
                }
            }

            // If ModelState is not valid or an error occurred, reload categories and return the view with the model
            ViewBag.Categories = await _productService.GetAllCategoriesAsync();
            return View(product);
        }
        [AuthorizeRole("ADMIN")] // Only allow ADMIN role to access these actions
        [HttpPost]
        public async Task<IActionResult> CreateCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return Json(new { success = false, message = "Category name cannot be empty." });
            }

            if (await _productService.CategoryExistsAsync(categoryName))
            {
                return Json(new { success = false, message = "Category Name already exists. Please use a different name." });
            }

            try
            {
                var newCategory = await _productService.AddCategoryAsync(categoryName);

                if (newCategory == null)
                {
                    return Json(new { success = false, message = "Category could not be created for an unknown reason." });
                }

                return Json(new { success = true, category = newCategory });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating category: {ErrorMessage}", ex.Message);
                return Json(new { success = false, message = "An error occurred while creating the category." });
            }
        }

        [AuthorizeRole("ADMIN")] // Only allow ADMIN role to access these actions
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Product));
            }
            ViewBag.Categories = await _productService.GetAllCategoriesAsync();
            return View(product);
        }

        [AuthorizeRole("ADMIN")] // Only allow ADMIN role to access these actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product product, IFormFile? productImage, bool deleteImageCheckbox)
        {
            // Remove "Category" from ModelState validation as it's a navigation property and not directly bound from the form
            if (ModelState.ContainsKey("Category"))
            {
                ModelState.Remove("Category");
            }

            if (!ModelState.IsValid)
            {
                // If ModelState is not valid, reload categories and return the view with the model
                ViewBag.Categories = await _productService.GetAllCategoriesAsync();
                return View(product);
            }

            try
            {
                // Pass the deleteImageCheckbox to the service layer for handling image deletion
                bool success = await _productService.UpdateProductAsync(product, productImage, _hostEnvironment.WebRootPath, deleteImageCheckbox);

                if (success)
                {
                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Product));
                }
                else
                {
                    TempData["ErrorMessage"] = "Product not found or could not be updated.";
                    ViewBag.Categories = await _productService.GetAllCategoriesAsync();
                    return View(product);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating product: {ProductId}", product.ProductId);
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                ViewBag.Categories = await _productService.GetAllCategoriesAsync();
                return View(product);
            }
        }

        [AuthorizeRole("ADMIN")] // Only allow ADMIN role to access these actions
        [HttpPost]
        [ValidateAntiForgeryToken] // Added Anti-Forgery Token for security
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                bool deleted = await _productService.DeleteProductAsync(id);

                if (deleted)
                {
                    // Return JSON for client-side success handling (e.g., SweetAlert)
                    return Json(new { success = true, message = "Product deleted successfully." });
                }
                else
                {
                    // Return JSON for client-side error handling
                    return Json(new { success = false, message = "Product not found or could not be deleted." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting product with ID {ProductId}: {ErrorMessage}", id, ex.Message);
                // Return JSON for client-side error handling
                return Json(new { success = false, message = "An error occurred while deleting the product." });
            }
        }
    }
}