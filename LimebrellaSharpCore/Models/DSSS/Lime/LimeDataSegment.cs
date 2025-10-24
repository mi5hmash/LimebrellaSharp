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
    /// Initializes a new instance of the LimeDataSegment class with the specified segment checksums.
    /// </summary>
    /// <param name="segmentChecksum">An array of unsigned 64-bit integers representing the checksums for each segment.</param>
    public LimeDataSegment(ulong[] segmentChecksum)
    {
        SegmentChecksum = segmentChecksum;
    }

    /// <summary>
    /// Sets the checksum values for each segment using the specified array.
    /// </summary>
    /// <param name="segmentChecksum">An array of unsigned 64-bit integers representing the checksum values to assign to each segment.</param>
    public void SetSegmentChecksum(ulong[] segmentChecksum)
    {
        for (var i = 0; i < SegmentChecksumSize; i++) 
            SegmentChecksum[i] = segmentChecksum[i];
    }

    /// <summary>
    /// Sets the segment checksum values from the specified span.
    /// </summary>
    /// <param name="segmentChecksum">A read-only span containing the checksum values to copy.</param>
    public void SetSegmentChecksum(ReadOnlySpan<ulong> segmentChecksum)
    {
        segmentChecksum[..SegmentChecksumSize].CopyTo(SegmentChecksum);
    }

    /// <summary>
    /// Determines whether the specified segment checksum matches the stored segment checksum value.
    /// </summary>
    /// <param name="segmentChecksum">A read-only span of 64-bit unsigned integers representing the segment checksum to validate.</param>
    /// <returns><see langword="true"/> if the specified segment checksum matches the stored value; otherwise, <see langword="false"/>.</returns>
    public bool ValidateSegmentChecksum(ReadOnlySpan<ulong> segmentChecksum)
    {
        ReadOnlySpan<ulong> data = SegmentChecksum;
        return data.SequenceEqual(segmentChecksum[..SegmentChecksumSize]);
    }

    /// <summary>
    /// Asynchronously validates the checksum of a data segment.
    /// </summary>
    /// <param name="segmentChecksum">A read-only memory buffer containing the checksum bytes to validate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the checksum is valid; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> ValidateSegmentChecksumAsync(ReadOnlyMemory<byte> segmentChecksum)
    {
        return await Task.Run(() =>
        {
            var segmentChecksumAsUlongs = MemoryMarshal.Cast<byte, ulong>(segmentChecksum.Span);
            return ValidateSegmentChecksum(segmentChecksumAsUlongs);
        });
    }

    /// <summary>
    /// Validates the integrity of the segment header in the first hashed key bank.
    /// </summary>
    /// <exception cref="InvalidDataException">Thrown if the segment header matches the expected invalid sequence, indicating a corrupted or invalid segment.</exception>
    public void CheckIntegrity()
    {
        var result = HashedKeyBanks.First().Header.SequenceEqual<ulong>([0x5B49D502_17C839BB, 0x772BEEF5_D2441867, 0x6E236B07_6EEB11B8, 0x1216F542_E37CEE41, 0, 0, 0, 0]);
        if (!result) throw new InvalidDataException("Invalid segment header.");
    }
}