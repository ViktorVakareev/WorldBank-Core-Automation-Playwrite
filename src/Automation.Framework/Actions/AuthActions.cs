using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Automation.Framework.Actions;

public static class AuthActions
{
    public static async Task LoginToWorldBankAsync(this IPage page, string username, string password)
    {
        // Wait for the HTML DOM to load to prevent network timeouts
        await page.GotoAsync("login.html", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // 🛡️ ARIA-Hardened Locators: Matched exactly to your provided application snapshot
        var userField = page.GetByRole(AriaRole.Textbox, new() { NameRegex = new Regex("Corporate ID|Username", RegexOptions.IgnoreCase) })
                            .Or(page.Locator("[placeholder*='Username']"));

        var passField = page.GetByRole(AriaRole.Textbox, new() { NameRegex = new Regex("Security Token|Password", RegexOptions.IgnoreCase) })
                            .Or(page.Locator("[placeholder*='Password']"));

        // 🚀 Reactivity Fix: Clear the fields, then simulate human typing to force JS state updates
        await userField.ClearAsync();
        await userField.PressSequentiallyAsync(username, new LocatorPressSequentiallyOptions { Delay = 50 });

        await passField.ClearAsync();
        await passField.PressSequentiallyAsync(password, new LocatorPressSequentiallyOptions { Delay = 50 });

        // 🎯 Click the explicit submit button identified in the Aria snapshot
        await page.GetByTestId("btn-login-submit")
                  .Or(page.Locator("button[type='submit']"))
                  .Or(page.Locator("button:has-text('Secure Login')"))
                  .ClickAsync();
    }

    public static async Task LogoutAsync(this IPage page)
    {
        await page.GetByRole(AriaRole.Link, new() { NameRegex = new Regex("Logout|Sign Out", RegexOptions.IgnoreCase) })
                  .Or(page.Locator("button:has-text('Logout')"))
                  .ClickAsync();
    }
}