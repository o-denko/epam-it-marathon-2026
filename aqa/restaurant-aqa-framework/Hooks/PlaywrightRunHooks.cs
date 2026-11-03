using Microsoft.Playwright;
using Reqnroll;
using RestaurantAqaFramework.Configuration;

namespace RestaurantAqaFramework.Hooks;

[Binding]
public class PlaywrightRunHooks
{
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;
    private readonly ScenarioContext _scenarioContext;

    public PlaywrightRunHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        _playwright = await Playwright.CreateAsync();
        var settings = TestSettings.Current;
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = settings.Headless
        });
    }

    [BeforeScenario(Order = 1)]
    public async Task BeforeScenario()
    {
        if (_browser == null || _playwright == null) return;

        var settings = TestSettings.Current;
        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = settings.UiBaseUrl,
            IgnoreHTTPSErrors = true
        });

        if (settings.TraceMode == "on" || settings.TraceMode == "on-failure")
        {
            await context.Tracing.StartAsync(new()
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
        }

        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(settings.DefaultTimeoutMs);

        var apiContext = await _playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = settings.ApiBaseUrl,
            IgnoreHTTPSErrors = true
        });

        _scenarioContext["BrowserContext"] = context;
        _scenarioContext["Page"] = page;
        _scenarioContext["ApiContext"] = apiContext;
    }

    [AfterScenario(Order = 100)]
    public async Task AfterScenario()
    {
        var settings = TestSettings.Current;
        if (_scenarioContext.TryGetValue("BrowserContext", out IBrowserContext context))
        {
            var isFailed = _scenarioContext.TestError != null;
            if (isFailed || settings.TraceMode == "on")
            {
                var traceDir = Path.Combine(Directory.GetCurrentDirectory(), "TestResults", "traces");
                Directory.CreateDirectory(traceDir);
                var tracePath = Path.Combine(traceDir, $"{_scenarioContext.ScenarioInfo.Title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
                await context.Tracing.StopAsync(new() { Path = tracePath });

                if (_scenarioContext.TryGetValue("Page", out IPage page))
                {
                    var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "TestResults", "screenshots");
                    Directory.CreateDirectory(screenshotDir);
                    var screenshotPath = Path.Combine(screenshotDir, $"{_scenarioContext.ScenarioInfo.Title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                    await page.ScreenshotAsync(new() { Path = screenshotPath });
                }
            }
            else
            {
                await context.Tracing.StopAsync();
            }

            await context.CloseAsync();
        }

        if (_scenarioContext.TryGetValue("ApiContext", out IAPIRequestContext apiContext))
        {
            await apiContext.DisposeAsync();
        }
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            await _browser.DisposeAsync();
        }
        _playwright?.Dispose();
    }
}
