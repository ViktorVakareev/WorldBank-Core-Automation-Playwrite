using System.Threading.Tasks;
using Automation.Framework.Actions;
using Automation.Framework.Infrastructure;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Automation.Framework.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class SecurityScenariosTests : AiTriage
{
    [Test]
    public async Task Auth_SqlInjectionAttempt_ShouldBeInterceptedByWaf()
    {
        // Arrange: Classic SQL Injection payload identified in the JS Source
        var maliciousPayload = "' OR 1=1;--";

        // Act
        await Page.LoginToWorldBankAsync(maliciousPayload, "hacked_password");

        // Assert: Verify the JS block intercepts and displays the security warning
        var errorBanner = Page.Locator("#error-message");

        await Expect(errorBanner).ToBeVisibleAsync();
        await Expect(errorBanner).ToHaveTextAsync("Security Violation: Invalid Input Detected");
    }

    [Test]
    public async Task Auth_CrossSiteScriptingAttempt_ShouldBeInterceptedByWaf()
    {
        // Arrange: Classic XSS payload
        var maliciousPayload = "<script>alert('XSS')</script>";

        // Act
        await Page.LoginToWorldBankAsync(maliciousPayload, "hacked_password");

        // Assert
        var errorBanner = Page.Locator("#error-message");

        await Expect(errorBanner).ToBeVisibleAsync();
        await Expect(errorBanner).ToHaveTextAsync("Security Violation: Invalid Input Detected");
    }

    [Test]
    public async Task Auth_NewDeviceUser_ShouldTriggerMfaChallenge()
    {
        // Arrange & Act: Login with the specific mock user that triggers MFA
        await Page.LoginToWorldBankAsync("newdevice_user", "password123");

        // Assert: The JS dynamically changes the CSS `display` properties.
        // Playwright natively checks computed visibility, making these assertions highly stable.
        var loginForm = Page.Locator("#login-form");
        var mfaSection = Page.Locator("#mfa-section");

        await Expect(loginForm).ToBeHiddenAsync();
        await Expect(mfaSection).ToBeVisibleAsync();
    }

    [Test]
    public async Task Auth_BruteForce_ShouldLockAccountAfterFourAttempts()
    {
        // 1. Arrange: Navigate to the page manually. 
        // We cannot use our reusable Action here because we MUST NOT reload the page between attempts.
        await Page.GotoAsync("login.html", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        var userField = Page.Locator("#username");
        var passField = Page.Locator("#password");
        var submitBtn = Page.Locator("#login-form button[type='submit']");
        var errorBanner = Page.Locator("#error-message");

        // 2. Act: Loop 4 times to exhaust the failedAttempts counter
        for (int i = 1; i <= 4; i++)
        {
            await userField.FillAsync($"hacker_attempt_{i}");
            await passField.FillAsync("wrong_password");
            await submitBtn.ClickAsync();

            // Verify standard error for the first 4 attempts
            await Expect(errorBanner).ToHaveTextAsync("Invalid credentials. Generic error.");
        }

        // 3. Act: The 5th attempt (failedAttempts is now 4)
        await userField.FillAsync("hacker_final_attempt");
        await passField.FillAsync("wrong_password");
        await submitBtn.ClickAsync();

        // 4. Assert: The application should lock the user out
        await Expect(errorBanner).ToBeVisibleAsync();
        await Expect(errorBanner).ToHaveTextAsync("Account Locked due to too many failed attempts.");
    }
}