using Microsoft.EntityFrameworkCore;

using UNIFY.Data;

using UNIFY.Models;


namespace UNIFY.Services
{
    public class OrderDetailsDto
    {
        public Order Order { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
