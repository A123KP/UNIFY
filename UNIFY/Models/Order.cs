using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UNIFY.Models
{
    public enum OrderStatus { PENDING, SHIPPED, DELIVERED, CANCELLED }

    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int UserId { get; set; }//user for database linkage and User for linq queries or data loading 
        public User User { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalAmount { get; set; }

        public DateTime? OrderDate { get; set; }

        public OrderStatus? Status { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>(); // new List<Cart>() to avoid NullReferenceException
    }
}

//•	Plural naming (Carts) clearly indicates that the property holds multiple items.
