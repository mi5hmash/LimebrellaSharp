using System.Runtime.InteropServices;
using LimebrellaSharpCore.Helpers;
using static LimebrellaSharpCore.Helpers.LimeDeencryptor;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

public class DsssLimeFile
{
    /// <summary>
    /// Header of the <see cref="DsssLimeFile"/>.
    /// </summary>
    public DsssLimeHeader LimeHeader { get; set; } = new();

    /// <summary>
    /// The segments of the <see cref="DsssLimeFile"/>.
    /// </summary>
    public DsssLimeDataSegment[] Segments { get; set; } = [];

    /// <summary>
    /// Footer of <see cref="DsssLimeFile"/>.
    /// </summary>
    public DsssLimeFooter Footer { get; set; } = new();
    
    /// <summary>
    /// Hashes needed to calculate the file checksum.
    /// </summary>
    private static uint[] Hashes { get; set; } = [];

    /// <summary>
    /// Deencryptor instance.
    /// </summary>
    public LimeDeencryptor Deencryptor { get; set; }

    /// <summary>
    /// Stores the encryption state of the current file.
    /// </summary>
    public bool IsEncrypted { get; private set; }

    /// <summary>
    /// Creates an empty <see cref="DsssLimeFile"/> class.
    /// </summary>
    /// /// <param name="deencryptor"></param>
    public DsssLimeFile(LimeDeencryptor deencryptor)
    {
        Deencryptor = deencryptor;
        Hashes = "OTMzNTg3MUI1MTJEOUVDQ0VDNTAyMjA1MzVBRUIyQzI2QkNBRUI4NQ==".Base64DecodeUtf8().ToUintArray();
    }

    /// <summary>
    /// Loads a '*.bin' archive of <see cref="DsssLimeFile"/> type into the existing object.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="encryptedFilesOnly"></param>
    /// <returns></returns>
    public BoolResult SetFileData(string filePath, bool encryptedFilesOnly = false)
    {
        // check if file exists 
        FileStream fs;
        try { fs = File.OpenRead(filePath); }
        catch { return new BoolResult(false, "Couldn't load the file. Error on trying to open the file."); }

        // try to load the encrypted file
        var result = TrySetFileData(fs);
        IsEncrypted = result.Result;

        // escape the function if only the encrypted files are needed
        if (encryptedFilesOnly) return result;

        // try to load decrypted file
        if (!IsEncrypted)
        {
            LimeHeader = new DsssLimeHeader();
            Footer = new DsssLimeFooter();
            try { fs = File.OpenRead(filePath); }
            catch { return new BoolResult(false, "Couldn't load the file. Error on trying to open the file."); }
            try
            {
                SetFileSegments(fs);
                result = new BoolResult(true);
            }
            catch { return new BoolResult(false, "Couldn't load the file. Error on trying to read the file."); }
        }

        return result;
    }

    /// <summary>
    /// Tries to set data of a <see cref="DsssLimeFile"/> type based on Stream <paramref name="fs"/>.
    /// </summary>
    /// <param name="fs"></param>
    /// <returns></returns>
    private BoolResult TrySetFileData(Stream fs)
    {
        using BinReader br = new(fs);
        try
        {
            // try to load header data into the LimeHeader
            LimeHeader = br.ReadStruct<DsssLimeHeader>() ?? throw new NullReferenceException();
        }
        catch { return new BoolResult(false, "Couldn't load the file. Invalid file header structure."); }

        var test = LimeHeader.CheckIntegrity();
        if (!test.Result) return new BoolResult(test.Result, $"Couldn't load the file. {test.Description}");

        var segmentsLength = fs.Length - (Marshal.SizeOf<DsssLimeHeader>() + Marshal.SizeOf<DsssLimeFooter>());
        var segmentsCount = segmentsLength / Marshal.SizeOf<DsssLimeDataSegment>();

        // overwrite Segments collection
        Segments = new DsssLimeDataSegment[segmentsCount];
        for (var i = 0; i < segmentsCount; i++)
        {
            DsssLimeDataSegment segment;
            try
            {
                segment = br.ReadStruct<DsssLimeDataSegment>() ?? throw new NullReferenceException();
            }
            catch { return new BoolResult(false, $"Couldn't load the file. Invalid DsssLimeDataSegment({i}) structure."); }
            Segments[i] = segment;
        }

        try
        {
            // try to load footer data into the Footer
            Footer = br.ReadStruct<DsssLimeFooter>() ?? throw new NullReferenceException();
        }
        catch { return new BoolResult(false, "Couldn't load the file. Invalid file footer structure."); }

        return new BoolResult(true);
    }

    /// <summary>
    /// Sets <see cref="Segments"/> of an existing object of a <see cref="DsssLimeFile"/> type based on Stream <paramref name="fs"/>.
    /// </summary>
    /// <param name="fs"></param>
    private void SetFileSegments(Stream fs)
    {
        var numberOfSegments = (int)Math.Ceiling((double)fs.Length / 0x1000);
        Segments = new DsssLimeDataSegment[numberOfSegments];

        for (var i = 0; i < numberOfSegments; i++)
        {
            Segments[i] = new DsssLimeDataSegment();
            // load data
            _ = fs.Read(Segments[i].SegmentData, 0, Segments[i].SegmentData.Length);
            // set default HashedKeyBanks
            for (var j = 0; j < Segments[i].HashedKeyBanks.Length; j++)
                Segments[i].HashedKeyBanks[j] = new DsssLimeHashedKeyBank();
        }

        // save length of decrypted data
        Footer.DecryptedDataLength = fs.Length;
    }

