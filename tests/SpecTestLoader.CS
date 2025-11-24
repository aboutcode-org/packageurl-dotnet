using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PackageUrl.Tests
{
    public class PurlTestCase
    {
        public string Description { get; set; }
        public string TestType { get; set; }
        public object Input { get; set; }
        public object ExpectedOutput { get; set; }
        public bool ExpectedFailure { get; set; }
        public string TestGroup { get; set; }
        public string FileName { get; set; }
    }

    public static class SpecTestLoader
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static List<PurlTestCase> LoadSpecFile(string relativePath)
        {
            string json = File.ReadAllText(relativePath);
            var root = JsonSerializer.Deserialize<SpecFile>(json, JsonOptions);

            var list = new List<PurlTestCase>();
            foreach (var tc in root.Tests)
            {
                tc.FileName = Path.GetFileName(relativePath);
                list.Add(tc);
            }

            return list;
        }

        public static List<PurlTestCase> LoadTypeCases(string[] files)
        {
            var output = new List<PurlTestCase>();

            foreach (var file in files)
            {
                string json = File.ReadAllText(file);
                var root = JsonSerializer.Deserialize<SpecFile>(json, JsonOptions);

                foreach (var tc in root.Tests)
                {
                    tc.FileName = Path.GetFileName(file);
                    output.Add(tc);
                }

            }

            return output;
        }

        private class SpecFile
        {
            public List<PurlTestCase> Tests { get; set; }
        }
    }
}
