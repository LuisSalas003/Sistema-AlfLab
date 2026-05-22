using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AlfLab.Api.Infrastructure.Security
{
    public static class EncryptionHelper
    {
        private static readonly string EncryptionKey = Environment.GetEnvironmentVariable("DB_ENCRYPTION_KEY") ?? "AlfLabSecretKeyParaDatosSensible123!"; 
        private static readonly byte[] Salt = { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

        // 👇 MODIFICACIÓN APLICADA: Inicialización inline usando una Tupla de C# (Satisface S3963)
        private static readonly (byte[] Key, byte[] IV) CryptoKeys = GenerateCryptoKeys();

        // Este método se ejecuta una sola vez al inicializar la variable CryptoKeys
        private static (byte[] Key, byte[] IV) GenerateCryptoKeys()
        {
            byte[] keyMaterial = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(EncryptionKey),
                Salt,
                100000, 
                HashAlgorithmName.SHA256,
                48 
            );

            return (keyMaterial[0..32], keyMaterial[32..48]);
        }

        public static string Encrypt(string clearText)
        {
            if (string.IsNullOrEmpty(clearText)) return clearText;
            
            byte[] clearBytes = Encoding.UTF8.GetBytes(clearText); 
            
            using Aes aes = Aes.Create();
            aes.Key = CryptoKeys.Key; // 👈 Consumimos desde la tupla en memoria
            aes.IV = CryptoKeys.IV;   // 👈 Consumimos desde la tupla en memoria

            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            
            cs.Write(clearBytes, 0, clearBytes.Length);
            cs.FlushFinalBlock();
            
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            
            if (!cipherText.Contains("=") && !cipherText.Contains("+") && !cipherText.Contains("/")) 
                return cipherText;

            try 
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                
                using Aes aes = Aes.Create();
                aes.Key = CryptoKeys.Key; // 👈 Consumimos desde la tupla en memoria
                aes.IV = CryptoKeys.IV;   // 👈 Consumimos desde la tupla en memoria

                using MemoryStream ms = new MemoryStream();
                using CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
                
                cs.Write(cipherBytes, 0, cipherBytes.Length);
                cs.FlushFinalBlock();
                
                return Encoding.UTF8.GetString(ms.ToArray()); 
            }
            catch 
            {
                return cipherText; 
            }
        }
    }
}