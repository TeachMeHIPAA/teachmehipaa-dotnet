using System.Diagnostics.CodeAnalysis;

namespace TeachMeHipaa;

/// <summary>
/// A verified Delivery: either an <see cref="TeachMeHipaa.Event"/> or, when
/// someone asked TeachMeHIPAA to test the endpoint, a
/// <see cref="TeachMeHipaa.TestDelivery"/>, which carries no Event.
/// </summary>
/// <example>
/// <code>
/// switch (delivery)
/// {
///     case { TestDelivery: not null }:
///         return Results.Ok();
///     case { Event: EventLearnerCertified certified }:
///         await RecordCertificationAsync(certified.Data.LearnerId);
///         break;
/// }
/// </code>
/// </example>
public sealed class WebhookDelivery
{
    internal WebhookDelivery(Event @event) => Event = @event;

    internal WebhookDelivery(TestDelivery testDelivery) => TestDelivery = testDelivery;

    /// <summary>
    /// The Event, or null for a Test delivery. Match it against the
    /// <c>Event*</c> types; an Event type this version doesn't know is an
    /// <see cref="UnknownEvent"/>.
    /// </summary>
    public Event? Event { get; }

    /// <summary>The Test delivery, or null when this Delivery carries an Event.</summary>
    public TestDelivery? TestDelivery { get; }

    /// <summary>Whether this is a Test delivery rather than an Event.</summary>
    [MemberNotNullWhen(true, nameof(TestDelivery))]
    [MemberNotNullWhen(false, nameof(Event))]
    public bool IsTestDelivery => TestDelivery is not null;
}
