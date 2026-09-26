using System;
using System.Runtime.CompilerServices;

namespace Everware.Common;

public readonly ref struct StackOverride<T> : IDisposable
{
    private readonly ref T reference;
    private readonly T oldValue;

    public StackOverride(ref T reference, T value)
    {
        this.reference = ref reference;
        oldValue = reference;
        reference = value;
    }

    public void Dispose()
    {
        reference = oldValue;
    }
}

public static class StackOverrideExtensions
{
    extension<T>(ref T reference)
        where T : struct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackOverride<T> Override(T value)
        {
            return new StackOverride<T>(ref reference, value);
        }
    }
}
