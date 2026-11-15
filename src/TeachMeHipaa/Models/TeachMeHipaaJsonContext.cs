using System.Text.Json.Serialization;

namespace TeachMeHipaa;

// Lists every model the services send or return. The generated model layer
// will own this file.
[JsonSerializable(typeof(Learner))]
[JsonSerializable(typeof(LearnerCreateOptions))]
[JsonSerializable(typeof(LearnerLaunchLink))]
[JsonSerializable(typeof(LearnerUpdateOptions))]
internal sealed partial class TeachMeHipaaJsonContext : JsonSerializerContext
{
}
