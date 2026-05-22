using System.IO;
using NUnit.Framework;

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