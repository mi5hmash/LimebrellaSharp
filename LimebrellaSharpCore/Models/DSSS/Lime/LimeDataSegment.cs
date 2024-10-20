using System.Runtime.InteropServices;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x1220)]
public class LimeDataSegment
{
    public const int SegmentDataSize = 0x1000;
    public const int SegmentChecksumSize = 4;
    public const int HashedKeyBanksSize = 4;

    /// <summary>
    /// Banks containing parts of a hashed public key. 
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = HashedKeyBanksSize)]
    public LimeHashedKeyBank[] HashedKeyBanks = new LimeHashedKeyBank[HashedKeyBanksSize];
    
    /// <summary>
    /// Raw encrypted data stored in this segment. The size is one page (4096 bytes). 
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = SegmentDataSize)]
    public byte[] SegmentData = new byte[SegmentDataSize];

    /// <summary>
    /// Checksum of this segment.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = SegmentChecksumSize)]
    public ulong[] SegmentChecksum = new ulong[SegmentChecksumSize];

    /// <summary>
    /// Create a parameter-less <see cref="LimeDataSegment"/>.
    /// </summary>
    public LimeDataSegment() { }

    /// <summary>
    /// Create a <see cref="LimeDataSegment"/> with given parameters.
    /// </summary>
    /// <param name="segmentChecksum"></param>
    public LimeDataSegment(ulong[] segmentChecksum)
    {
        SegmentChecksum = segmentChecksum;
    }

    /// <summary>
    /// Sets a current segment checksum from a given ulong array.
    /// </summary>
    /// <param name="segmentChecksum"></param>
    public void SetSegmentChecksum(ulong[] segmentChecksum)
    {
        for (var i = 0; i < SegmentChecksumSize; i++) 
            SegmentChecksum[i] = segmentChecksum[i];
    }
    public void SetSegmentChecksum(ReadOnlySpan<ulong> segmentChecksum)
    {
        segmentChecksum[..SegmentChecksumSize].CopyTo(SegmentChecksum);
    }

    /// <summary>
    /// Checks <see cref="SegmentChecksum"/> against <paramref name="segmentChecksum"/>.
    /// </summary>
    /// <param name="segmentChecksum"></param>
    /// <returns></returns>
    public bool ValidateSegmentChecksum(ReadOnlySpan<ulong> segmentChecksum)
    {
        ReadOnlySpan<ulong> data = SegmentChecksum;
        return data.SequenceEqual(segmentChecksum[..SegmentChecksumSize]);
    }

    /// <summary>
    /// Returns false if the encryption version is newer than 1.
    /// </summary>
    /// <returns></returns>
    public BoolResult CheckIntegrity()
    {
        return HashedKeyBanks.First().Header.SequenceEqual<ulong>([0x5B49D502_17C839BB, 0x772BEEF5_D2441867, 0x6E236B07_6EEB11B8, 0x1216F542_E37CEE41, 0, 0, 0, 0]) 
            ? new BoolResult(true) : new BoolResult(false, "Invalid file encryption version (greater than 1)!");
    }
}