using System;
using System.Collections.Generic;
using System.IO;
using Serilog;

namespace Diadem.Core.IO
{
    public static class FileUtility
    {
        public static string CalculateSha512Hash(string fileLocation)
        {
            if (string.IsNullOrEmpty(fileLocation) || !File.Exists(fileLocation))
            {
                return string.Empty;
            }

            using (var fileStream = File.Open(fileLocation, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                return StreamUtility.CalculateSha512Hash(fileStream);
            }
        }

        public static void DeleteFiles(List<string> filesToDelete)
        {
            Guard.ArgumentNotNull(filesToDelete, nameof(filesToDelete));
            foreach (var fileToDelete in filesToDelete)
            {
                DeleteFile(fileToDelete);
            }
        }
        
        public static void DeleteFile(string fileLocation)
        {
            if (string.IsNullOrEmpty(fileLocation) || !File.Exists(fileLocation))
            {
                return;
            }

            try
            {
                File.Delete(fileLocation);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error has occurred during deletion of a {fileLocation}", fileLocation);
                throw;
            }
        }
        
        public static void DeleteFileNoThrow(string fileLocation)
        {
            if (string.IsNullOrEmpty(fileLocation) || !File.Exists(fileLocation))
            {
                return;
            }

            try
            {
                File.Delete(fileLocation);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error has occurred during deletion of a {fileLocation}", fileLocation);
            }
        }

        public static string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public static long GetFileSize(string fileLocation)
        {
            if (string.IsNullOrEmpty(fileLocation) || !File.Exists(fileLocation))
            {
                return 0;
            }

            var fileInfo = new FileInfo(fileLocation);
            return fileInfo.Length;
        }
    }
}