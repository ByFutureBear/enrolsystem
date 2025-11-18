using System.Security.Cryptography;

namespace OnlineEnrolmentSystem.Security
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;   // 128-bit
        private const int KeySize = 32;   // 256-bit
        private const int Iterations = 100_000;
        private const char Delimiter = '.';

        public static string Hash(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(KeySize);

            return string.Join(Delimiter,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(key),
                Iterations.ToString());
        }

        public static bool Verify(string hashed, string password)
        {
            var parts = hashed.Split(Delimiter);
            if (parts.Length != 3) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var key = Convert.FromBase64String(parts[1]);
            var iterations = int.Parse(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var keyToCheck = pbkdf2.GetBytes(KeySize);

            return CryptographicOperations.FixedTimeEquals(key, keyToCheck);
        }
    }
}
