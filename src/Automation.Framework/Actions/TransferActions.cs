using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Automation.Framework.Actions;

public static class TransferActions
{
    public static async Task ExecuteWireTransferStepsAsync(this IPage page, string recipient, string account, string amount)
    {
        /* ==========================================
           STEP 1: DETAILS
           ========================================== */
        // Use the exact data-testid from the HTML source
        await page.GetByTestId("recipient-select").SelectOptionAsync(new[] { new SelectOptionValue { Index = 1 } });

        // Use the correct 'acc-number' data-testid
        await page.GetByTestId("acc-number").FillAsync(account);

        // Click the exact Next button for Step 1
        await page.GetByTestId("btn-next-1").ClickAsync();

        /* ==========================================
           STEP 2: AMOUNT & DATE
           ========================================== */
        // We must fill the mandatory Date field with today's date to pass JS validation
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        await page.GetByTestId("transfer-date").FillAsync(today);

        // Fill the amount
        await page.GetByTestId("transfer-amount").FillAsync(amount);

        // Advance to Review
        await page.GetByTestId("btn-next-2").ClickAsync();
    }
}