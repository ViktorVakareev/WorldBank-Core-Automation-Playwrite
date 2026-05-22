using System;
using NUnit.Framework;

namespace Automation.Framework.Infrastructure;

public static class AppConfig
{
    public static string GetBaseUrl()
    {
        // Check local runsettings parameters first; fallback to CI env variable; fallback to hardcoded default
        return TestContext.Parameters.Get("BaseUrl")
               ?? Environment.GetEnvironmentVariable("BASE_URL")
               ?? "https://viktorvakareev.github.io/Playwright-DotNet-Enterprise-Architecture/WorldBankMockApp/dev/";
    }

    public static bool ShouldRunAiTriage()
    {
        var runLocal = TestContext.Parameters.Get("RunAiTriage");
        if (!string.IsNullOrEmpty(runLocal))
        {
            return bool.TryParse(runLocal, out var localResult) && localResult;
        }

        var runCi = Environment.GetEnvironmentVariable("RUN_AI_TRIAGE");
        return !string.IsNullOrEmpty(runCi) && runCi.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}