using System.Runtime.InteropServices;
using LimebrellaSharpCore.Helpers;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

public class DsssLimeFile
{
    /// <summary>
    /// Header of DSSS file.
    /// </summary>
    public DsssHeader Header { get; set; } = new();

    /// <summary>
    /// The segments of the DSSSLime file.
    /// </summary>
    public DsssLimeDataSegment[] Segments { get; set; } = Array.Empty<DsssLimeDataSegment>();

    /// <summary>
    /// Footer of DSSSLime file.
    /// </summary>
    public DsssLimeFooter Footer { get; set; } = new();

    /// <summary>
    /// A path to DSSS '*.bin' archive.
    /// </summary>
    private string DsssPath { get; set; } = "";

    /// <summary>
    /// Hashes needed to calculate checksum.
    /// </summary>
    private static uint[] Hashes { get; set; } = Array.Empty<uint>();

    /// <summary>
    /// Create an empty DsssLimeFile class.
    /// </summary>
    public DsssLimeFile()
    {
        Hashes = "OTMzNTg3MUI1MTJEOUVDQ0VDNTAyMjA1MzVBRUIyQzI2QkNBRUI4NQ==".Base64DecodeUtf8().ToUintArray();
    }

    /// <summary>
    /// Load a '*.bin' archive of DsssLime type into the existing object.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public BoolResult LoadFile(string filePath)
    {
        DsssPath = filePath;
        FileStream stream;
        try
        {
            stream = File.OpenRead(DsssPath);
        }
        catch { return new BoolResult(false, "Couldn't load the file. Error on trying to open the file."); }

        using BinReader br = new(stream);
        try
        {
            // try to load header data into the Header
            Header = br.ReadStruct<DsssHeader>() ?? throw new NullReferenceException();
        }
        catch { return new BoolResult(false, "Couldn't load the file. Invalid file header structure."); }

        var test = Header.CheckIntegrity();
        if (!test.Result) return new BoolResult(test.Result, $"Couldn't load the file. {test.Description}");

        var segmentsLength = stream.Length - (Marshal.SizeOf<DsssHeader>() + Marshal.SizeOf<DsssLimeFooter>());
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
    /// Save an existing object of a DsssLime type as a new '*.bin' archive.
    /// </summary>
    /// <param name="filePath"></param>
    public void SaveFile(string filePath)
    {
        // randomize footer salt
        Footer.GenerateSalt();

        using MemoryStream ms = new();
        using BinWriter bw = new(ms);
        // write DSSS HEADER content
        bw.WriteStruct(Header);
        // write DSSS SEGMENTS content
        foreach (var segment in Segments) bw.WriteStruct(segment);
        // write DSSS FOOTER content
        bw.WriteStruct(Footer);

        var dataAsBytes = ms.ToArray().AsSpan();
        var dataAsInts = MemoryMarshal.Cast<byte, uint>(dataAsBytes[..(dataAsBytes.Length / sizeof(uint) * sizeof(uint))]);

        // sign file
        SignFile(ref dataAsInts);

        // save file
        File.WriteAllBytes(filePath, dataAsBytes.ToArray());
    }

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
}