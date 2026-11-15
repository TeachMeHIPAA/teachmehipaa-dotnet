namespace TeachMeHipaa;

/// <summary>
/// An Operation was not done within <see cref="Operation.WaitAsync"/>'s
/// timeout. <see cref="Operation"/> is the last state seen; it may still
/// finish, so it can be waited on again.
/// </summary>
public class OperationTimeoutException : TeachMeHipaaException
{
    /// <summary>Creates an error about an Operation that is still running.</summary>
    /// <param name="message">How long was waited.</param>
    /// <param name="operation">The last state of the Operation seen.</param>
    public OperationTimeoutException(string message, Operation operation)
        : base(message)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        RequestId = operation.LastResponse?.RequestId;
        Headers = operation.LastResponse?.Headers ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>The last state of the Operation seen, not yet done.</summary>
    public Operation Operation { get; }
}
