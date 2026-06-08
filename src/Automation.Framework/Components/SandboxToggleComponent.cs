using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Automation.Framework.Components;

public class SandboxToggleComponent : BaseComponent
{
    // Scope search strictly to the header/banner area
    public SandboxToggleComponent(IPage page)
        : base(page, page.GetByRole(AriaRole.Banner))
    {
    }

    public ILocator PublicSandboxBtn => RootLocator.Locator("button:has-text('Public Sandbox'), a:has-text('Public Sandbox')");
    public ILocator SecureSandboxBtn => RootLocator.Locator("button:has-text('Secure Sandbox'), a:has-text('Secure Sandbox')");

    public async Task SwitchToSecureAsync()
    {
        await SecureSandboxBtn.ClickAsync();
    }

    public async Task SwitchToPublicAsync()
    {
        await PublicSandboxBtn.ClickAsync();
    }
}