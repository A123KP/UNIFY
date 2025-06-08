// UNIFY.Services/HomeService.cs
using UNIFY.Data;
using UNIFY.Models;
using Microsoft.EntityFrameworkCore; // For .Include() and async methods like ToListAsync()
using System.Linq; // For LINQ methods
using Serilog; // For logging
using System.Threading.Tasks; // Required for async methods


namespace UNIFY.Services
{
    public class HomeService : IHomeService
    {
        private readonly AppDbContext _context;

        public HomeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetFlashSaleProductsAsync()
        {
            try
            {
                return await _context.Products
                                     .Include(p => p.Category)
                                     .Take(6)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting flash sale products for home view.");
                return Enumerable.Empty<Product>();
            }
        }

        public async Task<IEnumerable<Product>> GetExploreProductsAsync()
        {
            try
            {
                return await _context.Products
                                     .Include(p => p.Category)                                     
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting explore products for home view.");
                return Enumerable.Empty<Product>();
            }
        }
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            try
            {
                return await _context.Products
                                     .Include(p => p.Category) // Include category if you display it on AllProducts page
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all products.");
                return Enumerable.Empty<Product>();
            }
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            try
            {
                return await _context.Categories
                                     .OrderBy(c => c.Name)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all categories for home view.");
                return Enumerable.Empty<Category>();
            }
        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            // Start with all products and eagerly load Category for efficient querying
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            // Check if the search term is not empty or just whitespace
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string lowerSearchTerm = searchTerm.ToLower(); // Convert to lowercase once for case-insensitive comparison

                query = query.Where(p =>
                    p.Name.ToLower().Contains(lowerSearchTerm) || // Search by Product Name
                    (p.Description != null && p.Description.ToLower().Contains(lowerSearchTerm)) || // Search by Product Description (null-safe)
                    (p.Category != null && p.Category.Name.ToLower().Contains(lowerSearchTerm)) // Search by Category Name (null-safe)
                );
            }
            else
            {
                return new List<Product>();
            }

            return await query.ToListAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                                 .Include(p => p.Category) // Include Category for display in the view if necessary
                                 .Where(p => p.CategoryId == categoryId)
                                 .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            try
            {
                return await _context.Products
                                     .Include(p => p.Category) // Include Category if you want to display its name
                                     .FirstOrDefaultAsync(p => p.ProductId == productId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting product with ID {ProductId}.", productId);
                return null; // Return null if an error occurs or product not found
            }
        }
        
    }
}