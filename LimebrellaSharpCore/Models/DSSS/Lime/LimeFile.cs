using LimebrellaSharpCore.Helpers;
using System.Runtime.InteropServices;
using static LimebrellaSharpCore.Helpers.LimeDeencryptor;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

public class LimeFile
{
    /// <summary>
    /// File extension of the <see cref="LimeFile"/>.
    /// </summary>
    public const string FileExtension = ".bin";
    
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
    /// Stores the encryption state of the current file.
    /// </summary>
    public bool IsEncrypted { get; private set; }

    /// <summary>
    /// Divides the specified <paramref name="data"/> into segments and initializes the <see cref="Segments"/> array.
    /// </summary>
    /// <param name="data">A read-only span of bytes containing the data to be segmented. The length of this span determines the number of
    /// segments created.</param>
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
    /// Attempts to parse and set the file header, data segments, and footer from the specified binary data buffer. Throws an exception if the data format is invalid.
    /// </summary>
    /// <param name="data">A read-only span of bytes containing the binary file data to be parsed.</param>
    /// <exception cref="InvalidDataException">Thrown if the file header, any data segment, or the file footer structure is invalid or cannot be parsed from the provided data.</exception>
    private void TrySetFileData(ReadOnlySpan<byte> data)
    {
        using MemoryStream ms = new(data.ToArray());
        using BinaryReader br = new(ms);

        // HEADER
        // try to load header data into the Header
        try { Header = br.ReadStruct<LimeHeader>() ?? throw new NullReferenceException(); }
        catch { throw new InvalidDataException("Invalid file header structure."); }
        // check the integrity of the Header (no need to rethrow the exception there)
        Header.CheckIntegrity();

        // SEGMENTS
        var segmentsLength = ms.Length - (Marshal.SizeOf<LimeHeader>() + Marshal.SizeOf<LimeFooter>());
        var segmentsCount = segmentsLength / Marshal.SizeOf<LimeDataSegment>();
        // overwrite Segments collection
        Segments = new LimeDataSegment[segmentsCount];
        for (var i = 0; i < segmentsCount; i++)
        {
            LimeDataSegment segment;
            try { segment = br.ReadStruct<LimeDataSegment>() ?? throw new NullReferenceException(); }
            catch { throw new InvalidDataException($"Invalid DataSegment[{i}] structure."); }
            Segments[i] = segment;
        }
        // check the integrity of the first segment (no need to rethrow the exception there)
        Segments.First().CheckIntegrity();

        // FOOTER
        // try to load footer data into the Footer
        try { Footer = br.ReadStruct<LimeFooter>() ?? throw new NullReferenceException(); }
        catch { throw new InvalidDataException("Invalid file footer structure."); }
    }

    /// <summary>
    /// Attempts to set the file data from the specified byte span, using encrypted format if possible.
    /// </summary>
    /// <param name="data">A read-only span of bytes containing the file data to be loaded. The data may be in encrypted or raw format.</param>
    /// <param name="encryptedFilesOnly">If set to <see langword="true"/>, only encrypted file data will be accepted; otherwise, raw file data will be loaded if encrypted data is not available.</param>
    public void SetFileData(ReadOnlySpan<byte> data, bool encryptedFilesOnly = false)
    {
        IsEncrypted = false;
        try
        {
            // try to load the encrypted data
            TrySetFileData(data);
            IsEncrypted = true;
        }
        catch
        {
            // escape the function if only the encrypted data is needed
            if (encryptedFilesOnly) return;
            
            // reset header and footer
            Header = new LimeHeader();
            Footer = new LimeFooter();

            // load raw data as segments
            SetFileSegments(data);
        }
    }

    /// <summary>
    /// Asynchronously sets the file data using the specified byte buffer, with an option to restrict the operation to encrypted files only.
    /// </summary>
    /// <param name="data">A read-only memory buffer containing the file data to be set.</param>
    /// <param name="encryptedFilesOnly">If <see langword="true"/>, the operation will only affect files that are encrypted; otherwise, all files are affected.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SetFileDataAsync(ReadOnlyMemory<byte> data, bool encryptedFilesOnly = false)
        => await Task.Run(() => SetFileData(data.Span, encryptedFilesOnly));
    
