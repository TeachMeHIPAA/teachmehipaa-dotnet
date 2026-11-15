using System.Text.Json.Serialization;

namespace TeachMeHipaa;

// The core's wire types live in their own context: the source generator can't
// merge [JsonSerializable] lists spread over partial declarations, and the
// generated model layer owns TeachMeHipaaJsonContext.
[JsonSerializable(typeof(TokenEndpointResponse))]
[JsonSerializable(typeof(TokenEndpointError))]
[JsonSerializable(typeof(OrganizationIntrospection))]
[JsonSerializable(typeof(ProblemDocument))]
[JsonSerializable(typeof(WebhookDeliveryResult))]
[JsonSerializable(typeof(WebhookDeliverySent))]
[JsonSerializable(typeof(WebhookDeliveryFailed))]
[JsonSerializable(typeof(WebhookDeliverySettled))]
[JsonSerializable(typeof(UnknownWebhookDeliveryResult))]
internal sealed partial class CoreJsonContext : JsonSerializerContext
{
}
