namespace HardwareStore.Application.Customers
{
    public class UpdateShippingAddressDto
    {
        public string AddressType { get; set; }
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
    }
}