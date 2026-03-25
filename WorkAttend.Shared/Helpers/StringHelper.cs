using System;
using System.Security.Cryptography;
using System.Text;

namespace WorkAttend.Shared.Helpers
{
    public static class StringHelper
    {
        public static string GetRandomAlphanumericString(int length)
        {
            const string alphanumericCharacters =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyz" +
                "0123456789" +
                "dakarGeoPunchCharacters";

            return GetRandomString(length, alphanumericCharacters);
        }

        private static string GetRandomString(int length, string allowedChars)
        {
            if (length <= 0)
                return string.Empty;

            if (string.IsNullOrWhiteSpace(allowedChars))
                throw new ArgumentException("allowedChars cannot be null or empty.", nameof(allowedChars));

            var result = new StringBuilder(length);
            var buffer = new byte[4];

            using var rng = RandomNumberGenerator.Create();

            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(buffer);
                uint randomNumber = BitConverter.ToUInt32(buffer, 0);
                result.Append(allowedChars[(int)(randomNumber % (uint)allowedChars.Length)]);
            }

            return result.ToString();
        }
    }
}
