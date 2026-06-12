using NUnit.Framework;
using System.Collections.Concurrent;
using System.IO;
using System.Xml.Linq;

// 🛑 ARCHITECTURAL OVERRIDE: Disable all parallelization to prevent local CPU thrashing
[assembly: Parallelizable(ParallelScope.None)]
[assembly: LevelOfParallelism(1)]

namespace Automation.Framework.Infrastructure;

[SetUpFixture]
public class GlobalSetup
{
    // 🎯 Thread-safe collection to catch failures during parallel execution
    public static ConcurrentBag<string> FailedTestNames = new();

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        var targetDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestResults");
        if (Directory.Exists(targetDir))
        {
            Directory.Delete(targetDir, true);
        }
        Directory.CreateDirectory(targetDir);
    }

    [OneTimeTearDown]
    public void GenerateFailedTestsPlaylist()
    {
        // Only generate the playlist if there were actual failures
        if (!FailedTestNames.IsEmpty)
        {
            var resultsDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestResults");
            Directory.CreateDirectory(resultsDir);

            var playlistPath = Path.Combine(resultsDir, "FailedTests.playlist");

            // 🎯 Build a Visual Studio 2022 compatible (V2.0) XML Playlist
            var includesRule = new XElement("Rule",
                new XAttribute("Name", "Includes"),
                new XAttribute("Match", "Any"),
                FailedTestNames.Distinct().Select(testName =>
                    new XElement("Property",
                        new XAttribute("Name", "TestWithNormalizedFullyQualifiedName"),
                        new XAttribute("Value", testName)
                    )
                )
            );

            var playlistDoc = new XElement("Playlist",
                new XAttribute("Version", "2.0"),
                includesRule
            );

            playlistDoc.Save(playlistPath);
            TestContext.Progress.WriteLine($"[INFO] Generated VS Playlist with {FailedTestNames.Count} failed tests.");
        }
    }
}