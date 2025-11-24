using System;
using System.IO;
using System.Linq;

namespace PackageUrl.Tests
{
    public static class TestFileLoader
    {
        public static string[] LoadJsonFiles(string relativePath)
        {
            var root = FindSolutionRoot();

            var fullPath = Path.Combine(root, relativePath);

            if (!Directory.Exists(fullPath))
                throw new DirectoryNotFoundException($"Directory not found: {fullPath}");

            return Directory
                .GetFiles(fullPath, "*.json", SearchOption.TopDirectoryOnly);
        }

        private static string FindSolutionRoot()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (dir != null)
            {
                if (dir.GetFiles("PackageUrl.sln").Any())
                    return dir.FullName;

                dir = dir.Parent;
            }

            throw new Exception("Could not locate solution root (PackageUrl.sln).");
        }
    }
}
