using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIHandler.Models.GuestSessions;

public class SubmitGroupPreOrderRequest
{
    [Required]
    [JsonPropertyName("reservationId")]
    public string ReservationId { get; set; } = string.Empty;
}

public record SubmittedDishResponse(
    [property: JsonPropertyName("dishId")] string DishId,
    [property: JsonPropertyName("quantity")] int Quantity);

