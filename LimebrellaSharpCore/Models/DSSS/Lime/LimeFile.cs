using System.Runtime.InteropServices;
using LimebrellaSharpCore.Helpers;
using static LimebrellaSharpCore.Helpers.LimeDeencryptor;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

public class LimeFile(LimeDeencryptor deencryptor, string filename = "")
{
    /// <summary>
    /// Extension of the <see cref="LimeFile"/>.
    /// </summary>
    public const string Extension = "bin";

    /// <summary>
    /// Name of the <see cref="LimeFile"/>.
    /// </summary>
    public string FileName { get; set; } = filename;

    /// <summary>
    /// Header of the <see cref="LimeFile"/>.
    /// </summary>
    public LimeHeader Header { get; set; } = new();

    /// <summary>
    /// The segments of the <see cref="LimeFile"/>.
    /// </summary>
    public LimeDataSegment[] Segments { get; set; } = [];

    /// <summary>
    /// Footer of <see cref="LimeFile"/>.
    /// </summary>
    public LimeFooter Footer { get; set; } = new();

    /// <summary>
    /// Deencryptor instance.
    /// </summary>
    public LimeDeencryptor Deencryptor { get; } = deencryptor;

    /// <summary>
    /// Stores the encryption state of the current file.
    /// </summary>
    public bool IsEncrypted { get; private set; }

    /// <summary>
    /// Stores the encryption state of the current file.
    /// </summary>
    public uint Key { get; set; }

    /// <summary>
    /// Loads the <paramref name="data"/> into the <see cref="LimeFile"/> object.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="encryptedFilesOnly"></param>
    /// <returns></returns>
    public BoolResult SetFileData(ReadOnlySpan<byte> data, bool encryptedFilesOnly = false)
    {
        try
        {
            // try to load the encrypted data
            var result = TrySetFileData(data);
            IsEncrypted = result.Result;
            if (result.Result) return result;

            // escape the function if only the encrypted data is needed
            if (encryptedFilesOnly) return result;
            
            // reset header and footer
            Header = new LimeHeader();
            Footer = new LimeFooter();

            // try to load decrypted data
            SetFileSegments(data);
            IsEncrypted = false;
            return new BoolResult(true);
        }
        catch { /* ignored */ }
        return new BoolResult(false, "Error on trying to open the file.");
    }

    /// <summary>
    /// Loads the <paramref name="data"/> into the <see cref="LimeFile"/> object asynchronously.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="encryptedFilesOnly"></param>
    /// <returns></returns>
    public async Task<BoolResult> SetFileDataAsync(ReadOnlyMemory<byte> data, bool encryptedFilesOnly = false)
        => await Task.Run(() => SetFileData(data.Span, encryptedFilesOnly));

    /// <summary>
    /// Tries to set the <paramref name="data"/> of a <see cref="LimeFile"/>.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private BoolResult TrySetFileData(ReadOnlySpan<byte> data)
    {
        using MemoryStream ms = new(data.ToArray());
        using BinaryReader br = new(ms);
        // HEADER
        try
        {
            // try to load header data into the Header
            Header = br.ReadStruct<LimeHeader>() ?? throw new NullReferenceException();
        }
        catch { return new BoolResult(false, "Invalid file header structure."); }

        // check Header integrity
        var test = Header.CheckIntegrity();
        if (!test.Result) return test;

        // SEGMENTS
        var segmentsLength = ms.Length - (Marshal.SizeOf<LimeHeader>() + Marshal.SizeOf<LimeFooter>());
        var segmentsCount = segmentsLength / Marshal.SizeOf<LimeDataSegment>();

        // overwrite Segments collection
        Segments = new LimeDataSegment[segmentsCount];
        for (var i = 0; i < segmentsCount; i++)
        {
            LimeDataSegment segment;
            try
            {
                segment = br.ReadStruct<LimeDataSegment>() ?? throw new NullReferenceException();
            }
            catch { return new BoolResult(false, $"Invalid DsssLimeDataSegment({i}) structure."); }
            Segments[i] = segment;
        }

        // check the integrity of the first Segment
        test = Segments.First().CheckIntegrity();
        if (!test.Result) return test;

        // FOOTER
        try
        {
            // try to load footer data into the Footer
            Footer = br.ReadStruct<LimeFooter>() ?? throw new NullReferenceException();
        }
        catch { return new BoolResult(false, "Invalid file footer structure."); }

        return new BoolResult(true);
    }

