using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Automation.Framework.Actions;

public static class AuthActions
{
    public static async Task LoginToWorldBankAsync(this IPage page, string username, string password)
    {
        // Playwright natively appends this to your BaseURL from AiTriage.cs
        await page.GotoAsync("login.html");

        // 🚨 LOCATOR ROT DEFENSE: Using multiple fallback selectors (CSS + Attribute)
        // If the Mock App changes placeholders again, Playwright will check the ID first.
        await page.Locator("#username, [placeholder*='Username'], [placeholder*='User ID']").FillAsync(username);
        await page.Locator("#password, [placeholder*='Password']").FillAsync(password);

        await page.Locator("#login-btn, button:has-text('Login'), button:has-text('Sign In')").ClickAsync();
    }

    public static async Task LogoutAsync(this IPage page)
    {
        await page.Locator("button:has-text('Sign Out'), button:has-text('Logout')").ClickAsync();
    }
}