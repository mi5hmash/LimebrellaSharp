using System.Runtime.InteropServices;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
public class DsssLimeHeader
{
    /// <summary>
    /// This should be the ANSI for "DSSS", or 0x5353_5344.
    /// </summary>
    public uint FileFormat { get; set; } = 0x5353_5344;

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown1 { get; set; } = 0x2;

    /// <summary>
    /// Save File encryption type.
    /// Possible values: 2:None; 3:AutoStrong; ?:XOR; ?:BlowFish; 4:Citrus; 16:Lime; ?:RdsModule
    /// </summary>
    public uint EncryptionType { get; set; } = 0x10;

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown2 { get; set; }

    /// <summary>
    /// Create a parameter-less <see cref="DsssLimeHeader"/>.
    /// </summary>
    public DsssLimeHeader() { }

    /// <summary>
    /// Create a <see cref="DsssLimeHeader"/> with given parameters.
    /// </summary>
    /// <param name="unknown1"></param>
    /// <param name="encryptionType"></param>
    /// <param name="unknown2"></param>
    public DsssLimeHeader(uint unknown1, uint encryptionType, uint unknown2)
    {
        Unknown1 = unknown1;
        EncryptionType = encryptionType;
        Unknown2 = unknown2;
    }

    /// <summary>
    /// Returns false if its <see cref="FileFormat"/> does not make sense or <see cref="EncryptionType"/> is not supported.
    /// </summary>
    /// <returns></returns>
    public BoolResult CheckIntegrity()
    {
        if (FileFormat != 0x5353_5344) return new BoolResult(false, "Invalid file header!");
        if (EncryptionType != 0x10) return new BoolResult(false, "Invalid file encryption type!");
        return new BoolResult(true);
    }
}