using System.Text.Json.Serialization;

namespace RestaurantAqaFramework.Api.Models;

public class GetAllSessionsResponse
{
    [JsonPropertyName("participants")]
    public List<ParticipantDto> Participants { get; set; } = new();

    [JsonPropertyName("total")]
    public decimal Total { get; set; }
}

public class ParticipantDto
{
    [JsonPropertyName("guestSessionId")]
    public string GuestSessionId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("dishes")]
    public List<ParticipantDishDto> Dishes { get; set; } = new();

    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; set; }
}

public class ParticipantDishDto
{
    [JsonPropertyName("dishId")]
    public string DishId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }
}

public class OwnerSessionIdResponse
{
    [JsonPropertyName("guestSessionId")]
    public string GuestSessionId { get; set; } = string.Empty;
}

public class SubmitGroupOrderResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("orderId")]
    public string? OrderId { get; set; }
}
