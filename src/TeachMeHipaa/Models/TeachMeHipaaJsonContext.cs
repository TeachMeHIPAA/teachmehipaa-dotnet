using System.Text.Json.Serialization;

namespace TeachMeHipaa;

// Lists every model the services return. The generated model layer will own
// this file.
[JsonSerializable(typeof(Learner))]
internal sealed partial class TeachMeHipaaJsonContext : JsonSerializerContext
{
}
