using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Diadem.Core.Common
{
    public static class StringExtensions
    {
        public static string CamelCase(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            if (char.IsLower(text[0]))
            {
                return text;
            }

            return text.Length == 1 ? text.ToLower() : $"{text.Substring(0, 1).ToLower()}{text.Substring(1)}";
        }

        public static bool HasNonAsciiCharacter(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (var i = value.Length - 1; i >= 0; i--)
            {
                if (value[i] > 127)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool EqualsIgnoreCase(this string string1, string string2)
        {
            return string.Equals(string1, string2, StringComparison.OrdinalIgnoreCase);
        }

        public static string EncodeNonAsciiCharacters(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var stringBuilder = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    var encodedValue = $"\\u{((int) c):x4}";
                    stringBuilder.Append(encodedValue);
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }

        public static string DecodeEncodedNonAsciiCharacters(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return Regex.Replace(value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => ((char) int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());
        }

        /*
        public static bool TryCreateUri(this string uriString, UriKind uriKind, out Uri uri)
        {
            if (string.IsNullOrEmpty(uriString))
            {
                uri = (Uri) null;
                return false;
            }

            if (Uri.TryCreate(uriString, uriKind, out uri))
            {
                var queryString = uri.PathAndQuery;
                if (!string.IsNullOrEmpty(queryString))
                {
                    var queryStringEncoded = WebUtility.UrlEncode(queryString);
                    if (!string.IsNullOrEmpty(queryStringEncoded) &&
                        !string.Equals(queryString, queryStringEncoded, StringComparison.OrdinalIgnoreCase))
                    {
                        var uriBuilder = new UriBuilder(uri.Scheme, uri.Host, uri.Port, queryStringEncoded);
                        uri = uriBuilder.Uri;
                    }
                }
                
                return true;
            }
            
            return false;
        }
        */
    }
}