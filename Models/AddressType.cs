namespace EcommerceAPI.Models
{
    public static class AddressType
    {
        public const string Home = "Casa";
        public const string Office = "Oficina";
        public const string Other = "Otro";
        public static readonly string[] AllTypes = { Home, Office, Other };
        public static bool IsValidType(string type) => AllTypes.Contains(type);
    }
}