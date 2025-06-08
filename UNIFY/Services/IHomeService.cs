// UNIFY.Services/IHomeService.cs
using UNIFY.Models;
using System.Collections.Generic; // Make sure this is included for List<Product>
using System.Threading.Tasks;

namespace UNIFY.Services
{
    public interface IHomeService
    {
        Task<IEnumerable<Product>> GetFlashSaleProductsAsync();
        Task<IEnumerable<Product>> GetExploreProductsAsync();
        Task<IEnumerable<Product>> GetAllProductsAsync(); // NEW LINE: Add this declaration

        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        // CHANGE THIS LINE: Return type should be Task<List<Product>>
        Task<List<Product>> SearchProductsAsync(string searchTerm); // Corrected line
        Task<List<Product>> GetProductsByCategoryIdAsync(int categoryId);
        Task<Category?> GetCategoryByIdAsync(int categoryId);
        Task<Product?> GetProductByIdAsync(int productId);
    }
}