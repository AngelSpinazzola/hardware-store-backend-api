using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
﻿namespace HardwareStore.Infrastructure.Identity
{
    public class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
