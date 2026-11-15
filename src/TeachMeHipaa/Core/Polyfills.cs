#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    // Lets init-only setters and records compile for netstandard2.0.
    internal static class IsExternalInit
    {
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    // Gives netstandard2.0 callers the same nullability flow as net8.0 ones;
    // the compiler matches the attribute by name.
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    internal sealed class MemberNotNullWhenAttribute : Attribute
    {
        public MemberNotNullWhenAttribute(bool returnValue, string member)
        {
            ReturnValue = returnValue;
            Members = [member];
        }

        public bool ReturnValue { get; }

        public string[] Members { get; }
    }
}
#endif
