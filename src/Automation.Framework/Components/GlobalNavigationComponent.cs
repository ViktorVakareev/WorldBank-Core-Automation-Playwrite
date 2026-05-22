using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Automation.Framework.Components;

public class GlobalNavigationComponent
{
    private readonly IPage _page;

    // 🛡️ LOCATORS: Scoped strictly to the navigation header
    public ILocator BrandTitle => _page.Locator(".brand-title, [data-testid='app-title']");
    public ILocator HomeLink => _page.GetByRole(AriaRole.Link, new() { Name = "Home" });
    public ILocator DashboardLink => _page.GetByRole(AriaRole.Link, new() { Name = "Dashboard" });
    public ILocator WireTransferLink => _page.GetByRole(AriaRole.Link, new() { Name = "Wire Transfer" });
    public ILocator SettingsLink => _page.GetByRole(AriaRole.Link, new() { Name = "Settings" });

    public ILocator PublicSandboxBadge => _page.GetByText("Public Sandbox");
    public ILocator SecureSandboxBadge => _page.GetByText("Secure Sandbox");

    public GlobalNavigationComponent(IPage page)
    {
        _page = page;
    }

    // Encapsulated routing logic
    public async Task NavigateToAsync(string target)
    {
        ILocator element = target.ToLower() switch
        {
            "dashboard" => DashboardLink,
            "wire transfer" => WireTransferLink,
            "settings" => SettingsLink,
            _ => HomeLink
        };
        await element.ClickAsync();
    }
}