using System.Net;

namespace TeachMeHipaa;

/// <summary>
/// An Operation finished with an error. The problem's fields are on the
/// exception, and <see cref="Operation"/> is the failed Operation.
/// </summary>
public class OperationFailedException : TeachMeHipaaException
{
    /// <summary>Creates an error about a failed Operation.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="operation">The failed Operation.</param>
    public OperationFailedException(string message, Operation operation)
        : base(message) =>
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));

    /// <summary>The failed Operation, with its <see cref="Operation.Error"/>.</summary>
    public Operation Operation { get; }

    internal static OperationFailedException From(Operation operation, Problem problem)
    {
        var code = problem.Code.Value;
        var message = problem.Detail is { Length: > 0 } detail ? detail
            : problem.Title is { Length: > 0 } title ? title
            : code.Length > 0 ? code
            : $"Operation {operation.Name} failed.";
        return new OperationFailedException(message, operation)
        {
            StatusCode = problem.Status > 0 ? (HttpStatusCode)problem.Status : null,
            Code = code.Length > 0 ? code : null,
            Title = problem.Title.Length > 0 ? problem.Title : null,
            Detail = problem.Detail,
            Errors = problem.Errors?.Select(error => new FieldError(error.Path, error.Message)).ToList() ?? [],
            RequestId = operation.LastResponse?.RequestId,
            Headers = operation.LastResponse?.Headers ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
        };
    }
}
