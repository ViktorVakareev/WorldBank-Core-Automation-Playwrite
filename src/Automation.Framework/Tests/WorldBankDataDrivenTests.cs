using System.Collections.Generic;
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
public class WorldBankDataDrivenTests : AiTriage
{
    public static IEnumerable<TestCaseData> ValidTransferData()
    {
        yield return new TestCaseData(
            DataFactory.GenerateRecipientName(),
            DataFactory.GenerateValidIban(),
            DataFactory.GenerateRandomAmount(100, 5000)
        ).SetName("Transfer_Valid_StandardAmount");

        yield return new TestCaseData(
            DataFactory.GenerateRecipientName(),
            DataFactory.GenerateValidIban(),
            "999999.99"
        ).SetName("Transfer_Valid_HighValueEdgeCase");
    }

    // 🛡️ NUNIT SETUP: This runs automatically before EVERY test in this class.
    // You write it once, but Playwright executes it safely in isolation.
    [SetUp]
    public async Task SetupWireTransferStateAsync()
    {
        // 1. Authenticate
        var user = DataFactory.GetUser("StandardUser");
        await Page.LoginToWorldBankAsync(user.Username, user.Password);

        // 2. Synchronization Barrier
        await Expect(Page).ToHaveURLAsync(new Regex(".*dashboard\\.html"));

        // 3. Navigate
        var nav = new GlobalNavigationComponent(Page);
        await nav.NavigateToAsync("wire transfer");
    }

    [Test, TestCaseSource(nameof(ValidTransferData))]
    public async Task Transfer_Step1_ValidData_ShouldAdvanceToReview(string recipient, string account, string amount)
    {
        // 🚀 ACT: Look how lean the actual test is now! 
        // The Setup method already handled the login and navigation.
        await Page.ExecuteWireTransferStepsAsync(recipient, account, amount);

        // 🎯 ASSERT: Verify UI state shifts to Step 2
        await Expect(Page.Locator("#step-2-indicator, .review-section")).ToBeVisibleAsync();
        await Expect(Page.Locator(".error-text")).ToBeHiddenAsync();
    }
}