using System.IO;
using NUnit.Framework;

// 🛑 ARCHITECTURAL OVERRIDE: Disable all parallelization to prevent local CPU thrashing
[assembly: Parallelizable(ParallelScope.None)]
[assembly: LevelOfParallelism(1)]

namespace Automation.Framework.Infrastructure;

[SetUpFixture]
public class GlobalSetup
{
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
}