    /// <summary>
    /// Combines all file <see cref="Segments"/> into a single byte array representing the reconstructed file data.
    /// </summary>
    /// <returns>A byte array containing the concatenated data from all segments, trimmed to the decrypted data length specified in the footer.</returns>
    public byte[] GetFileSegments()
    {
        using MemoryStream ms = new();
        foreach (var segment in Segments) ms.Write(segment.SegmentData);
        ms.SetLength(Footer.DecryptedDataLength);
        return ms.ToArray();
    }

    /// <summary>
    /// Asynchronously combines all file <see cref="Segments"/> into a single byte array representing the reconstructed file data.
    /// </summary>
    /// <returns>A byte array containing the concatenated data from all segments, trimmed to the decrypted data length specified in the footer.</returns>
    public async Task<byte[]> GetFileSegmentsAsync()
    {
        using MemoryStream ms = new();
        foreach (var segment in Segments) await ms.WriteAsync(segment.SegmentData);
        ms.SetLength(Footer.DecryptedDataLength);
        return ms.ToArray();
    }

    /// <summary>
    /// Generates and returns the binary data representing the current file, including header, segments, and footer.
    /// </summary>
    /// <returns>A byte array containing the complete file data, including all headers, segments, and footer.</returns>
    public byte[] GetFileData()
    {
        // randomize footer salt
        Footer.GenerateSalt();
        // prepare memory stream and binary writer
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);
        // write DSSS HEADER content
        bw.WriteStruct(Header);
        // write DSSS SEGMENTS content
        foreach (var segment in Segments) bw.WriteStruct(segment);
        // write DSSS FOOTER content
        bw.WriteStruct(Footer);

        var dataAsBytes = ms.ToArray();
        var dataSpan = dataAsBytes.AsSpan();

        // sign file
        SignFile(ref dataSpan);

        // return data
        return dataAsBytes;
    }

    /// <summary>
    /// Asynchronously generates and returns the binary data representing the current file, including header, segments, and footer.
    /// </summary>
    /// <returns>A byte array containing the complete file data, including all headers, segments, and footer.</returns>
    public async Task<byte[]> GetFileDataAsync()
        => await Task.Run(GetFileData);

    /// <summary>
    /// Asynchronously decrypts all <see cref="Segments"/> for the specified user using the provided AES encryption platform.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose segments are to be decrypted.</param>
    /// <param name="po">Optional parallelization settings that control how the decryption operation is executed. If null, default parallel options are used.</param>
    /// <returns>A task that represents the asynchronous decryption operation.</returns>
    public async Task DecryptSegmentsAsync(ulong userId, ParallelOptions? po = null)
    {
        await DecryptDataAsync(Segments, userId, po);
        IsEncrypted = false;
    }

    /// <summary>
    /// Encrypts all data segments for the specified user using the provided AES encryption platform.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom the segments will be encrypted.</param>
    /// <param name="po">An optional ParallelOptions instance that configures the parallelization behavior of the encryption operation. If null, default parallelization settings are used.</param>
    /// <returns>A task that represents the asynchronous encryption operation.</returns>
    public async Task EncryptSegmentsAsync(ulong userId, ParallelOptions? po = null)
    {
        await EncryptDataAsync(Segments, userId, po);
        IsEncrypted = true;
    }

    /// <summary>
    /// Computes a 32-bit Murmur3 hash for the specified sequence of unsigned integers.
    /// </summary>
    /// <param name="data">The input data to hash, represented as a read-only span of 32-bit unsigned integers.</param>
    /// <param name="seed">An optional seed value to initialize the hash computation. Using different seeds produces different hash results for the same input data.</param>
    /// <returns>A 32-bit unsigned integer containing the computed Murmur3 hash of the input data.</returns>
    private static uint Murmur3_32(ReadOnlySpan<uint> data, uint seed = 0)
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
    /// Calculates and writes a Murmur3 hash signature to the end of the specified file data buffer.
    /// </summary>
    /// <remarks>Thanks to windwakr (https://github.com/windwakr) for identifying this hashing method as MurmurHash3_32.</remarks>
    /// <typeparam name="T">The value type of each element in the file data buffer.</typeparam>
    /// <param name="fileData">A span representing the file data to be signed. The signature will be written to the last element of this span.</param>
    private static void SignFile<T>(ref Span<T> fileData) where T : struct
    {
        var span = MemoryMarshal.Cast<T, uint>(fileData);
        span[^1] = Murmur3_32(span[..^1], 0xFFFFFFFF);
    }
}