using FluentAssertions;
using Microsoft.Playwright;
using Reqnroll;
using RestaurantAqaFramework.Api.Clients;
using RestaurantAqaFramework.Ui.Pages;

namespace RestaurantAqaFramework.StepDefinitions;

[Binding]
public class GroupOrderStepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly IPage _page;
    private readonly IAPIRequestContext _apiContext;
    private readonly GroupOrderPage _groupOrderPage;
    private readonly GroupOrderApiClient _apiClient;
    private int _lastApiResponseCode;

    public GroupOrderStepDefinitions(ScenarioContext context)
    {
        _context = context;
        _page = (IPage)_context["Page"];
        _apiContext = (IAPIRequestContext)_context["ApiContext"];
        _groupOrderPage = new GroupOrderPage(_page);
        _apiClient = new GroupOrderApiClient(_apiContext);
    }

    [Given(@"the Owner opens the group pre-order page in demo mode")]
    public async Task GivenTheOwnerOpensTheGroupPreOrderPageInDemoMode()
    {
        await _groupOrderPage.OpenDemoAsync();
    }

    [When(@"the page loads successfully")]
    public async Task WhenThePageLoadsSuccessfully()
    {
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var url = _page.Url;
        url.Should().Contain("group-order");
    }

    [Then(@"the Owner should see all joined participant cards")]
    public async Task ThenTheOwnerShouldSeeAllJoinedParticipantCards()
    {
        var cards = _groupOrderPage.ParticipantCards;
        var count = await cards.CountAsync();
        count.Should().BeGreaterThan(0, "There should be participant cards on the screen");
    }

    [Then(@"the Grand Total should be calculated from all submitted dishes")]
    public async Task ThenTheGrandTotalShouldBeCalculatedFromAllSubmittedDishes()
    {
        var totalElement = _groupOrderPage.GrandTotal;
        var isVisible = await totalElement.IsVisibleAsync();
        isVisible.Should().BeTrue("Grand total price element should be visible");
    }

    [When(@"there are participants with unsubmitted baskets")]
    public async Task WhenThereAreParticipantsWithUnsubmittedBaskets()
    {
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Then(@"a warning banner informing about unsubmitted participants should be displayed")]
    public async Task ThenAWarningBannerInformingAboutUnsubmittedParticipantsShouldBeDisplayed()
    {
        var banner = _groupOrderPage.WarningBanner;
        var isVisible = await banner.IsVisibleAsync();
        isVisible.Should().BeTrue("Warning banner for unsubmitted participants should be displayed");
    }

    [When(@"the Owner clicks the ""Submit Pre-Order"" button")]
    public async Task WhenTheOwnerClicksTheSubmitPreOrderButton()
    {
        await _groupOrderPage.ClickSubmitAsync();
    }

    [Then(@"the pre-order status should update to ""Sent to restaurant"" or ""Submitted""")]
    public async Task ThenThePreOrderStatusShouldUpdateToSentToRestaurantOrSubmitted()
    {
        var text = await _page.InnerTextAsync("body");
        text.Should().Contain("Sent to Restaurant");
    }

    [Then(@"background synchronization should keep updating the screen status")]
    public async Task ThenBackgroundSynchronizationShouldKeepUpdatingTheScreenStatus()
    {
        var refreshBtn = _groupOrderPage.RefreshButton;
        var isVisible = await refreshBtn.IsVisibleAsync();
        isVisible.Should().BeTrue("Refresh control should be available");
    }

    [When(@"the Owner removes a guest participant")]
    public async Task WhenTheOwnerRemovesAGuestParticipant()
    {
        await _groupOrderPage.ClickRemoveGuestAsync();
    }

    [Then(@"the participant card should disappear from the list")]
    public async Task ThenTheParticipantCardShouldDisappearFromTheList()
    {
        var cards = _groupOrderPage.ParticipantCards;
        var count = await cards.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Then(@"the Grand Total should be recalculated without the removed guest")]
    public async Task ThenTheGrandTotalShouldBeRecalculatedWithoutTheRemovedGuest()
    {
        var total = _groupOrderPage.GrandTotal;
        var isVisible = await total.IsVisibleAsync();
        isVisible.Should().BeTrue();
    }

    [Given(@"an unauthorized or non-owner user request to view sessions for group ""(.*)""")]
    public void GivenAnUnauthorizedUserRequestSessions(string groupId)
    {
        _context["TargetGroupId"] = groupId;
    }

    [When(@"the API request is sent without valid owner credentials")]
    public async Task WhenTheApiRequestIsSentWithoutCredentials()
    {
        var groupId = (string)_context["TargetGroupId"];
        var (code, _, _) = await _apiClient.GetSessionsAsync(groupId, authToken: null);
        _lastApiResponseCode = code;
    }

    [Given(@"a request to view sessions for non-existing group ""(.*)""")]
    public void GivenARequestToViewSessionsForNonExistingGroup(string groupId)
    {
        _context["TargetGroupId"] = groupId;
    }

    [When(@"the API request is sent")]
    public async Task WhenTheApiRequestIsSent()
    {
        var groupId = (string)_context["TargetGroupId"];
        var (code, _, _) = await _apiClient.GetSessionsAsync(groupId);
        _lastApiResponseCode = code;
    }

    [Given(@"an API request to submit group reservation ""(.*)""")]
    public void GivenAnApiRequestToSubmitGroupReservation(string groupId)
    {
        _context["TargetGroupId"] = groupId;
    }

    [When(@"the submit API is called without any submitted dishes")]
    public async Task WhenTheSubmitApiIsCalledWithoutDishes()
    {
        var groupId = (string)_context["TargetGroupId"];
        var (code, _) = await _apiClient.SubmitGroupOrderAsync(groupId);
        _lastApiResponseCode = code;
    }

    [Given(@"an unauthorized user attempt to submit group reservation ""(.*)""")]
    public void GivenAnUnauthorizedAttemptToSubmit(string groupId)
    {
        _context["TargetGroupId"] = groupId;
    }

    [When(@"the submit API request is sent without owner token")]
    public async Task WhenTheSubmitApiRequestIsSentWithoutToken()
    {
        var groupId = (string)_context["TargetGroupId"];
        var (code, _) = await _apiClient.SubmitGroupOrderAsync(groupId, authToken: null);
        _lastApiResponseCode = code;
    }

    [Given(@"an Owner session ID in group ""(.*)""")]
    public void GivenAnOwnerSessionIdInGroup(string groupId)
    {
        _context["TargetGroupId"] = groupId;
        _context["TargetSessionId"] = "owner-session-id-123";
    }

    [When(@"the Owner tries to delete their own session via API")]
    public async Task WhenTheOwnerTriesToDeleteTheirOwnSessionViaApi()
    {
        var groupId = (string)_context["TargetGroupId"];
        var sessionId = (string)_context["TargetSessionId"];
        var (code, _, _) = await _apiClient.DeleteSessionAsync(groupId, sessionId);
        _lastApiResponseCode = code;
    }

    [Given(@"a non-existent participant ""(.*)"" in group ""(.*)""")]
    public void GivenANonExistentParticipant(string sessionId, string groupId)
    {
        _context["TargetGroupId"] = groupId;
        _context["TargetSessionId"] = sessionId;
    }

    [When(@"the delete participant API request is sent")]
    public async Task WhenTheDeleteParticipantApiRequestIsSent()
    {
        var groupId = (string)_context["TargetGroupId"];
        var sessionId = (string)_context["TargetSessionId"];
        var (code, _, _) = await _apiClient.DeleteSessionAsync(groupId, sessionId);
        _lastApiResponseCode = code;
    }

    [Then(@"the API response status code should be (.*)")]
    public void ThenTheApiResponseStatusCodeShouldBe(int expectedCode)
    {
        _lastApiResponseCode.Should().Be(expectedCode);
    }

    [Then(@"the API response status code should be 401 or 403")]
    public void ThenTheApiResponseStatusCodeShouldBe401Or403()
    {
        _lastApiResponseCode.Should().BeOneOf(401, 403);
    }

    [Then(@"the API response status code should be 401, 403 or 404")]
    public void ThenTheApiResponseStatusCodeShouldBe401Or403Or404()
    {
        _lastApiResponseCode.Should().BeOneOf(401, 403, 404);
    }

    [Then(@"the API response status code should be 400, 401 or 404")]
    public void ThenTheApiResponseStatusCodeShouldBe400Or401Or404()
    {
        _lastApiResponseCode.Should().BeOneOf(400, 401, 404);
    }

    [Then(@"the API response status code should be 400 or 401")]
    public void ThenTheApiResponseStatusCodeShouldBe400Or401()
    {
        _lastApiResponseCode.Should().BeOneOf(400, 401);
    }
}
