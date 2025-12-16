namespace HardwareStore.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }                  
        public string Email { get; set; }
        public string? PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; } = "Customer";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<ShippingAddress> ShippingAddresses { get; set; } = new List<ShippingAddress>();

        // Google account
        public bool IsGoogleUser { get; set; } = false;
        public string? GoogleId { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
