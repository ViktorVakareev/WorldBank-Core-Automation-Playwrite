using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Automation.Framework.Actions;
using Automation.Framework.Components;
using Automation.Framework.Data;
using Automation.Framework.Infrastructure;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Automation.Framework.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class WorldBankFunctionalTests : AiTriage
{
    // 🛡️ NUNIT SETUP: Automatically prepares the browser state before the test executes
    [SetUp]
    public async Task SetupFunctionalTestStateAsync()
    {
        // 1. Authenticate
        var user = DataFactory.GetUser("StandardUser");
        await Page.LoginToWorldBankAsync(user.Username, user.Password);

        // 2. Synchronization Barrier (Wait for routing to resolve)
        await Expect(Page).ToHaveURLAsync(new Regex(".*dashboard\\.html"));

        // 3. Navigate to the starting point of the flow
        var navigation = new GlobalNavigationComponent(Page);
        await navigation.NavigateToAsync("wire transfer");
    }

    [Test]
    public async Task EndToEnd_ExecuteInternationalWire_ShouldGenerateReceipt()
    {
        // 1. Arrange
        var bogusRecipient = DataFactory.GenerateRecipientName();

        // 🚀 We fixed the app! The JS validation now correctly accepts the 22-digit requirement.
        var account = "1234567890123456789012";

        var amount = DataFactory.GenerateRandomAmount(500, 1500);

        // 2. Act: Fill Step 1 and Step 2
        await Page.ExecuteWireTransferStepsAsync(bogusRecipient, account, amount);

        // 3. Assert: Verify the Review screen populated correctly before submission
        await Expect(Page.GetByTestId("review-acc")).ToHaveTextAsync(account);
        await Expect(Page.GetByTestId("review-amount")).ToContainTextAsync(amount);

        // 4. Act: Confirm Transfer on Step 3
        await Page.GetByTestId("btn-submit-transfer").ClickAsync();

        // 5. Assert: Verify the exact success message element appears
        var successMessage = Page.GetByTestId("success-msg");
        await Expect(successMessage).ToBeVisibleAsync();
        await Expect(successMessage).ToContainTextAsync("Transfer Submitted Successfully!");

        // 6. Return to Dashboard (Using the universal header link)
        await Page.GetByTestId("nav-dashboard").ClickAsync();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*dashboard\\.html"));
    }
}