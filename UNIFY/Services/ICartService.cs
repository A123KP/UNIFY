using System.Collections.Generic;
using System.Threading.Tasks;
using UNIFY.Models; // For UNIFY.Models.Cart

namespace UNIFY.Services
{
    public interface ICartService
    {
        // Method now accepts a list of the EF Core Cart model
        Task SaveCartAsync(List<Cart> cartItemsFromController);
    }
}
