using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Automation.Framework.Actions;

public static class TransferActions
{
    public static async Task ExecuteWireTransferStepsAsync(this IPage page, string recipient, string account, string amount)
    {
        // Wait for the form to be ready
        var formContainer = page.Locator("#transfer-form-container, .transfer-form");
        await formContainer.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        // Fill out Step 1
        await page.Locator("#recipient-name, [placeholder*='Recipient Name']").FillAsync(recipient);
        await page.Locator("#account-number, [placeholder*='Account']").FillAsync(account);
        await page.Locator("#transfer-amount, [placeholder*='Amount']").FillAsync(amount);

        // Advance the stepper
        await page.Locator("#btn-continue-step, button:has-text('Continue')").ClickAsync();
    }
}