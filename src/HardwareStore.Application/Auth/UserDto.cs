namespace HardwareStore.Application.Auth
{
    public class UserDto
    {
        public int Id { get; set; }                  
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public bool IsGoogleUser { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
