using System.Runtime.InteropServices;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
public class LimeHeader
{
    private const uint ExpectedMagicNumber = 0x5353_5344;
    private const uint ExpectedEncryptionType = 0x10;

    /// <summary>
    /// Gets or sets the magic number used to identify or validate the data format.
    /// </summary>
    public uint MagicNumber { get; set; } = ExpectedMagicNumber;

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown1 { get; set; } = 0x2;

    /// <summary>
    /// Save File encryption type.
    /// Possible values: 2:None; 3:AutoStrong; ?:XOR; ?:BlowFish; 4:Citrus; 16:Lime|Mandarin; ?:RdsModule
    /// </summary>
    public uint EncryptionType { get; set; } = ExpectedEncryptionType;

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
    /// Determines whether the specified magic number does not match the expected value.
    /// </summary>
    /// <param name="magicNumber">The magic number to check.</param>
    /// <returns><see langword="true"/> if the magic number does not equal the expected value; otherwise, <see langword="false"/>.</returns>
    public bool CheckMagicNumber(uint? magicNumber = null)
    {
        magicNumber ??= MagicNumber;
        return magicNumber == ExpectedMagicNumber;
    }

    /// <summary>
    /// Determines whether the specified encryption type is not equal to Lime encryption type.
    /// </summary>
    /// <param name="encryptionType">The encryption type to check.</param>
    /// <returns><see langword="true"/> if the encryption type is not Lime encryption type; otherwise, <see langword="false"/>.</returns>
    public bool CheckEncryptionType(uint? encryptionType = null)
    {
        encryptionType ??= EncryptionType;
        return encryptionType == ExpectedEncryptionType;
    }

    /// <summary>
    /// Checks the integrity of the file by validating its magic number and encryption type.
    /// </summary>
    /// <exception cref="InvalidDataException">Thrown if the file's magic number or encryption type is invalid.</exception>
    public void CheckIntegrity()
    {
        var result = CheckMagicNumber();
        if (!result) throw new InvalidDataException("Invalid file magic number.");
        result = CheckEncryptionType();
        if (!result) throw new InvalidDataException("Invalid file encryption type.");
    }
}