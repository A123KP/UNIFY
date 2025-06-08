// UNIFY.Services/ProductService.cs
using UNIFY.Data;
using UNIFY.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.IO;
using Serilog;
using System.Threading.Tasks;
using System.Collections.Generic; // Added for IEnumerable

namespace UNIFY.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductService(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(string searchTerm, int pageNumber, int pageSize)
        {
            // Start with all products and include their categories
            IQueryable<Product> productsQuery = _context.Products.Include(p => p.Category);

            // Apply search filter if a searchTerm is provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string lowerSearchTerm = searchTerm.ToLower();
                productsQuery = productsQuery.Where(p =>
                    p.Name.ToLower().Contains(lowerSearchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(lowerSearchTerm)) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(lowerSearchTerm))
                );
            }

            // Get the total count of products AFTER applying the search filter but BEFORE pagination
            int totalProducts = await productsQuery.CountAsync();

            // Apply pagination
            var products = await productsQuery
                                .OrderBy(p => p.ProductId) // Order by ProductId for consistent pagination
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            return (products, totalProducts);
        }

        public async Task<int> GetTotalProductCountAsync()
        {
            try
            {
                return await _context.Products.CountAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting total product count.");
                return 0;
            }
        }

        public async Task<bool> AddProductAsync(Product product, IFormFile? productImage, string webRootPath)
        {
            try
            {
                _context.Add(product);
                await _context.SaveChangesAsync(); // Save to get ProductId for image naming

                if (productImage != null && productImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(webRootPath, "images", "ProductCatalog");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Ensure unique filename based on ProductId
                    string fileName = "Product" + product.ProductId + Path.GetExtension(productImage.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await productImage.CopyToAsync(fileStream);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding product to database or saving image.");
                return false;
            }
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            try
            {
                return await _context.Products
                                     .Include(p => p.Category)
                                     .FirstOrDefaultAsync(p => p.ProductId == productId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting product by ID: {ProductId}", productId);
                return null;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product, IFormFile? productImage, string webRootPath, bool deleteExistingImage)
        {
            try
            {
                var existingProduct = await _context.Products.FindAsync(product.ProductId);
                if (existingProduct == null)
                {
                    return false; // Product not found
                }

                // Update scalar properties from the incoming product model
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.CategoryId = product.CategoryId;

                // Handle image update/deletion
                string uploadsFolder = Path.Combine(webRootPath, "images", "ProductCatalog");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Find existing image file(s) for this product (e.g., Product123.jpg, Product123.png)
                // Use a pattern to find any extension
                string currentImagePattern = "Product" + product.ProductId + ".*";
                string[] existingImageFiles = Directory.GetFiles(uploadsFolder, currentImagePattern);

                if (productImage != null && productImage.Length > 0)
                {
                    // Delete old image files if they exist (replacing with new image)
                    foreach (var file in existingImageFiles)
                    {
                        try { System.IO.File.Delete(file); } catch (Exception ex) { Log.Warning(ex, "Could not delete old image file: {File}", file); }
                    }

                    // Save new image
                    string newFileName = "Product" + product.ProductId + Path.GetExtension(productImage.FileName);
                    string newFilePath = Path.Combine(uploadsFolder, newFileName);

                    using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await productImage.CopyToAsync(fileStream);
                    }
                }
                // If deleteExistingImage is true and no new image is uploaded, delete existing images
                else if (deleteExistingImage && existingImageFiles.Any())
                {
                    foreach (var file in existingImageFiles)
                    {
                        try { System.IO.File.Delete(file); } catch (Exception ex) { Log.Warning(ex, "Could not delete existing image file flagged for deletion: {File}", file); }
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating product with ID: {ProductId}", product.ProductId);
                return false;
            }
        }


        public async Task<bool> DeleteProductAsync(int productId)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return false;
                }

                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "ProductCatalog");
                if (Directory.Exists(uploadsFolder))
                {
                    // Delete any image file associated with this product ID, regardless of extension
                    string filePattern = "Product" + productId + ".*";
                    string[] existingImageFiles = Directory.GetFiles(uploadsFolder, filePattern);
                    foreach (var file in existingImageFiles)
                    {
                        try
                        {
                            System.IO.File.Delete(file);
                        }
                        catch (Exception fileEx)
                        {
                            Log.Warning(fileEx, "Could not delete image file {FilePath} for product {ProductId}.", file, productId);
                            // Log a warning, but don't prevent product deletion if image deletion fails.
                        }
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync(); // Save changes to delete product from DB
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting product with ID: {ProductId}", productId);
                return false;
            }
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            try
            {
                return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all categories.");
                return Enumerable.Empty<Category>();
            }
        }

        public async Task<Category?> AddCategoryAsync(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return null;
            }

            if (await CategoryExistsAsync(categoryName))
            {
                return null; // Category already exists
            }

            try
            {
                var newCategory = new Category { Name = categoryName };
                _context.Add(newCategory);
                await _context.SaveChangesAsync();
                return newCategory;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding category: {CategoryName}", categoryName);
                return null;
            }
        }

        public async Task<bool> CategoryExistsAsync(string categoryName)
        {
            return await _context.Categories.AnyAsync(c => c.Name.ToLower() == categoryName.ToLower());
        }        
    }
}