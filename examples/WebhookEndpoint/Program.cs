// An ASP.NET Core minimal API that verifies TeachMeHIPAA webhook Deliveries
// and handles their Events. It needs the Webhook subscription's Signing
// secret, not API credentials.
//
//   TMH_WEBHOOK_SECRET=... dotnet run --project examples/WebhookEndpoint --urls http://localhost:8000
//
// Then point a Sandbox Webhook subscription at https://<your tunnel>/webhooks/tmh
// and send it a Test delivery.
using TeachMeHipaa;

var builder = WebApplication.CreateBuilder(args);
var signingSecret = builder.Configuration["TMH_WEBHOOK_SECRET"] is { Length: > 0 } secret
    ? secret
    : throw new InvalidOperationException("Set TMH_WEBHOOK_SECRET to the Webhook subscription's Signing secret.");
var app = builder.Build();

app.MapPost("/webhooks/tmh", async (HttpRequest request, ILogger<Program> logger) =>
{
    // The signature covers the exact bytes and path TeachMeHIPAA sent, so
    // read the raw body rather than binding a model, and keep the path base
    // and query string.
    using var body = new MemoryStream();
    await request.Body.CopyToAsync(body, request.HttpContext.RequestAborted);
    WebhookDelivery delivery;
    try
    {
        delivery = WebhookUtility.ConstructEvent(
            body.ToArray(),
            name => request.Headers[name],
            signingSecret,
            $"{request.PathBase}{request.Path}{request.QueryString}");
    }
    catch (WebhookSignatureException error)
    {
        WebhookLog.Rejected(logger, error.Code);
        return Results.BadRequest(error.Code);
    }

    // A Delivery can arrive more than once; key any side effects by EventId.
    switch (delivery.Event)
    {
        case null:
            WebhookLog.TestDelivery(logger, delivery.TestDelivery!.SubscriptionId);
            break;
        case EventLearnerCertified certified:
            WebhookLog.Certified(logger, certified.EventId, certified.Data.Email, certified.Data.CourseId);
            break;
        case EventLearnerExpiring expiring:
            WebhookLog.Expiring(logger, expiring.EventId, expiring.Data.Email);
            break;
        default:
            WebhookLog.Ignored(logger, delivery.Event.EventId);
            break;
    }

    return Results.Ok();
});

app.Run();

/// <summary>The endpoint's log messages, as source-generated <see cref="ILogger"/> calls.</summary>
internal static partial class WebhookLog
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Rejected a Delivery: {Code}")]
    public static partial void Rejected(ILogger logger, string? code);

    [LoggerMessage(Level = LogLevel.Information, Message = "Test delivery for {SubscriptionId}")]
    public static partial void TestDelivery(ILogger logger, string subscriptionId);

    [LoggerMessage(Level = LogLevel.Information, Message = "{EventId}: {Email} certified in {CourseId}")]
    public static partial void Certified(ILogger logger, string eventId, string email, string courseId);

    [LoggerMessage(Level = LogLevel.Information, Message = "{EventId}: {Email}'s Certificate expires soon")]
    public static partial void Expiring(ILogger logger, string eventId, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "{EventId}: not handled")]
    public static partial void Ignored(ILogger logger, string eventId);
}
