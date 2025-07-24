using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models
{
    public class ShippingAddress
    {
        public int Id { get; set; }                   
        public int UserId { get; set; }                
        public string AddressType { get; set; }        
        public string Street { get; set; }
        public string Number { get; set; }
        public string Floor { get; set; }
        public string Apartment { get; set; }
        public string Tower { get; set; }
        public string BetweenStreets { get; set; }
        public string PostalCode { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string Observations { get; set; }

        // Persona autorizada
        public string AuthorizedPersonFirstName { get; set; }
        public string AuthorizedPersonLastName { get; set; }
        public string AuthorizedPersonPhone { get; set; }
        public string AuthorizedPersonDni { get; set; }

        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User User { get; set; }
    }
}
