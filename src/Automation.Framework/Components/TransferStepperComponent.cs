using Microsoft.Playwright;
using ReportPortal.Client.Abstractions.Responses;
using System;
using System.Threading.Tasks;

namespace Automation.Framework.Components;

public class TransferStepperComponent : BaseComponent
{
    // Scope strictly to the transfer form container
    public TransferStepperComponent(IPage page)
        : base(page, page.Locator(".transfer-container"))
    {
    }

    // Step 1 Locators
    public ILocator RecipientDropdown => RootLocator.GetByTestId("recipient-select");
    public ILocator AccountInput => RootLocator.GetByTestId("acc-number");
    public ILocator Step1NextBtn => RootLocator.GetByTestId("btn-next-1");

    // Step 2 Locators
    public ILocator DateInput => RootLocator.GetByTestId("transfer-date");
    public ILocator AmountInput => RootLocator.GetByTestId("transfer-amount");
    public ILocator Step2NextBtn => RootLocator.GetByTestId("btn-next-2");

    // Step 3 (Review) Locators
    public ILocator ReviewAccount => RootLocator.GetByTestId("review-acc");
    public ILocator ReviewAmount => RootLocator.GetByTestId("review-amount");
    public ILocator SubmitTransferBtn => RootLocator.GetByTestId("btn-submit-transfer");

    // Success Modal
    public ILocator SuccessMessage => Page.GetByTestId("success-msg"); // Sits outside the container in the DOM

    public async Task CompleteStepOneAsync(string account)
    {
        // Select the first valid option from the dropdown
        await RecipientDropdown.SelectOptionAsync(new[] { new SelectOptionValue { Index = 1 } });
        await AccountInput.FillAsync(account);
        await Step1NextBtn.ClickAsync();
    }

    public async Task CompleteStepTwoAsync(string amount)
    {
        // Dynamically inject today's date to pass JS validation
        await DateInput.FillAsync(DateTime.Now.ToString("yyyy-MM-dd"));
        await AmountInput.FillAsync(amount);
        await Step2NextBtn.ClickAsync();
    }

    public async Task SubmitTransferAsync()
    {
        await SubmitTransferBtn.ClickAsync();
    }
}