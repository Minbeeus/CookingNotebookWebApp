using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CookingNotebookWebApp.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            // Tạo salt ngẫu nhiên
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            // Hash bằng PBKDF2
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32
            );

            // Ghép salt và hash lại (để lưu vào DB)
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] stored = Convert.FromBase64String(parts[1]);

            // Hash lại mật khẩu người dùng nhập vào
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32
            );

            return hash.SequenceEqual(stored);
        }
    }
}