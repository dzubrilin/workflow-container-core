using System;
using System.IO;
using Diadem.Core.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace Diadem.Web
{
    public class TemporaryDirectoryFileProvider : IFileProvider
    {
        private readonly IConfigurationProvider _configurationProvider;

        public TemporaryDirectoryFileProvider(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (string.IsNullOrEmpty(subpath))
            {
                return new NotFoundFileInfo(subpath);
            }
            
            var coreConfigurationSection = _configurationProvider.GetSection<ICoreConfigurationSection>();
            var tempDirectory = coreConfigurationSection.TempDirectory;
            var requestedFileDirectory = Path.GetDirectoryName(subpath);

            if (!string.Equals(tempDirectory, requestedFileDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return new NotFoundFileInfo(subpath); 
            }
            
            var fileInfo = new FileInfo(subpath);
            return new PhysicalFileInfo(fileInfo);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return NotFoundDirectoryContents.Singleton;
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }
    }
}