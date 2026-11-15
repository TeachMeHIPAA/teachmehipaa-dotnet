// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="LearnerService.BatchUpsertAsync"/>.</summary>
public sealed class LearnerBatchUpsertOptions
{
    /// <summary>Up to 500 learners, each with the same fields as creating one learner. Required.</summary>
    [JsonPropertyName("items")]
    public IReadOnlyList<LearnerBatchUpsertRequestItemsItem> Items { get; set; } = [];

    /// <summary>Applies to every item. <c>email</c> (default) sends invites; <c>none</c> sends nothing.</summary>
    [JsonPropertyName("delivery")]
    public LearnerBatchUpsertRequestDelivery? Delivery { get; set; }

    /// <summary>Applies to every item. <c>require_available</c> fails an item when no seat is free.</summary>
    [JsonPropertyName("seatPolicy")]
    public LearnerBatchUpsertRequestSeatPolicy? SeatPolicy { get; set; }
}
