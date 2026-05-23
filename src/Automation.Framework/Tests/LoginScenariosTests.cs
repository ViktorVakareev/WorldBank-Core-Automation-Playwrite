using System.Text.RegularExpressions;
using Automation.Framework.Actions;
using Automation.Framework.Data;
using Automation.Framework.Infrastructure;

namespace Automation.Framework.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class LoginScenariosTests : AiTriage
{
    [Test]
    public async Task Auth_ValidCredentials_ShouldRouteToSecureDashboard()
    {
        // Arrange
        var validUser = DataFactory.GetUser("StandardUser");

        // Act
        await Page.LoginToWorldBankAsync(validUser.Username, validUser.Password);

        // Assert: Playwright natively waits for the URL to change
        await Expect(Page).ToHaveURLAsync(new Regex(".*dashboard\\.html"));
        await Expect(Page.GetByText("Welcome, StandardUser")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Auth_InvalidCredentials_ShouldDisplayGenericSecurityError()
    {
        // Arrange: Use Bogus to generate guaranteed invalid payloads
        var invalidUsername = DataFactory.GenerateRecipientName().Replace(" ", "_");
        var invalidPassword = DataFactory.GenerateRandomAmount();

        // Act
        await Page.LoginToWorldBankAsync(invalidUsername, invalidPassword);

        // Assert: Search the accessibility tree for the text directly, ignoring brittle CSS classes
        var errorBanner = Page.GetByText(new Regex("Invalid credentials|Authentication failed", RegexOptions.IgnoreCase));

        await Expect(errorBanner).ToBeVisibleAsync();
    }
}