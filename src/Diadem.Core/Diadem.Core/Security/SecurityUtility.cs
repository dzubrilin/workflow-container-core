using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Diadem.Core.Security
{
    public static class SecurityUtility
    {
        private static readonly ConcurrentDictionary<string, byte[]> KeyMap = new ConcurrentDictionary<string, byte[]>();
        
        public static string EncryptWithAes(string text, string key)
        {
            var keyBytes = KeyMap.GetOrAdd(key, GetAesKey);
            using (var aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var memoryStream = new MemoryStream(256))
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (var streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(text);
                    }
                    
                    var encryptedBytes = memoryStream.ToArray();
                    var ivAndEncryptedBytes = new byte[aes.IV.Length + encryptedBytes.Length];
                    
                    Array.Copy(aes.IV, 0, ivAndEncryptedBytes, 0, aes.IV.Length);
                    Array.Copy(encryptedBytes, 0, ivAndEncryptedBytes, aes.IV.Length, encryptedBytes.Length);

                    return Convert.ToBase64String(ivAndEncryptedBytes);
                }
            }
        }
        
        public static string DecryptWithAes(string text, string key)
        {
            var keyBytes = KeyMap.GetOrAdd(key, GetAesKey);
            var encryptedBytes = Convert.FromBase64String(text);
            
            using (var aes = Aes.Create())
            {
                aes.Key = keyBytes;
                
                var iv = new byte[aes.BlockSize/8];
                var cipherBytes = new byte[encryptedBytes.Length - iv.Length];
                
                Array.Copy(encryptedBytes, iv, iv.Length);
                Array.Copy(encryptedBytes, iv.Length, cipherBytes, 0, cipherBytes.Length);

                aes.IV = iv;
                aes.Mode = CipherMode.CBC;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var memoryStream = new MemoryStream(cipherBytes))
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (var streamReader = new StreamReader(cryptoStream))
                    {
                        var result = streamReader.ReadToEnd();
                        return result;
                    }
                }
            }
        }
        
        private static byte[] GetAesKey(string key)
        {
            Guard.ArgumentNotNullOrEmpty(key, nameof(key));
            
            var keyBytes = new byte[16];
            var inputBytes = Encoding.UTF8.GetBytes(key);
            if (inputBytes.Length < keyBytes.Length)
            {
                throw new ArgumentException($"Key {key} is not a valid key for AES encryption");
            }

            var tmpBytes = new byte[keyBytes.Length];
            var intervals = inputBytes.Length / keyBytes.Length + 1;
            for (var interval = 0; interval < intervals; interval++)
            {
                for (var j = 0; j < tmpBytes.Length; j++)
                {
                    tmpBytes[j] = 0;
                }

                var low = interval * keyBytes.Length;
                var high = Math.Min(inputBytes.Length, (interval + 1) * keyBytes.Length);
                for (var index = low; index < high; index++)
                {
                    tmpBytes[index - low] = inputBytes[index];
                }
                
                Xor(keyBytes, tmpBytes);
            }

            return keyBytes;
        }

        private static void Xor(byte[] bytesA, byte[] bytesB)
        {
            if (bytesB.Length == 0)
            {
                return;
            }

            if (bytesB.Length > bytesA.Length)
            {
                throw new Exception("First array must be a base for XOR operation");
            }

            for (var i = 0; i < bytesB.Length; i++)
            {
                bytesA[i] = (byte) (bytesA[i] ^ bytesB[i]);
            }
        }
    }
}