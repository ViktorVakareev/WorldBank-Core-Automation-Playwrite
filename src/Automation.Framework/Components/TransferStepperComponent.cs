using Microsoft.Playwright;

namespace Automation.Framework.Components;

public class TransferStepperComponent
{
    private readonly IPage _page;

    public TransferStepperComponent(IPage page) => _page = page;

    // Step 1 Inputs
    public ILocator RecipientInput => _page.GetByPlaceholder("Recipient Name");
    public ILocator AccountInput => _page.GetByPlaceholder("Account Number");
    public ILocator AmountInput => _page.GetByPlaceholder("Amount");

    // Navigation
    public ILocator ContinueBtn => _page.GetByRole(AriaRole.Button, new() { Name = "Continue" });
    public ILocator BackBtn => _page.GetByRole(AriaRole.Button, new() { Name = "Back" });

    public async Task CompleteStepOneAsync(string recipient, string account, string amount)
    {
        await RecipientInput.FillAsync(recipient);
        await AccountInput.FillAsync(account);
        await AmountInput.FillAsync(amount);
        await ContinueBtn.ClickAsync();
    }
}