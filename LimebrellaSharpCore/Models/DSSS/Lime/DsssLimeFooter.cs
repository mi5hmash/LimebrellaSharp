using System.Runtime.InteropServices;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x8C)]
public class DsssLimeFooter
{
    /// <summary>
    /// A length of a decrypted data in bytes.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x80)]
    public byte[] Salt = new byte[0x80];

    /// <summary>
    /// A length of a decrypted data in bytes.
    /// </summary>
    public long DecryptedDataLength { get; set; } = 0x0000_0000_0056_3412;
    
    /// <summary>
    /// A file signature.
    /// </summary>
    public uint Signature { get; set; } = 0x7856_3412;

    /// <summary>
    /// Create a parameter-less <see cref="DsssLimeFooter"/>.
    /// </summary>
    public DsssLimeFooter() { }

    /// <summary>
    /// Create a <see cref="DsssLimeFooter"/> with given parameters.
    /// </summary>
    /// <param name="decryptedDataLength"></param>
    /// <param name="signature"></param>
    public DsssLimeFooter(long decryptedDataLength, uint signature)
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