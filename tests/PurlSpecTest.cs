using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace PackageUrl.Tests
{
    public class PurlSpecTests
    {
        private static readonly string[] specFiles = TestFileLoader.LoadJsonFiles("spec/tests/spec");

        private static readonly string[] typeFiles = TestFileLoader.LoadJsonFiles("spec/tests/types");

        private static readonly List<PurlTestCase> AllTests =
            SpecTestLoader.LoadSpecFile(specFiles[0]);
        
        public static IEnumerable<object[]> TypeTests =>
            SpecTestLoader.LoadTypeCases(typeFiles)
                          .ConvertAll(tc => new object[] { tc });

        public static IEnumerable<object[]> ParseTests =>
        AllTests
            .FindAll(t => t.TestType == "parse")
            .ConvertAll(t => new object[] { t });

        public static IEnumerable<object[]> BuildTests =>
            AllTests
                .FindAll(t => t.TestType == "build")
                .ConvertAll(t => new object[] { t });



        [Theory]
        [MemberData(nameof(ParseTests))]
        public void Parse(PurlTestCase tc)
        {
            if (tc.ExpectedFailure)
            {
                Assert.ThrowsAny<System.Exception>(() =>
                {
                    PackageURL p = new PackageURL(tc.Input.ToString());
                });
            }
            else
            {
                PackageURL p = new PackageURL(tc.Input.ToString());
                Assert.Equal(tc.ExpectedOutput.ToString(), p.ToString());
            }
        }


        [Theory]
        [MemberData(nameof(BuildTests))]
        public void Build(PurlTestCase tc)
        {
            var inp = (JsonElement)tc.Input;

            var purl = new PackageURL(
                type: inp.GetProperty("type").GetString(),
                @namespace: inp.TryGetProperty("namespace", out var ns) ? ns.GetString() : null,
                name: inp.GetProperty("name").GetString(),
                version: inp.TryGetProperty("version", out var ver) ? ver.GetString() : null,
                qualifiers: inp.TryGetProperty("qualifiers", out var q) ? 
                    JsonSerializer.Deserialize<SortedDictionary<string, string>>(q.GetRawText()) : null,
                subpath: inp.TryGetProperty("subpath", out var sp) ? sp.GetString() : null
            );

            if (tc.ExpectedFailure)
            {
                Assert.ThrowsAny<System.Exception>(purl.ToString);
            }
            else
            {
                Assert.Equal(tc.ExpectedOutput.ToString(), purl.ToString());
            }
        }


        [Theory]
        [MemberData(nameof(TypeTests))]
        public void TypeCases(PurlTestCase tc)
        {
            if (tc.ExpectedFailure)
            {
                Assert.ThrowsAny<System.Exception>(() => RunCase(tc));
            }
            else
            {
                RunCase(tc);
            }
        }

        private void RunCase(PurlTestCase tc)
        {
            switch (tc.TestType)
            {
                case "parse":
                    PackageURL p1 = new PackageURL(tc.Input.ToString());
                    var exp = (JsonElement)tc.ExpectedOutput;

                    Assert.Equal(exp.GetProperty("type").GetString(), p1.Type);
                    Assert.Equal(exp.GetProperty("namespace").GetString(), p1.Namespace);
                    Assert.Equal(exp.GetProperty("name").GetString(), p1.Name);
                    Assert.Equal(exp.GetProperty("version").GetString(), p1.Version);
                    Assert.Equal(exp.GetProperty("subpath").GetString(), p1.Subpath);
                    if (exp.TryGetProperty("qualifiers", out var qual))
                    {
                        var expectedQualifiers = JsonSerializer.Deserialize<SortedDictionary<string, string>>(qual.GetRawText());
                        Assert.Equal(expectedQualifiers, p1.Qualifiers);
                    }
                    else
                    {
                        Assert.Null(p1.Qualifiers);
                    }
                    break;

                case "roundtrip":
                    PackageURL p2 = new PackageURL(tc.Input.ToString());
                    Assert.Equal(tc.ExpectedOutput.ToString(), p2.ToString());
                    break;

                case "build":
                    var inp = (JsonElement)tc.Input;
                    var purl = new PackageURL(
                        type: inp.GetProperty("type").GetString(),
                        @namespace: inp.TryGetProperty("namespace", out var ns) ? ns.GetString() : null,
                        name: inp.GetProperty("name").GetString(),
                        version: inp.TryGetProperty("version", out var ver) ? ver.GetString() : null,
                        qualifiers: inp.TryGetProperty("qualifiers", out var q) ? 
                    JsonSerializer.Deserialize<SortedDictionary<string, string>>(q.GetRawText()) : null,
                        subpath: inp.TryGetProperty("subpath", out var sp) ? sp.GetString() : null
                    );

                    if (tc.ExpectedFailure)
                    {
                        Assert.ThrowsAny<System.Exception>(() => purl.ToString());
                    }
                    else
                    {
                        Assert.Equal(tc.ExpectedOutput.ToString(), purl.ToString());
                    }
                    break;
            }
        }
    }
}
