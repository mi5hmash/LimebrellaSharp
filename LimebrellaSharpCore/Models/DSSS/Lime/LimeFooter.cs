using System.Runtime.InteropServices;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x8C)]
public class LimeFooter
{
    public const int SaltSize = 0x80;

    /// <summary>
    /// A length of a decrypted data in bytes.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = SaltSize)]
    public byte[] Salt = new byte[SaltSize];

    /// <summary>
    /// A length of a decrypted data in bytes.
    /// </summary>
    public long DecryptedDataLength { get; set; } = 0x0000_0000_0056_3412;
    
    /// <summary>
    /// A file signature.
    /// </summary>
    public uint Signature { get; set; } = 0x7856_3412;

    /// <summary>
    /// Create a parameter-less <see cref="LimeFooter"/>.
    /// </summary>
    public LimeFooter() { }

    /// <summary>
    /// Create a <see cref="LimeFooter"/> with given parameters.
    /// </summary>
    /// <param name="decryptedDataLength"></param>
    /// <param name="signature"></param>
    public LimeFooter(long decryptedDataLength, uint signature)
    {
        DecryptedDataLength = decryptedDataLength;
        Signature = signature;
    }

    /// <summary>
    /// Generates random salt.
    /// </summary>
    public void GenerateSalt()
    {
        Random random = new();
        for (var i = 0; i < Salt.Length; i++) Salt[i] = (byte)random.Next(byte.MaxValue + 1);
    }
}