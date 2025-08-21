namespace EcommerceAPI.DTOs.Customers
{
    public class ShippingAddressDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string AddressType { get; set; } // "Casa" o "Trabajo"
        public string Street { get; set; }
        public string Number { get; set; }
        public string? Floor { get; set; }
        public string? Apartment { get; set; }
        public string? Tower { get; set; }
        public string? BetweenStreets { get; set; }
        public string PostalCode { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string? Observations { get; set; }

        // Datos de quien recibe el pedido
        public string AuthorizedPersonFirstName { get; set; }
        public string AuthorizedPersonLastName { get; set; }
        public string AuthorizedPersonPhone { get; set; }
        public string AuthorizedPersonDni { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
