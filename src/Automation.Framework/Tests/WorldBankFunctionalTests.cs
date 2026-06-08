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
        var account = "1234567890123456789012";
        var amount = "1500.00";

        var stepper = new TransferStepperComponent(Page);
        var nav = new GlobalNavigationComponent(Page);

        // Act
        await stepper.CompleteStepOneAsync(account);
        await stepper.CompleteStepTwoAsync(amount);

        // Assert Review State
        await Expect(stepper.ReviewAccount).ToHaveTextAsync(account);
        await Expect(stepper.ReviewAmount).ToContainTextAsync(amount);

        // Act
        await stepper.SubmitTransferAsync();

        // Assert Success
        await Expect(stepper.SuccessMessage).ToBeVisibleAsync();

        // Safely return home
        await nav.NavigateToAsync("dashboard");
    }
}