using System.ComponentModel.DataAnnotations.Schema; //for Column attribute
using System.ComponentModel.DataAnnotations; //for Data Annotations
using System.Collections.Generic; //for ICollection
namespace UNIFY.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } 

        public ICollection<Cart> Carts { get; set; } = new List<Cart>(); // new List<Cart>() to avoid NullReferenceException
    }
}
//•	Plural naming like (Carts) clearly indicates that the property holds multiple items.