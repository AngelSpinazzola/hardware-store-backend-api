using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    public class User
    {
        public int Id { get; set; }                  
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; } = "Customer";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<ShippingAddress> ShippingAddresses { get; set; } = new List<ShippingAddress>();
    }
}
