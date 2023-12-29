using System.Runtime.InteropServices;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x1220)]
public class DsssLimeDataSegment
{
    /// <summary>
    /// Banks containing parts of a hashed public key, as of version 1 there are 4 of them. 
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public DsssLimeHashedKeyBank[] HashedKeyBanks = new DsssLimeHashedKeyBank[4];
    
    /// <summary>
    /// Raw encrypted data stored in this segment. The size is one page (4096 bytes). 
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1000)]
    public byte[] SegmentData = new byte[0x1000];

    /// <summary>
    /// Checksum of this segment.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public ulong[] SegmentChecksum = new ulong[4];

    /// <summary>
    /// Create a parameter-less <see cref="DsssLimeDataSegment"/>.
    /// </summary>
    public DsssLimeDataSegment() { }

    /// <summary>
    /// Create a <see cref="DsssLimeDataSegment"/> with given parameters.
    /// </summary>
    /// <param name="segmentChecksum"></param>
    public DsssLimeDataSegment(ulong[] segmentChecksum)
    {
        SegmentChecksum = segmentChecksum;
    }

    /// <summary>
    /// Sets a current segment checksum from a given ulong array.
    /// </summary>
    /// <param name="segmentChecksum"></param>
    public void SetSegmentChecksum(ulong[] segmentChecksum)
    {
        for (var i = 0; i < SegmentChecksum.Length; i++) SegmentChecksum[i] = segmentChecksum[i];
    }
    public void SetSegmentChecksum(ReadOnlySpan<ulong> segmentChecksum)
    {
        segmentChecksum[..4].CopyTo(SegmentChecksum);
    }

    /// <summary>
    /// Checks <see cref="SegmentChecksum"/> against <paramref name="segmentChecksum"/>.
    /// </summary>
    /// <param name="segmentChecksum"></param>
    /// <returns></returns>
    public bool ValidateSegmentChecksum(ReadOnlySpan<ulong> segmentChecksum)
    {
        ReadOnlySpan<ulong> data = SegmentChecksum;
        return data.SequenceEqual(segmentChecksum[..SegmentChecksum.Length]);
    }
}