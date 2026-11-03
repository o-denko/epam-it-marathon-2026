using Microsoft.Playwright;

namespace RestaurantAqaFramework.Ui.Pages;

public class GroupOrderPage
{
    private readonly IPage _page;

    public GroupOrderPage(IPage page)
    {
        _page = page;
    }

    public ILocator PageTitle => _page.Locator("h1").Filter(new() { HasText = "Group Pre-Order" });
    public ILocator WarningBanner => _page.Locator("p").Filter(new() { HasText = "haven't submitted their cart yet" });
    public ILocator GrandTotal => _page.Locator("body").Filter(new() { HasText = "Grand Total" });
    public ILocator SubmitButton => _page.GetByRole(AriaRole.Button, new() { Name = "Submit Pre-Order" });
    public ILocator RefreshButton => _page.GetByRole(AriaRole.Button, new() { Name = "Refresh" });
    public ILocator LastUpdatedText => _page.Locator("body").Filter(new() { HasText = "Updated at" });
    public ILocator StatusBadge => _page.Locator(".status-badge, p").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Collecting pre-orders|Sent to Restaurant", System.Text.RegularExpressions.RegexOptions.IgnoreCase) });

    public ILocator ParticipantCards => _page.Locator("app-participant-card");
    public ILocator ConfirmModal => _page.Locator("app-modal");
    public ILocator ConfirmModalSubmitButton => ConfirmModal.Locator("button").Filter(new() { HasText = "Confirm" });
    public ILocator RemoveGuestButtons => _page.Locator("button[aria-label*='Remove'], button:has-text('Remove Guest')");

    public async Task OpenDemoAsync(string reservationId = "demo-reservation")
    {
        await _page.GotoAsync($"/reservations/{reservationId}/group-order?demo");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.WaitForTimeoutAsync(1500);
    }

    public async Task OpenAsync(string reservationId)
    {
        await _page.GotoAsync($"/reservations/{reservationId}/group-order");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.WaitForTimeoutAsync(1500);
    }

    public async Task ClickSubmitAsync()
    {
        await SubmitButton.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
        var submitInModal = _page.Locator("app-modal button").Filter(new() { HasText = "Submit" });
        if (await submitInModal.IsVisibleAsync())
        {
            await submitInModal.ClickAsync();
        }
        await _page.WaitForTimeoutAsync(1500);
    }

    public async Task ClickRemoveGuestAsync(string? guestName = null)
    {
        ILocator removeBtn;
        if (!string.IsNullOrEmpty(guestName))
        {
            removeBtn = _page.Locator($"button[aria-label='Remove {guestName}'], button:has-text('Remove Guest')").First;
        }
        else
        {
            removeBtn = RemoveGuestButtons.First;
        }

        await removeBtn.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
        if (await ConfirmModal.IsVisibleAsync())
        {
            await ConfirmModal.Locator("button").Filter(new() { HasText = "Confirm" }).ClickAsync();
        }
        await _page.WaitForTimeoutAsync(1000);
    }
}
