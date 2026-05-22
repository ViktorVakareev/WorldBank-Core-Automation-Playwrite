using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Allure.Net.Commons;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Automation.Framework.Infrastructure;

[TestFixture]
public abstract class AiTriage : PageTest
{
    // Singleton HTTP client prevents port exhaustion during heavy parallel Llama requests
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

    public BrowserTypeLaunchOptions LaunchOptions()
    {
        var options = new BrowserTypeLaunchOptions();

        // 🛡️ DEVSECOPS FIX: Prevents Docker container deadlocks & memory exhaustion
        options.Args = new[]
        {
            "--disable-dev-shm-usage",
            "--no-sandbox",
            "--disable-gpu"
        };
        return options;
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();

        // Native relative routing
        options.BaseURL = AppConfig.GetBaseUrl();
        options.ViewportSize = new ViewportSize { Width = 1920, Height = 1080 };

        // 🛡️ I/O ISOLATION: Guarantees parallel browser threads NEVER lock each other's video files
        var threadId = TestContext.CurrentContext.WorkerId ?? Guid.NewGuid().ToString();
        options.RecordVideoDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestResults", "videos", threadId);
        options.RecordVideoSize = new RecordVideoSize { Width = 1280, Height = 720 };

        return options;
    }

    [TearDown]
    public async Task ExecuteEnterpriseTeardownAsync()
    {
        var context = TestContext.CurrentContext;
        bool isFailed = context.Result.Outcome.Status == TestStatus.Failed;
        var testName = context.Test.Name;

        /* ==========================================
           PHASE 1: VISUAL ARTIFACTS & I/O CLEANUP
           ========================================== */
        if (isFailed)
        {
            var screenshotPath = Path.Combine(context.TestDirectory, "TestResults", $"{testName}_{Guid.NewGuid():N}.png");
            await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true });
            TestContext.AddTestAttachment(screenshotPath, "📸 UI State on Failure");
        }

        // CRITICAL: Force Playwright to flush the .webm video file to disk
        await Context.CloseAsync();

        if (Page.Video != null)
        {
            if (isFailed)
            {
                var videoPath = await Page.Video.PathAsync();
                TestContext.AddTestAttachment(videoPath, "🎥 Execution Recording");
            }
            else
            {
                try
                {
                    await Page.Video.DeleteAsync(); // Save Jenkins disk space
                }
                catch (IOException ex)
                {
                    TestContext.Progress.WriteLine($"[WARNING] Video lock ignored: {ex.Message}");
                }
            }
        }

        /* ==========================================
           PHASE 2: Llama AI FAILURE TRIAGE
           ========================================== */
        if (isFailed && AppConfig.ShouldRunAiTriage())
        {
            var stackTrace = context.Result.StackTrace ?? "No stack trace available";
            var errorMessage = context.Result.Message ?? "No error message available";

            var aiAnalysis = await ProcessAiRequestAsync(errorMessage, stackTrace, testName);

            if (!string.IsNullOrEmpty(aiAnalysis))
            {
                try
                {
                    // 1. Create a physical Markdown file in the TestResults directory
                    var safeTestName = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));
                    var mdFileName = $"AI_Analysis_{safeTestName}_{Guid.NewGuid():N}.md";
                    var mdFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestResults", mdFileName);

                    // 2. Write the Llama output to the disk
                    await File.WriteAllTextAsync(mdFilePath, aiAnalysis, Encoding.UTF8);

                    // 3. Attach natively via NUnit. 
                    // 🛡️ Allure automatically intercepts this and pins it to the HTML report seamlessly.
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