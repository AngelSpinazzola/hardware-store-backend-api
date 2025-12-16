using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
﻿using System.Net.Mail;
using System.Text.RegularExpressions;

namespace HardwareStore.Infrastructure.Identity
{
    public static class SecurityHelper
    {
        // Sanitiza entrada de usuario removiendo caracteres peligrosos
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remueve caracteres peligrosos pero mantener caracteres útiles para nombres
            var sanitized = Regex.Replace(input.Trim(), @"[<>""'&\\\/]", "");

            // Remueve múltiples espacios consecutivos
            sanitized = Regex.Replace(sanitized, @"\s+", " ");

            return sanitized.Trim();
        }

        // Sanitiza términos de búsqueda removiendo caracteres especiales
        public static string SanitizeSearchTerm(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return string.Empty;

            // Para búsquedas, permitir solo letras, números, espacios y algunos caracteres seguros
            var sanitized = Regex.Replace(searchTerm, @"[^\w\s\-\.]", "");

            // Remover múltiples espacios
            sanitized = Regex.Replace(sanitized, @"\s+", " ");

            return sanitized.Trim();
        }

        // Sanitiza nombres de categorías
        public static string SanitizeCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return string.Empty;

            // Para categorías, permitir letras, números, espacios y guiones
            var sanitized = Regex.Replace(category, @"[^\w\s\-]", "");

            return sanitized.Trim();
        }

        // Valida formato de email
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Valida y sanitiza email
        public static (bool IsValid, string SanitizedEmail) ValidateAndSanitizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, string.Empty);

            var sanitizedEmail = email.Trim().ToLowerInvariant();
            var isValid = IsValidEmail(sanitizedEmail);

            return (isValid, isValid ? sanitizedEmail : string.Empty);
        }

        // Valida fuerza de contraseña
        public static (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Contraseña requerida");

            if (password.Length < 8)
                return (false, "La contraseña debe tener al menos 8 caracteres");

            if (password.Length > 100)
                return (false, "La contraseña no puede exceder 100 caracteres");

            if (!password.Any(char.IsUpper))
                return (false, "La contraseña debe contener al menos una mayúscula");

            if (!password.Any(char.IsLower))
                return (false, "La contraseña debe contener al menos una minúscula");

            if (!password.Any(char.IsDigit))
                return (false, "La contraseña debe contener al menos un número");

            // Valida caracteres especiales
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
                return (false, "La contraseña debe contener al menos un carácter especial");

            return (true, "");
        }

        // Valida datos de registro
        public static (bool IsValid, string ErrorMessage) ValidateRegistrationData(
            string email,
            string password,
            string firstName,
            string lastName)
        {
            // Validar email
            var emailValidation = ValidateAndSanitizeEmail(email);
            if (!emailValidation.IsValid)
                return (false, "Email inválido");

            // Validar contraseña
            var passwordValidation = ValidatePassword(password);
            if (!passwordValidation.IsValid)
                return passwordValidation;

            // Validar nombres
            if (string.IsNullOrWhiteSpace(firstName) || firstName.Length < 2)
                return (false, "El nombre debe tener al menos 2 caracteres");

            if (string.IsNullOrWhiteSpace(lastName) || lastName.Length < 2)
                return (false, "El apellido debe tener al menos 2 caracteres");

            if (firstName.Length > 50 || lastName.Length > 50)
                return (false, "Los nombres no pueden exceder 50 caracteres");

            return (true, "");
        }

        // Valida parámetros de paginación
        public static (bool IsValid, string ErrorMessage) ValidatePaginationParams(int page, int pageSize, int maxPageSize = 100)
        {
            if (page < 1)
                return (false, "El número de página debe ser mayor a 0");

            if (pageSize < 1)
                return (false, "El tamaño de página debe ser mayor a 0");

            if (pageSize > maxPageSize)
                return (false, $"El tamaño de página no puede exceder {maxPageSize}");

            return (true, "");
        }

        public static bool IsValidId(int id)
        {
            return id > 0;
        }

        // Valida múltiples IDs
        public static bool AreValidIds(params int[] ids)
        {
            return ids.All(id => id > 0);
        }

        // Limita la longitud de strings para prevenir ataques
        public static string LimitStringLength(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Length <= maxLength ? input : input[..maxLength];
        }

        // Valida que un string no contenga solo espacios en blanco
        public static bool IsNotEmptyOrWhitespace(string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        // Valida rangos de precios y cantidades
        public static (bool IsValid, string ErrorMessage) ValidateProductData(
            string name = null,
            decimal? price = null,
            int? stock = null,
            string brand = null,
            string model = null)
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (name.Length < 2)
                    return (false, "El nombre debe tener al menos 2 caracteres");

                if (name.Length > 200)
                    return (false, "El nombre no puede exceder 200 caracteres");
            }

            if (price.HasValue && price <= 0)
                return (false, "El precio debe ser mayor a 0");

            if (stock.HasValue && stock < 0)
                return (false, "El stock no puede ser negativo");

            if (!string.IsNullOrEmpty(brand) && brand.Length > 100)
                return (false, "La marca no puede exceder 100 caracteres");

            if (!string.IsNullOrEmpty(model) && model.Length > 100)
                return (false, "El modelo no puede exceder 100 caracteres");

            return (true, "");
        }

        public static string SanitizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // Para nombres, permite solo letras, espacios, acentos y algunos caracteres especiales
            var sanitized = Regex.Replace(name.Trim(), @"[^a-zA-ZáéíóúñÁÉÍÓÚÑüÜ\s\-\.]", "");

            // Remueve múltiples espacios
            sanitized = Regex.Replace(sanitized, @"\s+", " ");

            return sanitized.Trim();
        }

        public static string SanitizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            // Para teléfonos, permite solo números, espacios, guiones, paréntesis y el signo +
            var sanitized = Regex.Replace(phone.Trim(), @"[^\d\s\-\(\)\+]", "");

            // Remueve múltiples espacios
            sanitized = Regex.Replace(sanitized, @"\s+", " ");

            return sanitized.Trim();
        }
    }
}
