using System.Text.Json;
using Microsoft.Playwright;
using RestaurantAqaFramework.Api.Models;
using RestaurantAqaFramework.Configuration;

namespace RestaurantAqaFramework.Api.Clients;

public class GroupOrderApiClient
{
    private readonly IAPIRequestContext _request;
    private readonly string _baseUrl;

    public GroupOrderApiClient(IAPIRequestContext request)
    {
        _request = request;
        _baseUrl = TestSettings.Current.ApiBaseUrl.TrimEnd('/');
    }

    public async Task<(int StatusCode, GetAllSessionsResponse? Data, string RawBody)> GetSessionsAsync(string groupReservationId, string? authToken = null)
    {
        var headers = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(authToken))
        {
            headers["Authorization"] = $"Bearer {authToken}";
        }

        var response = await _request.GetAsync($"{_baseUrl}/guest/groups/{groupReservationId}/sessions", new()
        {
            Headers = headers
        });

        var raw = await response.TextAsync();
        GetAllSessionsResponse? data = null;
        if (response.Ok)
        {
            data = JsonSerializer.Deserialize<GetAllSessionsResponse>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        return (response.Status, data, raw);
    }

    public async Task<(int StatusCode, OwnerSessionIdResponse? Data, string RawBody)> GetOwnerSessionIdAsync(string reservationId, string? authToken = null)
    {
        var headers = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(authToken))
        {
            headers["Authorization"] = $"Bearer {authToken}";
        }

        var response = await _request.GetAsync($"{_baseUrl}/reservations/{reservationId}/guest-session-id", new()
        {
            Headers = headers
        });

        var raw = await response.TextAsync();
        OwnerSessionIdResponse? data = null;
        if (response.Ok)
        {
            data = JsonSerializer.Deserialize<OwnerSessionIdResponse>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        return (response.Status, data, raw);
    }

    public async Task<(int StatusCode, string RawBody)> SubmitGroupOrderAsync(string groupReservationId, string? authToken = null)
    {
        var headers = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(authToken))
        {
            headers["Authorization"] = $"Bearer {authToken}";
        }

        var response = await _request.PostAsync($"{_baseUrl}/group-reservations/{groupReservationId}/submit", new()
        {
            Headers = headers
        });

        var raw = await response.TextAsync();
        return (response.Status, raw);
    }

    public async Task<(int StatusCode, GetAllSessionsResponse? Data, string RawBody)> DeleteSessionAsync(string groupReservationId, string guestSessionId, string? authToken = null, string? guestToken = null)
    {
        var headers = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(authToken))
        {
            headers["Authorization"] = $"Bearer {authToken}";
        }
        if (!string.IsNullOrEmpty(guestToken))
        {
            headers["X-Guest-Token"] = guestToken;
        }

        var response = await _request.DeleteAsync($"{_baseUrl}/guest/groups/{groupReservationId}/sessions/{guestSessionId}", new()
        {
            Headers = headers
        });

        var raw = await response.TextAsync();
        GetAllSessionsResponse? data = null;
        if (response.Ok)
        {
            data = JsonSerializer.Deserialize<GetAllSessionsResponse>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        return (response.Status, data, raw);
    }
}
