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
        // 🚀 DYNAMIC DATA FIX: We bypass the generic IBAN generator to force 
        // a strict 22-digit numeric string to satisfy the app's JS validation.
        // (Note: If your GitHub Pages hasn't updated yet, change 22 to 10!)
        var bogus = new Bogus.Faker();

        yield return new TestCaseData(
            DataFactory.GenerateRecipientName(),
            bogus.Random.String2(22, "0123456789"), // Generates exactly 22 random digits
            DataFactory.GenerateRandomAmount(100, 5000)
        ).SetName("Transfer_Valid_StandardAmount");

        yield return new TestCaseData(
            DataFactory.GenerateRecipientName(),
            bogus.Random.String2(22, "0123456789"),
            "999999.99"
        ).SetName("Transfer_Valid_HighValueEdgeCase");
    }

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
        // Act: Execute the 3-step action we built
        await Page.ExecuteWireTransferStepsAsync(recipient, account, amount);

        // 🎯 ASSERT: Verify UI state shifts successfully to Step 3 (Review)
        var reviewSection = Page.GetByTestId("review-acc");
        await Expect(reviewSection).ToBeVisibleAsync();

        // Ensure the exact dynamic account string was persisted through the DOM
        await Expect(reviewSection).ToHaveTextAsync(account);
    }
}