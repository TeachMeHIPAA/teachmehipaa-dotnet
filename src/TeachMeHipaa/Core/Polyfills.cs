#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices;

// Lets init-only setters and records compile for netstandard2.0.
internal static class IsExternalInit
{
}
#endif
