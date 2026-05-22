using Microsoft.Playwright;

namespace Automation.Framework.Components;

public class SandboxToggleComponent
{
    private readonly IPage _page;

    public SandboxToggleComponent(IPage page) => _page = page;

    public ILocator PublicSandboxBtn => _page.Locator("button:has-text('Public Sandbox')");
    public ILocator SecureSandboxBtn => _page.Locator("button:has-text('Secure Sandbox')");

    public async Task SwitchToSecureAsync()
    {
        await SecureSandboxBtn.ClickAsync();
    }

    public async Task SwitchToPublicAsync()
    {
        await PublicSandboxBtn.ClickAsync();
    }
}