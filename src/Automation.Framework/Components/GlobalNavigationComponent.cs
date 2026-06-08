using Microsoft.Playwright;
using ReportPortal.Client.Abstractions.Responses;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Automation.Framework.Components;

public class GlobalNavigationComponent : BaseComponent
{
    public GlobalNavigationComponent(IPage page)
        : base(page, page.GetByRole(AriaRole.Banner))
    {
    }

    public ILocator BrandTitle => RootLocator.Locator(".brand-title, [data-testid='app-title']");
    public ILocator PublicSandboxBadge => RootLocator.GetByText("Public Sandbox");
    public ILocator SecureSandboxBadge => RootLocator.GetByText("Secure Sandbox");

    public async Task NavigateToAsync(string destination)
    {
        string normalizedDestination = destination.ToLower().Trim();

        string expectedEndpoint = normalizedDestination switch
        {
            "dashboard" => "**/dashboard.html*",
            "wire transfer" => "**/transfer.html*",
            "settings" => "**/settings.html*",
            _ => "**/index.html*"
        };

        // 1. Establish network listener to prevent deadlocks
        var navTask = Page.WaitForURLAsync(expectedEndpoint, new PageWaitForURLOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // 2. Click without waiting
        await RootLocator.GetByRole(AriaRole.Link, new() { NameRegex = new Regex(destination, RegexOptions.IgnoreCase) })
                         .ClickAsync(new LocatorClickOptions { NoWaitAfter = true });

        // 3. Resolve the route instantly
        await navTask;
    }
}