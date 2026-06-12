using Allure.NUnit;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework.Interfaces;
using System.Text;
using System.Text.Json;
using Automation.Framework;

namespace Automation.Framework.Infrastructure;

[TestFixture]
[AllureNUnit]
public abstract class AiTriage : PageTest
{
    // Singleton HTTP client prevents port exhaustion during heavy parallel Llama requests
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromMinutes(5) };

    public BrowserTypeLaunchOptions LaunchOptions()
    {
        var options = new BrowserTypeLaunchOptions();

        // Check if we are running in CI or Local
        bool isCI = Environment.GetEnvironmentVariable("CI") == "true";

        // 🐛 LOCAL DEBUGGING: Force Playwright to ignore VS caching and show the UI
        if (!isCI)
        {
            Environment.SetEnvironmentVariable("PLAYWRIGHT_HEADLESS", "false");
        }

        if (isCI)
        {
            // 🛡️ DEVSECOPS FIX: Silent, aggressive memory flags for CI runners
            options.Args = new[]
            {
                "--disable-dev-shm-usage",
                "--no-sandbox",
                "--disable-gpu"
            };
            options.Headless = true;
        }
        else
        {
            // 🐛 LOCAL DEBUGGING: Show the browser UI and slow it down for the human eye
            options.Headless = false;
            options.SlowMo = 50;
        }

        return options;
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();

        // Native relative routing
        options.BaseURL = AppConfig.GetBaseUrl();
        options.ViewportSize = new ViewportSize { Width = 1920, Height = 1080 };

        // 🛡️ I/O ISOLATION: Guarantees parallel browser threads NEVER lock each other's video files
        var threadId = TestContext.CurrentContext.WorkerId ?? Guid.NewGuid().ToString("N");
        options.RecordVideoDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestResults", "videos", threadId);
        options.RecordVideoSize = new RecordVideoSize { Width = 1280, Height = 720 };

        return options;
    }

    [SetUp]
    public async Task EnterpriseSetupAsync()
    {
        // 🔬 PLAYWRIGHT TRACING: Begin recording the DOM, Network, and Console
        await Context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });
    }

    [TearDown]
    public async Task ExecuteEnterpriseTeardownAsync()
    {
        var context = TestContext.CurrentContext;
        bool isFailed = context.Result.Outcome.Status == TestStatus.Failed;
        var testName = context.Test.Name;
        var safeTestName = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));

        // 🎯 THE FIX: Force the OS to create the directory on the ephemeral runner
        var resultsDir = Path.Combine(context.TestDirectory, "TestResults");
        Directory.CreateDirectory(resultsDir);

        /* ==========================================
           PHASE 1: TRACING & VISUAL ARTIFACTS
           ========================================== */
        if (isFailed)
        {
            // 🎯 NEW: Add the fully-qualified test name to our VS Playlist tracker
            GlobalSetup.FailedTestNames.Add(context.Test.FullName);

            try
            {
                // 1. Capture Full Page Screenshot
                var screenshotPath = Path.Combine(resultsDir, $"{safeTestName}_{Guid.NewGuid():N}.png");
                await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true })
                          .WaitAsync(TimeSpan.FromSeconds(5));
                TestContext.AddTestAttachment(screenshotPath, "📸 UI State on Failure");

                // 2. Package and Save the Playwright Trace Zip
                var tracePath = Path.Combine(resultsDir, $"trace_{safeTestName}_{Guid.NewGuid():N}.zip");
                await Context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath })
                          .WaitAsync(TimeSpan.FromSeconds(5));
                TestContext.AddTestAttachment(tracePath, "🔍 Playwright DOM Trace (trace.playwright.dev)");
            }           
            catch (TimeoutException)
            {
                TestContext.Progress.WriteLine("[WARNING] Browser process deadlocked. Visual artifact extraction aborted.");
            }
            catch (Exception ex)
            {
                TestContext.Progress.WriteLine($"[WARNING] Failed to extract visual artifacts: {ex.Message}");
            }
        }
        else
        {
            // If the test passed, gracefully discard the trace data from RAM
            await Context.Tracing.StopAsync(new TracingStopOptions { Path = null });
        }

        // 3. Handle Video Attachments
        if (Page.Video != null)
        {
            if (isFailed)
            {
                try
                {
                    // Safely extract the video path and attach it to ReportPortal/Allure
                    var videoPath = await Page.Video.PathAsync().WaitAsync(TimeSpan.FromSeconds(5));
                    TestContext.AddTestAttachment(videoPath, "🎥 Execution Recording");
                }
                catch (Exception ex)
                {
                    TestContext.Progress.WriteLine($"[WARNING] Could not retrieve video path: {ex.Message}");
                }
            }
        }

        /* ==========================================
           PHASE 2: Llama AI FAILURE TRIAGE
           ========================================== */
        if (isFailed && ConfigReader.ShouldRunAiTriage)
        {
            var stackTrace = context.Result.StackTrace ?? "No stack trace available";
            var errorMessage = context.Result.Message ?? "No error message available";

            var aiAnalysis = await ProcessAiRequestAsync(errorMessage, stackTrace, testName);

            if (!string.IsNullOrEmpty(aiAnalysis))
            {
                try
                {
                    var mdFileName = $"AI_Analysis_{safeTestName}_{Guid.NewGuid():N}.md";
                    // 🎯 THE FIX: Use the guaranteed directory here too
                    var mdFilePath = Path.Combine(resultsDir, mdFileName);

                    await File.WriteAllTextAsync(mdFilePath, aiAnalysis, Encoding.UTF8);
                    TestContext.AddTestAttachment(mdFilePath, $"🤖 AI Analysis - {testName}");                
                }
                catch (Exception ex)
                {
                    TestContext.Progress.WriteLine($"[WARNING] Failed to save AI Triage attachment: {ex.Message}");
                }
            }
        }
    }

    private async Task<string> ProcessAiRequestAsync(string errorMessage, string stackTrace, string testName)
    {
        TestContext.Progress.WriteLine($"[AI TRIAGE] Connecting to Llama model for {testName}...");
        try
        {
            var prompt = $"As a Senior QA Automation Architect, analyze this Playwright C# test failure.\nTest: {testName}\nError: {errorMessage}\nStack Trace: {stackTrace}\n\nProvide a very concise 3-bullet-point root cause analysis. Strictly classify it as 'Locator Rot', 'Application Defect', or 'Infrastructure Timeout'.";

            var payload = new { model = "llama3", prompt = prompt, stream = false };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync("http://localhost:11434/api/generate", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return jsonResponse.RootElement.GetProperty("response").GetString();
            }
            return "⚠️ AI Triage Endpoint Returned Non-Success Status Code.";
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"[AI TRIAGE ERROR] {ex.Message}");
            return $"⚠️ AI Triage Unavailable: {ex.Message}";
        }
    }
}