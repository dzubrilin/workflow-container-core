/*
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Diadem.Web
{
    public static class HttpContextExtensions
    {
        public static void RegisterTemporaryFileForDeletion(this HttpContext httpContext, string fileLocation)
        {
            List<string> fileList;
            if (httpContext.Items.ContainsKey(DeleteTemporaryFileResultFilter.DeleteTemporaryFileHttpContextKey))
            {
                fileList = (List<string>) httpContext.Items[DeleteTemporaryFileResultFilter.DeleteTemporaryFileHttpContextKey];
                fileList.Add(fileLocation);
                return;
            }

            fileList = new List<string> {fileLocation};
            httpContext.Items.Add(DeleteTemporaryFileResultFilter.DeleteTemporaryFileHttpContextKey, fileList);
        }
    }
}
*/