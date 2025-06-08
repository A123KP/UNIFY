// UNIFY.Services/IProductService.cs
using UNIFY.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UNIFY.Services
{
    public interface IProductService
    {
        // Product Listing and Pagination
        Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(string searchTerm, int pageNumber, int pageSize);


        // Product Management (Add, Edit, Delete)
        Task<bool> AddProductAsync(Product product, IFormFile? productImage, string webRootPath);
        Task<Product?> GetProductByIdAsync(int productId);
        // Updated signature to include deleteExistingImage for explicit deletion from UI
        Task<bool> UpdateProductAsync(Product product, IFormFile? productImage, string webRootPath, bool deleteExistingImage);
        Task<bool> DeleteProductAsync(int productId);

        // Category Management (for product forms)
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> AddCategoryAsync(string categoryName);
        Task<bool> CategoryExistsAsync(string categoryName);
    }
}