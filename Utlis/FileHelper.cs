using System.Data;
using System.IO;
using System.Data;
using System.Text;
using System.Globalization;

namespace KoTblDbImporter.Utlis
{
    public static class FileHelper
    {
        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }
        public static string[] GetTblFileNames(string path)
        {
            string fullPath = Path.Combine(path);

            if (DirectoryExists(fullPath))
            {
                return Directory.GetFiles(fullPath)
                                .Where(file => file.EndsWith(".tbl", StringComparison.OrdinalIgnoreCase))
                                .ToArray();
            }
            else
            {
                throw new DirectoryNotFoundException($"The directory '{fullPath}' does not exist.");
            }
        }
    }
}