    /// <summary>
    /// Sets <see cref="Segments"/> of an existing object of a <see cref="LimeFile"/> type based on the <paramref name="data"/>.
    /// </summary>
    /// <param name="data"></param>
    private void SetFileSegments(ReadOnlySpan<byte> data)
    {
        var numberOfSegments = (int)Math.Ceiling((double)data.Length / LimeDataSegment.SegmentDataSize);
        Segments = new LimeDataSegment[numberOfSegments];

        using MemoryStream ms = new(data.ToArray());
        for (var i = 0; i < numberOfSegments; i++)
        {
            Segments[i] = new LimeDataSegment();
            // load data
            _ = ms.Read(Segments[i].SegmentData, 0, Segments[i].SegmentData.Length);
            // set default HashedKeyBanks
            for (var j = 0; j < Segments[i].HashedKeyBanks.Length; j++)
                Segments[i].HashedKeyBanks[j] = new LimeHashedKeyBank();
        }
        // save length of decrypted data
        Footer.DecryptedDataLength = data.Length;
    }

    /// <summary>
    /// Gets an existing object of a <see cref="LimeFile"/> type as byte array.
    /// </summary>
    /// <returns></returns>
    public ReadOnlySpan<byte> GetFileData()
    {
        // randomize footer salt
        Footer.GenerateSalt();

        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);
        // write DSSS HEADER content
        bw.WriteStruct(Header);
        // write DSSS SEGMENTS content
        foreach (var segment in Segments) bw.WriteStruct(segment);
        // write DSSS FOOTER content
        bw.WriteStruct(Footer);

        var dataAsBytes = ms.ToArray().AsSpan();
        var dataAsInts = MemoryMarshal.Cast<byte, uint>(dataAsBytes);

        // sign file
        SignFile(ref dataAsInts);

