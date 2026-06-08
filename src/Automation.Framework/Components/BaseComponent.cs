using Microsoft.Playwright;

namespace Automation.Framework.Components;

public abstract class BaseComponent
{
    // The root element of this specific component (e.g., the <nav> tag)
    protected readonly ILocator RootLocator;
    protected readonly IPage Page;

    protected BaseComponent(IPage page, ILocator rootLocator)
    {
        Page = page;
        RootLocator = rootLocator;
    }

    // A fast, synchronous check to ensure the component is present in the DOM
    public async Task<bool> IsDisplayedAsync()
    {
        return await RootLocator.IsVisibleAsync();
    }
}