using System;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using System.Text;
using System.Linq;

namespace BaseCode.Utils
{
    public static class PasswordHasher
    {
        private const int DegreeOfParallelism = 8;
        private const int MemorySize = 65536; // 64MB
        private const int Iterations = 4;

        public static string HashPassword(string password)
        {
            var salt = GenerateSalt();
            var hash = HashPasswordWithSalt(password, salt);
            
            // Combine salt and hash for storage
            var hashBytes = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, hashBytes, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, hashBytes, salt.Length, hash.Length);
            
            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);
            
            // Extract salt and hash
            var salt = new byte[16];
            var hash = new byte[hashBytes.Length - 16];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, 16);
            Buffer.BlockCopy(hashBytes, 16, hash, 0, hash.Length);
            
            var newHash = HashPasswordWithSalt(password, salt);
            return hash.SequenceEqual(newHash);
        }

        private static byte[] GenerateSalt()
        {
            var salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private static byte[] HashPasswordWithSalt(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = DegreeOfParallelism,
                MemorySize = MemorySize,
                Iterations = Iterations
            };

            return argon2.GetBytes(32); // 256-bit hash
        }
    }
}
