using System.Security.Cryptography;
using System.Text;

namespace AlfLab.Api.Infrastructure.Security
{
    public static class EncryptionHelper
    {
        private static readonly string EncryptionKey = "AlfLabSecretKeyParaDatosSensible"; 
        private static readonly byte[] Salt = { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

        public static string Encrypt(string clearText)
        {
            if (string.IsNullOrEmpty(clearText)) return clearText;
            
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                // MÉTODO MODERNO: Generamos 48 bytes de un solo golpe sin instanciar objetos obsoletos
                byte[] keyMaterial = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(EncryptionKey),
                    Salt,
                    100000,
                    HashAlgorithmName.SHA256,
                    48 // 32 para la Key + 16 para el IV = 48 bytes en total
                );

                // Dividimos el material usando rangos modernos de C#
                encryptor.Key = keyMaterial[0..32];  // Toma del byte 0 al 31
                encryptor.IV = keyMaterial[32..48];  // Toma del byte 32 al 47
                
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            
            if (!cipherText.Contains("=") && !cipherText.Contains("+") && !cipherText.Contains("/")) 
                return cipherText;

            try 
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    // MÉTODO MODERNO: Repetimos el proceso de derivación exactamente igual
                    byte[] keyMaterial = Rfc2898DeriveBytes.Pbkdf2(
                        Encoding.UTF8.GetBytes(EncryptionKey),
                        Salt,
                        100000,
                        HashAlgorithmName.SHA256,
                        48
                    );

                    encryptor.Key = keyMaterial[0..32];
                    encryptor.IV = keyMaterial[32..48];
                    
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                return cipherText;
            }
            catch 
            {
                return cipherText; 
            }
        }
    }
}