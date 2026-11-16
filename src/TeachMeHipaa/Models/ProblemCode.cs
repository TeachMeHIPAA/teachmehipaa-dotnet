// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>A machine-readable error code, such as <c>seat_unavailable</c>.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(ProblemCodeJsonConverter))]
public readonly record struct ProblemCode
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public ProblemCode(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>validation_failed</c>.</summary>
    public static ProblemCode ValidationFailed { get; } = new("validation_failed");

    /// <summary><c>unauthorized</c>.</summary>
    public static ProblemCode Unauthorized { get; } = new("unauthorized");

    /// <summary><c>forbidden</c>.</summary>
    public static ProblemCode Forbidden { get; } = new("forbidden");

    /// <summary><c>not_found</c>.</summary>
    public static ProblemCode NotFound { get; } = new("not_found");

    /// <summary><c>method_not_allowed</c>.</summary>
    public static ProblemCode MethodNotAllowed { get; } = new("method_not_allowed");

    /// <summary><c>not_acceptable</c>.</summary>
    public static ProblemCode NotAcceptable { get; } = new("not_acceptable");

    /// <summary><c>conflict</c>.</summary>
    public static ProblemCode Conflict { get; } = new("conflict");

    /// <summary><c>rate_limited</c>.</summary>
    public static ProblemCode RateLimited { get; } = new("rate_limited");

    /// <summary><c>precondition_required</c>.</summary>
    public static ProblemCode PreconditionRequired { get; } = new("precondition_required");

    /// <summary><c>precondition_failed</c>.</summary>
    public static ProblemCode PreconditionFailed { get; } = new("precondition_failed");

    /// <summary><c>idempotency_key_reuse</c>.</summary>
    public static ProblemCode IdempotencyKeyReuse { get; } = new("idempotency_key_reuse");

    /// <summary><c>unsupported_media_type</c>.</summary>
    public static ProblemCode UnsupportedMediaType { get; } = new("unsupported_media_type");

    /// <summary><c>illegal_transition</c>.</summary>
    public static ProblemCode IllegalTransition { get; } = new("illegal_transition");

    /// <summary><c>seat_unavailable</c>.</summary>
    public static ProblemCode SeatUnavailable { get; } = new("seat_unavailable");

    /// <summary><c>seat_already_claimed</c>.</summary>
    public static ProblemCode SeatAlreadyClaimed { get; } = new("seat_already_claimed");

    /// <summary><c>already_invited</c>.</summary>
    public static ProblemCode AlreadyInvited { get; } = new("already_invited");

    /// <summary><c>email_already_registered</c>.</summary>
    public static ProblemCode EmailAlreadyRegistered { get; } = new("email_already_registered");

    /// <summary><c>last_admin_removal_blocked</c>.</summary>
    public static ProblemCode LastAdminRemovalBlocked { get; } = new("last_admin_removal_blocked");

    /// <summary><c>org_not_active</c>.</summary>
    public static ProblemCode OrgNotActive { get; } = new("org_not_active");

    /// <summary><c>enrollment_archived</c>.</summary>
    public static ProblemCode EnrollmentArchived { get; } = new("enrollment_archived");

    /// <summary><c>lessons_incomplete</c>.</summary>
    public static ProblemCode LessonsIncomplete { get; } = new("lessons_incomplete");

    /// <summary><c>batch_too_large</c>.</summary>
    public static ProblemCode BatchTooLarge { get; } = new("batch_too_large");

    /// <summary><c>import_too_large</c>.</summary>
    public static ProblemCode ImportTooLarge { get; } = new("import_too_large");

    /// <summary><c>export_too_large</c>.</summary>
    public static ProblemCode ExportTooLarge { get; } = new("export_too_large");

    /// <summary><c>summary_too_large</c>.</summary>
    public static ProblemCode SummaryTooLarge { get; } = new("summary_too_large");

    /// <summary><c>seat_not_refundable</c>.</summary>
    public static ProblemCode SeatNotRefundable { get; } = new("seat_not_refundable");

    /// <summary><c>refund_failed</c>.</summary>
    public static ProblemCode RefundFailed { get; } = new("refund_failed");

    /// <summary><c>not_configured</c>.</summary>
    public static ProblemCode NotConfigured { get; } = new("not_configured");

    /// <summary><c>reset_unavailable</c>.</summary>
    public static ProblemCode ResetUnavailable { get; } = new("reset_unavailable");

    /// <summary><c>auto_recertify_unavailable</c>.</summary>
    public static ProblemCode AutoRecertifyUnavailable { get; } = new("auto_recertify_unavailable");

    /// <summary><c>api_client_limit_reached</c>.</summary>
    public static ProblemCode ApiClientLimitReached { get; } = new("api_client_limit_reached");

    /// <summary><c>external_id_conflict</c>.</summary>
    public static ProblemCode ExternalIdConflict { get; } = new("external_id_conflict");

    /// <summary><c>cannot_offboard_admin</c>.</summary>
    public static ProblemCode CannotOffboardAdmin { get; } = new("cannot_offboard_admin");

    /// <summary><c>cannot_launch_admin</c>.</summary>
    public static ProblemCode CannotLaunchAdmin { get; } = new("cannot_launch_admin");

    /// <summary><c>learner_removed</c>.</summary>
    public static ProblemCode LearnerRemoved { get; } = new("learner_removed");

    /// <summary><c>learner_not_accepted</c>.</summary>
    public static ProblemCode LearnerNotAccepted { get; } = new("learner_not_accepted");

    /// <summary><c>cannot_rename_admin</c>.</summary>
    public static ProblemCode CannotRenameAdmin { get; } = new("cannot_rename_admin");

    /// <summary><c>learner_in_multiple_orgs</c>.</summary>
    public static ProblemCode LearnerInMultipleOrgs { get; } = new("learner_in_multiple_orgs");

    /// <summary><c>account_exists_use_sign_in</c>.</summary>
    public static ProblemCode AccountExistsUseSignIn { get; } = new("account_exists_use_sign_in");

    /// <summary><c>name_change_not_allowed</c>.</summary>
    public static ProblemCode NameChangeNotAllowed { get; } = new("name_change_not_allowed");

    /// <summary><c>temporarily_unavailable</c>.</summary>
    public static ProblemCode TemporarilyUnavailable { get; } = new("temporarily_unavailable");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class ProblemCodeJsonConverter : JsonConverter<ProblemCode>
{
    public override ProblemCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A ProblemCode must be a string."));

    public override void Write(Utf8JsonWriter writer, ProblemCode value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
