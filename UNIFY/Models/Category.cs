using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace UNIFY.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } 

        public ICollection<Product> Products { get; set; } = new List<Product>(); // new List<Product>() to avoid NullReferenceException
    }
}
//•	Plural naming (Carts) clearly indicates that the property holds multiple items.
