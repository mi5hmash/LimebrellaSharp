using System.Runtime.InteropServices;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
public class LimeHeader
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
    /// Possible values: 2:None; 3:AutoStrong; ?:XOR; ?:BlowFish; 4:Citrus; 16:Lime|Mandarin; ?:RdsModule
    /// </summary>
    public uint EncryptionType { get; set; } = 0x10;

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown2 { get; set; }

    /// <summary>
    /// Create a parameter-less <see cref="LimeHeader"/>.
    /// </summary>
    public LimeHeader() { }

    /// <summary>
    /// Create a <see cref="LimeHeader"/> with given parameters.
    /// </summary>
    /// <param name="unknown1"></param>
    /// <param name="encryptionType"></param>
    /// <param name="unknown2"></param>
    public LimeHeader(uint unknown1, uint encryptionType, uint unknown2)
    {
        Unknown1 = unknown1;
        EncryptionType = encryptionType;
        Unknown2 = unknown2;
    }
    
    /// <summary>
    /// Checks if given <paramref name="fileFormat"/> makes sense.
    /// </summary>
    /// <param name="fileFormat"></param>
    /// <returns></returns>
    public BoolResult CheckFileFormat(uint? fileFormat = null)
    {
        fileFormat ??= FileFormat;
        return fileFormat != 0x5353_5344 ? new BoolResult(false, "Invalid file header!") : new BoolResult(true);
    }

    /// <summary>
    /// Checks if given <paramref name="encryptionType"/> makes sense.
    /// </summary>
    /// <param name="encryptionType"></param>
    /// <returns></returns>
    public BoolResult CheckEncryptionType(uint? encryptionType = null)
    {
        encryptionType ??= EncryptionType;
        return encryptionType != 0x10 ? new BoolResult(false, "Invalid file encryption type!") : new BoolResult(true);
    }

    /// <summary>
    /// Checks the integrity of <see cref="LimeHeader"/>.
    /// </summary>
    /// <returns></returns>
    public BoolResult CheckIntegrity()
    {
        var result = CheckFileFormat();
        if (!result.Result) return result;
        result = CheckEncryptionType();
        return result;
    }
}