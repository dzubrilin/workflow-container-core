/*
using System.Collections.Generic;
using Diadem.Core.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Diadem.Web
{
    public class DeleteTemporaryFileResultFilter : IResultFilter
    {
        public const string DeleteTemporaryFileHttpContextKey = "DeleteTemporaryFileHttpContextKey";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeleteTemporaryFileResultFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            // do nothing in here
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            if (!_httpContextAccessor.HttpContext.Items.ContainsKey(DeleteTemporaryFileHttpContextKey))
            {
                return;
            }

            var fileList = (List<string>) _httpContextAccessor.HttpContext.Items[DeleteTemporaryFileHttpContextKey];
            foreach (var fileLocation in fileList)
            {
                FileUtility.DeleteFileNoThrow(fileLocation);
            }
        }
    }
}
*/