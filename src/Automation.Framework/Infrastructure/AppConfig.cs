using NUnit.Framework;
using System;
using System.Text.Json;

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
        // 1. CLOUD PIPELINE (Highest Priority): Controlled strictly by GitHub Actions YAML
        var envVar = Environment.GetEnvironmentVariable("RUN_AI_TRIAGE");
        if (!string.IsNullOrEmpty(envVar) && bool.TryParse(envVar, out var envResult))
        {
            return envResult;
        }

        // 2. LOCAL DEV OVERRIDE: Reads the untracked localRun.json if it exists
        string localConfigPath = Path.Combine(AppContext.BaseDirectory, "localRun.json");
        if (File.Exists(localConfigPath))
        {
            try
            {
                var json = File.ReadAllText(localConfigPath);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("RunAiTriage", out var prop))
                {
                    return prop.GetBoolean();
                }
            }
            catch
            {
                // Failsafe: If the JSON is malformed, ignore it rather than crashing the test run
            }
        }

        // 3. LEGACY FALLBACK: NUnit .runsettings parameters
        var runLocal = TestContext.Parameters.Get("RunAiTriage");
        if (!string.IsNullOrEmpty(runLocal) && bool.TryParse(runLocal, out var localResult))
        {
            return localResult;
        }

        // 4. DEFAULT: Safe off-state
        return false;
    }
}