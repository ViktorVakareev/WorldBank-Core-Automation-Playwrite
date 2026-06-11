using Microsoft.Extensions.Configuration;

public static class ConfigReader
{
    private static IConfigurationRoot _config;

    static ConfigReader()
    {
        _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            // 1. Load the baseline defaults (Committed to Git)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)

            // 2. Load the local overrides (Git-ignored, overrides step 1)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)

            // 3. Load Environment Variables (GitHub Actions, overrides everything)
            .AddEnvironmentVariables()
            .Build();
    }

    // Example Usage Methods
    public static string BaseUrl => _config["Framework:BaseUrl"];
    public static bool Headless => bool.Parse(_config["Playwright:Headless"] ?? "true");
    public static bool ShouldRunAiTriage => bool.Parse(_config["AiTriage:Enabled"] ?? "false");
}