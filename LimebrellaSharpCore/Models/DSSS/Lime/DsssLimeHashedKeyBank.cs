using System.Runtime.InteropServices;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x80)]
public struct DsssLimeHashedKeyBank
{
    /// <summary>
    /// KeyBank header made of 8 ulong segments, but as of version 1 only the first 4 segments are occupied.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public ulong[] Header = new ulong[8];

    /// <summary>
    /// HashedKey
    /// Key made of 8 ulong segments, but as of version 1 only the first 5 segments are occupied.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public ulong[] HashedKey = new ulong[8];

    /// <summary>
    /// Create a parameter-less <see cref="DsssLimeHashedKeyBank"/>.
    /// </summary>
    public DsssLimeHashedKeyBank()
    {
        Init();
    }

    /// <summary>
    /// Initialize struct. 
    /// </summary>
    public void Init()
    {
        SetDefaultHeader();
        SetDefaultKey();
    }

    /// <summary>
    /// Returns a default <see cref="Header"/> value.
    /// </summary>
    /// <returns></returns>
    public void SetDefaultHeader() => Header = [0x1111_1111_1111_1111, 0x1111_1111_1111_1111, 0x1111_1111_1111_1111, 0x1111_1111_1111_1111, 0x0, 0x0, 0x0, 0x0
    ];

    /// <summary>
    /// Returns a default <see cref="HashedKey"/> value.
    /// </summary>
    /// <returns></returns>
    public void SetDefaultKey() => HashedKey = [0x1111_1111_1111_1111, 0x1111_1111_1111_1111, 0x1111_1111_1111_1111, 0x1111_1111_1111_1111, 0x1111_1111_1111_1111, 0x0, 0x0, 0x0
    ];

    /// <summary>
    /// Sets random <see cref="Header"/>.
    /// </summary>
    public readonly void SetRandomHeader() => SetHeader(RandomUlongArray(4));

    /// <summary>
    /// Sets random <see cref="HashedKey"/>.
    /// </summary>
    public readonly void SetRandomKey() => SetKey(RandomUlongArray(5));

    /// <summary>
    /// Sets <see cref="Header"/>.
    /// </summary>
    /// <param name="header"></param>
    public readonly void SetHeader(Span<ulong> header)
    {
        Span<ulong> spanHeader = Header;
        spanHeader.Clear();
        header.CopyTo(spanHeader);
    }

    /// <summary>
    /// Sets <see cref="HashedKey"/>.
    /// </summary>
    /// <param name="key"></param>
    public readonly void SetKey(Span<ulong> key)
    {
        Span<ulong> spanHashedKey = HashedKey;
        spanHashedKey.Clear();
        key.CopyTo(spanHashedKey);
    }
    
    /// <summary>
    /// Creates a random ulong array of provided length.
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
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