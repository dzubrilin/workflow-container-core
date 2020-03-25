using System;
using System.IO;
using System.Security.Cryptography;

namespace Diadem.Core.IO
{
    public static class StreamUtility
    {
        public static string CalculateSha512Hash(Stream stream)
        {
            Guard.ArgumentNotNull(stream, nameof(stream));
            using (var sha512 = SHA512.Create())
            {
                var hash = sha512.ComputeHash(stream);
                return Convert.ToBase64String(hash);
            }
        }
        
        public static string CalculateSha512HashAndReset(Stream stream)
        {
            var result = CalculateSha512Hash(stream);
            stream.Position = 0;
            return result;
        }
    }
}