    /// <summary>
    /// Gets an existing object of a <see cref="DsssLimeFile"/> type as byte array.
    /// </summary>
    /// <returns></returns>
    public Span<byte> GetFileData()
    {
        // randomize footer salt
        Footer.GenerateSalt();

        using MemoryStream ms = new();
        using BinWriter bw = new(ms);
        // write DSSS HEADER content
        bw.WriteStruct(LimeHeader);
        // write DSSS SEGMENTS content
        foreach (var segment in Segments) bw.WriteStruct(segment);
        // write DSSS FOOTER content
        bw.WriteStruct(Footer);

        var dataAsBytes = ms.ToArray().AsSpan();
        var dataAsInts = MemoryMarshal.Cast<byte, uint>(dataAsBytes[..(dataAsBytes.Length / sizeof(uint) * sizeof(uint))]);

        // sign file
        SignFile(ref dataAsInts);

        // return data
        return dataAsBytes;
    }

    /// <summary>
    /// Gets all <see cref="Segments"/> of an existing object of a DsssLime type as Span&lt;byte&gt;.
    /// </summary>
    /// <returns></returns>
    public Span<byte> GetFileSegments()
    {
        using MemoryStream ms = new();
        foreach (var segment in Segments) ms.Write(segment.SegmentData);
        ms.SetLength(Footer.DecryptedDataLength);
        return ms.ToArray().AsSpan();
    }

    /// <summary>
    /// Encrypts <see cref="Segments"/>.
    /// </summary>
    /// <param name="steamId"></param>
    /// <returns></returns>
    public bool EncryptSegments(ulong steamId)
    {
        var result = Deencryptor.Limetree(Segments, steamId, Mode.Encrypt);
        if (result) IsEncrypted ^= true;
        return result;
    }

    /// <summary>
    /// Decrypts <see cref="Segments"/>.
    /// </summary>
    /// <param name="steamId"></param>
    /// <returns></returns>
    public bool DecryptSegments(ulong steamId)
    {
        var result = Deencryptor.Limetree(Segments, steamId, Mode.Decrypt);
        if (result) IsEncrypted ^= true;
        return result;
    }

    /// <summary>
    /// Buteforces the nth segment of <see cref="Segments"/>.
    /// </summary>
    /// <param name="steamId"></param>
    /// <param name="segmentIndex"></param>
    /// <returns></returns>
    public bool BruteforceSegment(ulong steamId, uint segmentIndex = 0)
        => Deencryptor.LimepickSegment(Segments[segmentIndex], steamId);

    /// <summary>
    /// This method signs a DSSS file.
    /// </summary>
    /// <param name="fileData"></param>
    private static void SignFile(ref Span<uint> fileData)
    {
        var hash0 = 0xFFFFFFFF;
        var length = fileData.Length - 1;
        var lengthInBytes = length * 4;

        for (var i = 0; i < length; i++)
            hash0 = 5 * (uint.RotateLeft((Hashes[0] * uint.RotateLeft(Hashes[1] * fileData[i], 15)) ^ hash0, 13) - Hashes[2]);

        uint mod0 = 0;
        switch (lengthInBytes & 3)
        {
            case 1:
                hash0 ^= Hashes[0] * uint.RotateLeft(Hashes[1] * (mod0 ^ fileData[0]), 15);
                break;
            case 2:
                mod0 ^= fileData[1] << 8;
                hash0 ^= Hashes[0] * uint.RotateLeft(Hashes[1] * (mod0 ^ fileData[0]), 15);
                break;
            case 3:
                mod0 = fileData[2] << 16;
                mod0 ^= fileData[1] << 8;
                hash0 ^= Hashes[0] * uint.RotateLeft(Hashes[1] * (mod0 ^ fileData[0]), 15);
                break;
        }
        var basis = (uint)(lengthInBytes ^ hash0);
        var hiWordOfBasis = (basis >> 16) & 0xFFFF;

        fileData[^1] = (Hashes[3] * ((Hashes[4] * (basis ^ hiWordOfBasis)) ^ ((Hashes[4] * (basis ^ hiWordOfBasis)) >> 13))) ^ ((Hashes[3] * ((Hashes[4] * (basis ^ hiWordOfBasis)) ^ ((Hashes[4] * (basis ^ hiWordOfBasis)) >> 13))) >> 16);
    }

    /// <summary>
    /// Returns false if file is not supported.
    /// </summary>
    /// <returns></returns>
    public BoolResult CheckCompatibility(ulong steamId)
    {
        if (!IsEncrypted) return new BoolResult(true);
        return !BruteforceSegment(steamId) ? new BoolResult(false, $"File was not encrypted with provided SteamID ({steamId}) and is not compatible.") : new BoolResult(true);
    }
}