        // return data
        return dataAsBytes;
    }

    /// <summary>
    /// Gets an existing object of a <see cref="LimeFile"/> type as byte array asynchronously.
    /// </summary>
    /// <returns></returns>
    public async Task<ReadOnlyMemory<byte>> GetFileDataAsync()
    {
        return await Task.Run(() =>
        {
            // Randomize footer salt
            Footer.GenerateSalt();

            using MemoryStream ms = new();
            using BinaryWriter bw = new(ms);
            // write DSSS HEADER content
            bw.WriteStruct(Header);
            // write DSSS SEGMENTS content
            foreach (var segment in Segments) bw.WriteStruct(segment);
            // write DSSS FOOTER content
            bw.WriteStruct(Footer);

            var dataAsBytes = ms.ToArray().AsSpan();
            var dataAsInts = MemoryMarshal.Cast<byte, uint>(dataAsBytes);

            // sign file
            SignFile(ref dataAsInts);
            return dataAsBytes.ToArray().AsMemory();
        });
    }

    /// <summary>
    /// Gets all <see cref="Segments"/> of an existing object of a DsssLime type as Span&lt;byte&gt;.
    /// </summary>
    /// <returns></returns>
    public ReadOnlySpan<byte> GetFileSegments()
    {
        using MemoryStream ms = new();
        foreach (var segment in Segments) ms.Write(segment.SegmentData);
        ms.SetLength(Footer.DecryptedDataLength);
        return ms.ToArray();
    }

    /// <summary>
    /// Gets all <see cref="Segments"/> of an existing object of a DsssLime type as Span&lt;byte&gt; asynchronously.
    /// </summary>
    /// <returns></returns>
    public async Task<ReadOnlyMemory<byte>> GetFileSegmentsAsync()
    {
        using MemoryStream ms = new();
        foreach (var segment in Segments) await ms.WriteAsync(segment.SegmentData);
        ms.SetLength(Footer.DecryptedDataLength);
        return ms.ToArray().AsMemory();
    }
    
    /// <summary>
    /// Encrypts <see cref="Segments"/> asynchronously.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> EncryptSegmentsAsync(DeencryptDataAsyncDelegate deencryptAsyncDelegate)
    {
        var result = await Deencryptor.EncryptDataAsync(Segments, Key, deencryptAsyncDelegate);
        if (result) IsEncrypted ^= true;
        return result;
    }
    
    /// <summary>
    /// Decrypts <see cref="Segments"/> asynchronously.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> DecryptSegmentsAsync(DeencryptDataAsyncDelegate deencryptAsyncDelegate)
    {
        var result = await Deencryptor.DecryptDataAsync(Segments, Key, deencryptAsyncDelegate);
        if (result) IsEncrypted ^= true;
        return result;
    }

    /// <summary>
    /// Bruteforces the <paramref name="segmentIndex"/> of <see cref="Segments"/>.
    /// </summary>
    /// <param name="end"></param>
    /// <param name="segmentIndex"></param>
    /// <param name="id"></param>
    /// <param name="deencryptDelegate"></param>
    /// <param name="cts"></param>
    /// <param name="start"></param>
    /// <returns>Operation status and working <paramref name="id"/> if true.</returns>
    public bool BruteforceSegment(out uint id, DeencryptDataDelegate deencryptDelegate, CancellationTokenSource cts, uint start, uint end, uint segmentIndex = 0)
        => Deencryptor.LimepickSegmentBatch(deencryptDelegate, cts, Segments[segmentIndex], start, end, out id);

    /// <summary>
    /// Calculates MurmurHash3.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    public static uint Murmur3_32(ReadOnlySpan<uint> data, uint seed = 0)
    {
        const uint hash0 = 0x1B873593;
        const uint hash1 = 0xCC9E2D51;
        const uint hash2 = 0x052250EC;
        const uint hash3 = 0xC2B2AE35;
        const uint hash4 = 0x85EBCA6B;

        const byte rotation1 = 0xD;
        const byte rotation2 = 0xF;
        const byte shift1 = 0x10;

        var lengthInBytes = data.Length * sizeof(uint);

        foreach (var e in data)
            seed = 5 * (uint.RotateLeft((hash0 * uint.RotateLeft(hash1 * e, rotation2)) ^ seed, rotation1) - hash2);

        uint mod0 = 0;
        switch (lengthInBytes & 3)
        {
            case 3:
                mod0 = data[2] << shift1;
                goto case 2;
            case 2:
                mod0 ^= data[1] << 8;
                goto case 1;
            case 1:
                seed ^= hash0 * uint.RotateLeft(hash1 * (mod0 ^ data[0]), rotation2);
                break;
        }

        var basis = (uint)(lengthInBytes ^ seed);
        var hiWordOfBasis = (basis >> shift1) & 0xFFFF;

        return (hash3 * ((hash4 * (basis ^ hiWordOfBasis)) ^ ((hash4 * (basis ^ hiWordOfBasis)) >> rotation1))) ^ ((hash3 * ((hash4 * (basis ^ hiWordOfBasis)) ^ ((hash4 * (basis ^ hiWordOfBasis)) >> rotation1))) >> shift1);
    }

    /// <summary>
    /// This method signs a DSSS file.
    /// Thanks to windwakr (https://github.com/windwakr) for identifying this hashing method as MurmurHash3_32.
    /// </summary>
    /// <param name="fileData"></param>
    private static void SignFile(ref Span<uint> fileData)
        => fileData[^1] = Murmur3_32(fileData[..^1], 0xFFFFFFFF);
}