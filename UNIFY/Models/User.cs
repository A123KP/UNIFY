using System.ComponentModel.DataAnnotations;//for Key, Required, MaxLength attributes
using System.ComponentModel.DataAnnotations.Schema; //for Column attribute
using System.Collections.Generic; //for ICollection
using Microsoft.EntityFrameworkCore; //for Index attribute
namespace UNIFY.Models
{
    public enum UserRole { CUSTOMER, ADMIN } //c# by default assign 0 1
    [Index(nameof(Username), IsUnique = true)] // Index to ensure unique usernames
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } 

        [Required, MaxLength(255)]
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; } 

        public ICollection<Cart> Carts { get; set; } = new List<Cart>(); // new List<Cart>() to avoid NullReferenceException
        public ICollection<Order> Orders { get; set; } = new List<Order>(); 
    }
}
//if try to add or access items in the collection before EF loads it, you might get a NullReferenceException
//at runtime.
//Initializing the collection (e.g., = new List<Payment>()) is a best practice to avoid this
//•	Plural naming (Carts) clearly indicates that the property holds multiple items.
