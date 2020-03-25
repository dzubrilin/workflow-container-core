using System;
using System.Net;
using System.Text;
using Diadem.Core.DomainModel;

namespace Diadem.Workflow.Provider.Http
{
    public class WorkflowMessageHttpUriBuilder
    {
        private readonly Uri _address;

        public WorkflowMessageHttpUriBuilder(Uri address)
        {
            _address = address;
        }

        public Uri Build(string jsonStateString)
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = _address.Scheme,
                Port = _address.Port,
                Host = _address.Host
            };

            if (string.IsNullOrEmpty(jsonStateString))
            {
                uriBuilder.Path = _address.AbsolutePath;
                uriBuilder.Query = _address.Query;
                return uriBuilder.Uri;
            }

            var jsonState = new JsonState(jsonStateString);
            if (!string.IsNullOrEmpty(_address.AbsolutePath))
            {
                uriBuilder.Path = Parametrize(_address, _address.AbsolutePath, jsonState, false);
            }
            
            if (!string.IsNullOrEmpty(_address.Query))
            {
                uriBuilder.Query = Parametrize(_address, _address.Query, jsonState, true);
            }

            return uriBuilder.Uri;
        }

        private static string Parametrize(Uri address, string stringToParametrize, JsonState jsonState, bool encode)
        {
            var decodedStringToParametrize = WebUtility.UrlDecode(stringToParametrize);
            if (decodedStringToParametrize.IndexOf("{", StringComparison.OrdinalIgnoreCase) == -1)
            {
                return decodedStringToParametrize;
            }
            
            var stringBuilder = new StringBuilder(decodedStringToParametrize.Length);
            for (var i = 0; i < decodedStringToParametrize.Length; i++)
            {
                if (decodedStringToParametrize[i] == '{')
                {
                    var endIndex = decodedStringToParametrize.IndexOf('}', i + 1);
                    if (endIndex == -1)
                    {
                        throw new ApplicationException($"Address [{address}] is malformed for parametrization");
                    }

                    var parameterName = decodedStringToParametrize.Substring(i + 1, endIndex - i - 1);
                    var parameterValue = jsonState.GetProperty<string>(parameterName);

                    stringBuilder.Append(encode ? WebUtility.UrlEncode(parameterValue) : parameterValue);
                    i += endIndex - i;
                }
                else
                {
                    stringBuilder.Append(decodedStringToParametrize[i]);
                }
            }

            return stringBuilder.ToString();
        }
    }
}