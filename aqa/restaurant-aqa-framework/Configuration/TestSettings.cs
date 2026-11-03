using DotNetEnv;

namespace RestaurantAqaFramework.Configuration;

public class TestSettings
{
    private static TestSettings? _current;
    public static TestSettings Current => _current ??= Load();

    public string UiBaseUrl { get; set; } = "https://d3bnmono898o6h.cloudfront.net";
    public string ApiBaseUrl { get; set; } = "https://fiw10fpnv9.execute-api.eu-north-1.amazonaws.com/develop";
    public string Browser { get; set; } = "chromium";
    public bool Headless { get; set; } = true;
    public string TraceMode { get; set; } = "on-failure";
    public int DefaultTimeoutMs { get; set; } = 30000;

    public string OwnerEmail { get; set; } = "owner@example.com";
    public string OwnerPassword { get; set; } = "Password123!";
    public string CustomerEmail { get; set; } = "customer@example.com";
    public string CustomerPassword { get; set; } = "Password123!";
    public string WaiterEmail { get; set; } = "waiter@example.com";
    public string WaiterPassword { get; set; } = "Password123!";

    public bool ReportPortalEnabled { get; set; } = false;
    public string ReportPortalServerUrl { get; set; } = "http://localhost:8080/api/v1";
    public string ReportPortalApiKey { get; set; } = "";
    public string ReportPortalProjectName { get; set; } = "green_tasty_aqa";

    private static TestSettings Load()
    {
        try
        {
            Env.TraversePath().Load();
        }
        catch
        {
            // Ignore if .env is missing, fallback to environment variables
        }

        return new TestSettings
        {
            UiBaseUrl = Environment.GetEnvironmentVariable("UI_BASE_URL") ?? "https://d3bnmono898o6h.cloudfront.net",
            ApiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "https://fiw10fpnv9.execute-api.eu-north-1.amazonaws.com/develop",
            Browser = Environment.GetEnvironmentVariable("BROWSER") ?? "chromium",
            Headless = bool.TryParse(Environment.GetEnvironmentVariable("HEADLESS"), out var h) ? h : true,
            TraceMode = Environment.GetEnvironmentVariable("TRACE_MODE") ?? "on-failure",
            DefaultTimeoutMs = int.TryParse(Environment.GetEnvironmentVariable("DEFAULT_TIMEOUT_MS"), out var t) ? t : 30000,

            OwnerEmail = Environment.GetEnvironmentVariable("OWNER_EMAIL") ?? "owner@example.com",
            OwnerPassword = Environment.GetEnvironmentVariable("OWNER_PASSWORD") ?? "Password123!",
            CustomerEmail = Environment.GetEnvironmentVariable("CUSTOMER_EMAIL") ?? "customer@example.com",
            CustomerPassword = Environment.GetEnvironmentVariable("CUSTOMER_PASSWORD") ?? "Password123!",
            WaiterEmail = Environment.GetEnvironmentVariable("WAITER_EMAIL") ?? "waiter@example.com",
            WaiterPassword = Environment.GetEnvironmentVariable("WAITER_PASSWORD") ?? "Password123!",

            ReportPortalEnabled = bool.TryParse(Environment.GetEnvironmentVariable("REPORTPORTAL_ENABLED"), out var rp) && rp,
            ReportPortalServerUrl = Environment.GetEnvironmentVariable("REPORTPORTAL_SERVER_URL") ?? "http://localhost:8080/api/v1",
            ReportPortalApiKey = Environment.GetEnvironmentVariable("REPORTPORTAL_API_KEY") ?? "",
            ReportPortalProjectName = Environment.GetEnvironmentVariable("REPORTPORTAL_PROJECT_NAME") ?? "green_tasty_aqa"
        };
    }
}
