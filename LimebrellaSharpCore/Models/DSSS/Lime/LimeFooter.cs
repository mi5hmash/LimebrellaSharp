using System.Runtime.InteropServices;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x8C)]
public class LimeFooter
{
    public const int SaltSize = 0x80;

    /// <summary>
    /// A block of random bytes.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = SaltSize)]
    public byte[] Salt = new byte[SaltSize];

    /// <summary>
    /// A length of a decrypted data in bytes.
    /// </summary>
    public long DecryptedDataLength { get; set; } = 0x4853_414D_4835_494D;
    
    /// <summary>
    /// A file signature.
    /// </summary>
    public uint Signature { get; set; } = 0x4835_494D;

    /// <summary>
    /// Create a parameter-less <see cref="LimeFooter"/>.
    /// </summary>
    public LimeFooter() { }

    /// <summary>
    /// Initializes a new instance of the LimeFooter class with the specified decrypted data length and signature.
    /// </summary>
    /// <param name="decryptedDataLength">The length, in bytes, of the decrypted data represented by this footer.</param>
    /// <param name="signature">The file signature.</param>
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