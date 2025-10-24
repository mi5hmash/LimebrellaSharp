using System.Runtime.InteropServices;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x80)]
public struct LimeHashedKeyBank
{
    public const int HeaderSize = 8;
    public const int KeyFragmentSize = 8;

    /// <summary>
    /// Gets or sets the header data as an array of unsigned 64-bit integers.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = HeaderSize)]
    public ulong[] Header = new ulong[8];

    /// <summary>
    /// Holds the key fragment as an array of unsigned 64-bit integers, with a fixed size defined by <see cref="KeyFragmentSize"/>.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = KeyFragmentSize)]
    public ulong[] KeyFragment = new ulong[8];

    /// <summary>
    /// Create a parameter-less <see cref="LimeHashedKeyBank"/>.
    /// </summary>
    public LimeHashedKeyBank() { }
    
    /// <summary>
    /// Sets random <see cref="Header"/>.
    /// </summary>
    public readonly void SetRandomHeader() => SetHeader(RandomUlongArray(4));
    
    /// <summary>
    /// Sets random <see cref="KeyFragment"/>.
    /// </summary>
    public readonly void SetRandomKey() => SetKey(RandomUlongArray(5));

    /// <summary>
    /// Sets the <see cref="Header"/> by copying the contents of the specified span into the current header.
    /// </summary>
    /// <param name="header">A span containing the header to copy.</param>
    public readonly void SetHeader(Span<ulong> header)
    {
        Span<ulong> spanHeader = Header;
        spanHeader.Clear();
        header.CopyTo(spanHeader);
    }

    /// <summary>
    /// Sets the <see cref="KeyFragment"/> to the values provided in the specified span.
    /// </summary>
    /// <param name="key">A span containing the key values to copy into the key fragment.</param>
    public readonly void SetKey(Span<ulong> key)
    {
        Span<ulong> spanKeyFragment = KeyFragment;
        spanKeyFragment.Clear();
        key.CopyTo(spanKeyFragment);
    }
    
    /// <summary>
    /// Generates an array of random 64-bit unsigned integers of the specified length.
    /// </summary>
    /// <param name="length">The number of random 64-bit unsigned integers to generate.</param>
    /// <returns>An array of randomly generated 64-bit unsigned integers.</returns>
    private static ulong[] RandomUlongArray(int length)
    {
        const int size = sizeof(ulong);
        var uLongs = new ulong[length];
        var randomBytes = new byte[size * uLongs.Length];
        Random random = new();
        random.NextBytes(randomBytes);
        for (var i = 0; i < length; i++)
        {
            var startIndex = i * size;
            uLongs[i] = BitConverter.ToUInt64(randomBytes, startIndex);
        }
        return uLongs;
    }
}