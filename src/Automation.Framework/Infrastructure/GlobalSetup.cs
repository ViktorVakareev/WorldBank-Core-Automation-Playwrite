using NUnit.Framework;
using System.Collections.Concurrent;
using System.IO;
using System.Xml.Linq;

// 🛑 ARCHITECTURAL OVERRIDE: Disable all parallelization to prevent local CPU thrashing
[assembly: Parallelizable(ParallelScope.None)]
[assembly: LevelOfParallelism(1)]

namespace Automation.Framework;

[SetUpFixture]
public class GlobalSetup
{
    // Thread-safe collection to catch failures
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
        if (!FailedTestNames.IsEmpty)
        {
            // 🎯 THE FIX: Use TestDirectory to perfectly match AiTriage.cs
            var resultsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestResults");
            Directory.CreateDirectory(resultsDir);

            var playlistPath = Path.Combine(resultsDir, "FailedTests.playlist");

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

            // Write to the global console so we can see it in the .trx
            TestContext.Progress.WriteLine($"[INFO] Generated VS Playlist at {playlistPath} with {FailedTestNames.Count} failed tests.");
        }
    }
}