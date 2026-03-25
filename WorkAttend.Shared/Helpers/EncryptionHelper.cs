using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WorkAttend.Shared.Helpers
{
    public static class EncryptionHelper
    {
        // IMPORTANT:
        // DES requires exactly 8 bytes for Key and 8 bytes for IV.
        // Keep these same values if you must decrypt old DB passwords.
        private const string KEY = "PUN*OJWX";
        private const string IV = "MUB*MPRA";

        public static string Encrypt(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            byte[] keyBytes = Encoding.UTF8.GetBytes(KEY);
            byte[] ivBytes = Encoding.UTF8.GetBytes(IV);
            byte[] inputBytes = Encoding.UTF8.GetBytes(text);

            using DES des = DES.Create();
            des.Key = keyBytes;
            des.IV = ivBytes;
            des.Mode = CipherMode.CBC;
            des.Padding = PaddingMode.PKCS7;

            using MemoryStream memoryStream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(
                memoryStream,
                des.CreateEncryptor(),
                CryptoStreamMode.Write);

            cryptoStream.Write(inputBytes, 0, inputBytes.Length);
            cryptoStream.FlushFinalBlock();

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public static string Decrypt(this string encText)
        {
            if (string.IsNullOrWhiteSpace(encText))
                return string.Empty;

            byte[] keyBytes = Encoding.UTF8.GetBytes(KEY);
            byte[] ivBytes = Encoding.UTF8.GetBytes(IV);
            byte[] inputBytes = Convert.FromBase64String(encText);

            using DES des = DES.Create();
            des.Key = keyBytes;
            des.IV = ivBytes;
            des.Mode = CipherMode.CBC;
            des.Padding = PaddingMode.PKCS7;

            using MemoryStream memoryStream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(
                memoryStream,
                des.CreateDecryptor(),
                CryptoStreamMode.Write);

            cryptoStream.Write(inputBytes, 0, inputBytes.Length);
            cryptoStream.FlushFinalBlock();

            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }
}

